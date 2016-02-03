  
//****************************************************************************
//  NAME: AbstractDatabase.cs                                                          
//****************************************************************************
//                                                                            
//  Description:                                                              
//    Implements the common base functionality for all IDatabase implementations.
//                                                                            
//                                                                            
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      
//****************************************************************************
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess.Metadata;
using SiemensTransactions = Siemens.WarehouseManagement.TransactionManagement;

namespace Siemens.WarehouseManagement.DataAccess
{
  /// <summary>
  /// Implements the common base functionality for all IDatabase implementations.
  /// Each IDatabase implementation should inherit from this base class.                                  
  /// </summary>
  public abstract class AbstractDatabase : IDatabase
  {
    #region Fields
    // create logger instance
    private static readonly ILog Log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);

    private readonly string _connectionString = string.Empty;
    private DbProviderFactory _provider;

    /// <summary>
    /// The connection this class is currently using
    /// </summary>
    //protected DbConnection _connection;
    /// <summary>
    /// connection to the database
    /// </summary>
    //private DbConnection Connection
    //{
    //  get
    //  {
    //    if (_connection == null || (_connection.State | ConnectionState.Closed) == ConnectionState.Closed)
    //    {
    //      CreateConnection();
    //    }
    //    return _connection;
    //  }
    //  set
    //  {
    //    _connection = value;
    //  }
    //}


    private int _threadId = -1;
    private static int _instanceCount;

    #endregion

    #region Construction/Destruction/Initialisation
    /// <summary>
    /// Constructor of AbstractDatabase which establishes the connection to the database.
    /// </summary>
    /// <param name="providerName">
    /// Invariant provider name needed by the ADO.NET provider factories to create a database
    /// specific provider factory.
    /// </param>
    /// <param name="connectionString">
    /// Connection string used to establish the initial connection to a database.
    /// The connection string syntax is provided by ADO.NET 2.0 and the data provider.
    /// </param>
    protected AbstractDatabase(string providerName, string connectionString)
    {
      Interlocked.Increment(ref _instanceCount);
      Log.InfoFormat("AbstractDatabase created. InstanceCount [{0}] Hash [{1}]", _instanceCount, base.GetHashCode());

      // check if the arguments are not null
      if (providerName == null)
        throw new ArgumentNullException("providerName");
      if (connectionString == null)
        throw new ArgumentNullException("connectionString");
      _connectionString = connectionString;

      // try to establish the initial connection to the database
      _provider = DbProviderFactories.GetFactory(providerName);

      if (_provider == null)
      {
        string errorMessage = "There is no DbProviderFactory for " + providerName + ".";
        if (Log.IsInfoEnabled)
          Log.Info(errorMessage);
        throw new ArgumentException(errorMessage);
      }

      //SILOC-1354 
      //Password may not occur on console!
      DbConnectionStringBuilder connectionStringBuilder = _provider.CreateConnectionStringBuilder();
      connectionStringBuilder.ConnectionString = _connectionString;
      connectionStringBuilder["password"] = "***";
      Log.InfoFormat("ConnectionString = {0}", connectionStringBuilder.ConnectionString);
      //CreateConnection();

      Log.InfoFormat("DB Provider = [{0}]", _provider.GetType().AssemblyQualifiedName);
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="AbstractDatabase"/> is reclaimed by garbage collection.
    /// </summary>
    ~AbstractDatabase()
    {
      Dispose(false);
    }

    /// <summary>
    /// Dispose method of AbstractDatabase which closes the databse connection.
    /// </summary>
    /// <param name="disposing">
    /// If disposing equals true, the method has been called directly or indirectly by a user's code 
    /// and managed and unmanaged resources can be disposed. 
    /// If disposing equals false, the method has been called by the runtime from inside the finalizer and 
    /// only unmanaged resources can be disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        _provider = null;
      }
      Interlocked.Decrement(ref _instanceCount);
      Log.InfoFormat("AbstractDatabase disposed. InstanceCount [{0}] Hash [{1}]", _instanceCount, GetHashCode());

    }
    #endregion

    #region IDisposable Members

    /// <summary>
    /// Dispose the database, thus close all open connections.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    private DbConnection CreateOpenConnection()
    {
      try
      {
        DbConnection result = _provider.CreateConnection();
        result.ConnectionString = _connectionString;
        result.Open();
        return result;        
      }
      catch (Exception ex)
      {
        throw new ConnectionLostException(ex.Message, ex.Message, ex);
      }
    }

    #region IDatabase Members

    #region Properties
    /// <summary>
    /// Get-Property representing the current system time of the underlaying database.
    /// </summary>
    public DateTime SystemTime
    {
      get
      {
        // create command
        using (DbCommand command = CreateCommand(SysTimeQuery))
        {
          using (IDataReader reader = ExecuteReader(command))
          {
            reader.Read();
            return reader.GetDateTime(0);
          }
        }
      }
    }

    /// <summary>
    /// Get-Property indicating wheter the identy column in an insert query has to be set or not.
    /// </summary>
    public abstract bool RequireIdentityColumn
    {
      get;
    }

    /// <summary>
    /// Get-Property representing the sql string needed to query the database system time.
    /// </summary>
    protected abstract string SysTimeQuery
    {
      get;
    }

    /// <summary>
    /// A string template to generate a lock statement for this database
    /// </summary>
    /// <remarks>
    /// The template must be a string that can be used in <c>String.Format</c>.
    /// It must contain three parameters:
    /// {0} - The names of the fields of the entity, comma-separated
    /// {1} - The Table name
    /// {2} - A WHERE expression selecting the specific row       
    /// </remarks>
    /// <example>
    /// SELECT {0} FROM {1} WHERE {2} FOR UPDATE
    /// </example>
    public abstract string RowLockStatementTemplate { get; }

    /// <summary>
    /// A string template to generate a try lock statement for this database
    /// </summary>
    /// <remarks>
    /// The template must be a string that can be used in <c>String.Format</c>.
    /// It must contain three parameters:
    /// {0} - The names of the fields of the entity, comma-separated
    /// {1} - The Table name
    /// {2} - A WHERE expression selecting the specific row       
    /// </remarks>
    /// <example>
    /// SELECT {0} FROM {1} WHERE {2} FOR UPDATE NOWAIT
    /// </example>
    public abstract string TryRowLockStatementTemplate { get; }



    #endregion

    #region Public Methods

    /// <summary>
    /// Starts a new database transaction
    /// </summary>
    /// <returns>
    /// A new transaction object suitable for this database object
    /// </returns>
    public DbTransaction BeginTransaction()
    {
      _threadId = Thread.CurrentThread.ManagedThreadId;
      DbConnection connection = null;
      try
      {
        connection = CreateOpenConnection();
        return connection.BeginTransaction();
      }
      catch (SystemException ex)
      {
        _threadId = -1;
        Log.Error("BeginTransaction failed! ", ex);
        if (connection != null)
        {
          connection.Dispose();
        }
        throw MapException(ex, connection);
      }
    }

    /// <summary>
    /// Commits a transaction for this database object
    /// </summary>
    /// <param name="transaction">
    /// The transaction to commit
    /// </param>
    public void CommitTransaction(DbTransaction transaction)
    {
      if (transaction == null) throw new ArgumentNullException("transaction");
      //if (transaction.Connection != Connection) throw new ArgumentException("Transaction was not created by this database object");
      try
      {
        DbConnection con = transaction.Connection;
        transaction.Commit();
        transaction.Dispose();
        con.Dispose();
      }
      catch (SystemException ex)
      {
        Log.Error("CommitTransaction failed! ", ex);
        throw MapException(ex, transaction.Connection);
      }
      finally
      {
        _threadId = -1;
      }
    }

    /// <summary>
    /// Rolls back a transaction for this database object
    /// </summary>
    /// <param name="transaction">
    /// The transaction to roll back
    /// </param>
    public void RollbackTransaction(DbTransaction transaction)
    {
      if (transaction == null) throw new ArgumentNullException("transaction");
      DbConnection con = transaction.Connection;
      try
      {
        transaction.Rollback();
      }
      catch (SystemException ex)
      {
        Log.Error("RollbackTransaction failed! ", ex);
        throw MapException(ex, transaction.Connection);
      }
      finally
      {
        transaction.Dispose();
        if (con != null)
        {
          con.Dispose();
        }
        _threadId = -1;
      }
    }

    /// <summary>
    /// Creates a named safepoint for a transaction
    /// </summary>
    /// <param name="transaction">
    /// The transaction to create a safepoint for
    /// </param>
    /// <param name="name"></param>
    public void CreateSavepoint(DbTransaction transaction, string name)
    {
      if (transaction == null) throw new ArgumentNullException("transaction");
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

      MethodInfo info = transaction.GetType().GetMethod("Save", new[] { typeof(string) });
      if (info == null) throw new InvalidOperationException("Underlying connection does not support savepoints");

      try
      {
        info.Invoke(transaction, new object[] { name });
      }
      catch (TargetInvocationException ex)
      {
        Log.Error("CreateSavepoint failed! ", ex.InnerException);
        throw MapException(ex.InnerException as SystemException, transaction.Connection);
      }
    }

    /// <summary>
    /// Rolls back a transaction to a safepoint
    /// </summary>
    /// <param name="transaction">
    /// The transaction to roll back
    /// </param>
    /// <param name="safepointName">
    /// The name of the transaction to roll back
    /// </param>
    public void RollbackTransaction(DbTransaction transaction, string safepointName)
    {
      if (transaction == null) throw new ArgumentNullException("transaction");
      if (String.IsNullOrEmpty(safepointName)) throw new ArgumentNullException("safepointName");
      //if (transaction.Connection != Connection) throw new ArgumentException("Transaction was not created by this database object");

      MethodInfo info = transaction.GetType().GetMethod("Rollback", new[] { typeof(string) });
      if (info == null) throw new InvalidOperationException("Underlying connection does not support savepoints");

      try
      {
        info.Invoke(transaction, new object[] { safepointName });
      }
      catch (TargetInvocationException ex)
      {
        Log.Error("RollbackTransaction failed! ", ex.InnerException);
        throw MapException(ex.InnerException as SystemException, transaction.Connection);
      }

    }

    /// <summary>
    /// Creates a DbCommand for a SQL query.
    /// </summary>
    /// <param name="query">SQL query</param>
    /// <returns>created DbCommand</returns>
    /// <exception cref="System.ArgumentException">
    ///   thrown if <c>query</c> is null or empty.
    /// </exception>
    public DbCommand GetSqlStringCommand(string query)
    {
      // check if the argument is not null
      if (query == null)
        throw new ArgumentNullException("query");

      // check if the query is not an empty string
      if (query.Length == 0)
        throw new ArgumentException("An empty string is not a valid SQL query.",
                                    "query");

      // adapt sql dialect
      query = AdaptSqlDialect(query);

      return CreateCommand(query);
    }

    /// <summary>
    /// Executes the command and returns the number of rows affected.
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <returns>number of rows affected</returns>
    /// <exception cref="System.ArgumentNullException">
    ///   thrown if <c>command</c> is null
    /// </exception>
    /// <exception cref="CrossThreadDbAccessViloationException">
    ///   thrown if a database object is used in multiple threads.
    /// </exception>
    /// <exception cref="DataAccessException">
    ///   thrown if the database raises an error while executing the query.
    /// </exception>
    public int ExecuteNonQuery(DbCommand command)
    {
      return ExecuteNonQuery(command, null);
    }


    /// <summary>
    /// Executes the command and returns the number of rows affected.
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="transaction">The transaction to use</param>
    /// <returns>number of rows affected</returns>
    /// <exception cref="System.ArgumentNullException">
    ///   thrown if <c>command</c> is null
    /// </exception>
    /// <exception cref="CrossThreadDbAccessViloationException">
    ///   thrown if a database object is used in multiple threads.
    /// </exception>
    /// <exception cref="DataAccessException">
    ///   thrown if the database raises an error while executing the query.
    /// </exception>
    public int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
    {
      // check if the argument is not null
      if (command == null) throw new ArgumentNullException("command");

      if (_threadId >= 0 && _threadId != Thread.CurrentThread.ManagedThreadId)
      {
        throw new CrossThreadDbAccessViloationException(_threadId, Thread.CurrentThread.ManagedThreadId);
      }

      DbConnection connection = null;
      try
      {
        connection = transaction == null ? CreateOpenConnection() : transaction.Connection;
        command.Transaction = transaction;
        command.Connection = connection;
        // execute query
        if (Log.IsDebugEnabled)
        {
          Log.DebugFormat("Calling ExecuteNonQuery on Database [{3}] Connection [{0}] ... \n\t\t\tcommandtext = [{1}], \n\t\t\tparameters are: \n\t\t\t[{2}]", command.Connection.GetHashCode(), command.CommandText, BuildParameterLogString(command), GetHashCode());
        }
        return command.ExecuteNonQuery();
      }
      catch (SystemException ex)
      {
        Log.Error("ExecuteNonQuery failed! ", ex);

        // Log the DbCommand
        LogDbCommand(command, "ExecuteNonQuery");

        throw MapException(ex, connection);
      }
      finally
      {
        if (connection != null)
        {
          command.Dispose();
        }
        if (transaction == null)
        {
          if (connection != null)
          {
            connection.Dispose();            
          }
        }
      }
    }

    /// <see cref="IDatabase.ExecuteReader(System.Data.Common.DbCommand)"/>
    public IDataReader ExecuteReader(DbCommand command)
    {
      return ExecuteReader(command, null);
    }


    /// <summary>
    /// Executes the command and returns an IDataReader through which the result can be read. 
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="transaction">The transaction to use</param>
    /// <returns>IDataReader instance</returns>
    /// <exception cref="System.ArgumentNullException">
    ///   thrown if <c>command</c> is null
    /// </exception>
    /// <exception cref="CrossThreadDbAccessViloationException">
    ///   thrown if a database object is used in multiple threads.
    /// </exception>
    /// <exception cref="DataAccessException">
    ///   thrown if the database raises an error while executing the query.
    /// </exception>
    public IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
    {
      // check if the argument is not null
      if (command == null) throw new ArgumentNullException("command");

      if (_threadId >= 0 && _threadId != Thread.CurrentThread.ManagedThreadId)
      {
        throw new CrossThreadDbAccessViloationException(_threadId, Thread.CurrentThread.ManagedThreadId);
      }

      DbConnection connection = null;
      CommandBehavior behavior = CommandBehavior.Default;

      try
      {
        if (transaction == null)
        {
          connection = CreateOpenConnection();
          behavior = CommandBehavior.CloseConnection;
        }
        else
        {
          connection = transaction.Connection;
        }

        command.Transaction = transaction;
        command.Connection = connection;
        // execute the command and return and IDataReader object
        if (Log.IsDebugEnabled)
        {

          Log.DebugFormat("Calling ExecuteReader on Database [{3}] Connection [{0}] ... \n\t\t\tcommandtext = [{1}], \n\t\t\tparameters are: \n[{2}]", command.Connection.GetHashCode(), command.CommandText, BuildParameterLogString(command), GetHashCode());
        }

        return command.ExecuteReader(behavior);
      }
      catch (SystemException ex)
      {
        DataAccessException dataAccessException = MapException(ex, connection);
        if (transaction == null && connection != null)
        {
          connection.Dispose();          
        }

        Log.Error("ExecuteReader failed!", ex);

        // Log the DbCommand
        LogDbCommand(command, "ExecuteReader");

        throw dataAccessException;
      }
    }

    /// <summary>
    /// Read the value of a single column from the current row
    /// </summary>
    /// <param name="reader">the database reader to read data from.</param>
    /// <param name="ordinal">the ordinal number of the column to read</param>
    /// <returns>The method returns the read object. If it is a value type, it is boxed</returns>
    /// <exception cref="UnspecifiedDataException">
    ///   thrown if an unhandleble database error is raised by the database
    /// </exception>
    /// <exception cref="TransactionAbortedException">
    ///   thrown if the current transaction was aborted by the database
    /// </exception>
    public virtual object Read(IDataReader reader, int ordinal)
    {
      try
      {
        return FromDbRepresentation(reader[ordinal]);
      }
      catch (SystemException ex)
      {
        Log.Error("ExecuteReader failed! - ", ex);
        throw MapException(ex, null);
      }
    }

    /// <summary>
    /// Read the value of a single column from the current row
    /// </summary>
    /// <param name="reader">the database reader to read data from.</param>
    /// <param name="columnName">the name of the column to read</param>
    /// <returns>The method returns the read object. If it is a value type, it is boxed</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    ///   thrown if no column of the name <c>columnName</c> exists in the result set.
    /// </exception>
    /// <exception cref="UnspecifiedDataException">
    ///   thrown if an unhandleble database error is raised by the database
    /// </exception>
    /// <exception cref="TransactionAbortedException">
    ///   thrown if the current transaction was aborted by the database
    /// </exception>
    public virtual object Read(IDataReader reader, string columnName)
    {
      try
      {
        return FromDbRepresentation(reader[columnName]);
      }
      catch (IndexOutOfRangeException ex)
      {
        throw new ArgumentException(columnName, ex);
      }
      catch (SystemException ex)
      {
        Log.Error("ExecuteReader failed! - ", ex);
        throw MapException(ex, null);
      }
    }

    /// <summary>
    /// Adds a new In DbParameter object to the given command. 
    /// </summary>
    /// <param name="command">command object to which the new parameter shall be added</param>
    /// <param name="name">name of the parameter</param>
    /// <param name="value">parameter value</param>
    /// <exception cref="System.ArgumentNullException">
    ///   thrown if <c>command</c> is null
    /// </exception>
    public void AddParameter(DbCommand command, string name, object value)
    {
      // check if the command argument is not null
      if (command == null)
        throw new ArgumentNullException("command");

      object translatedValue = ToDbRepresentation(value);
      DbParameter param;
      if (!command.Parameters.Contains(name))
      {
        param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = translatedValue;
        param.Direction = ParameterDirection.Input;
        //param.DbType = type;
        command.Parameters.Add(param);
      }
      else
      {
        param = command.Parameters[name];
        param.Value = translatedValue;
      }
    }

    /// <summary>
    /// Queries an autonamer key generated during insert.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <param name="transaction">
    ///   the transaciton to use when executing the command. 
    ///   <c>null</c> if no transaction shall be used.
    /// </param>
    /// <returns>autonumber key</returns>
    public abstract decimal GetKeyAfterInsert(string sequenceName, DbTransaction transaction);

    /// <summary>
    /// Gets a batabase specific selectbuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public abstract ExpressionVisitor<string> GetSelectVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder);

    /// <summary>
    /// Gets a batabase specific wherebuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public abstract ExpressionVisitor<string> GetWhereVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder);

    /// <summary>
    /// Gets a batabase specific havingbuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public abstract ExpressionVisitor<string> GetHavingVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder);

    /// <summary>
    /// Builds a parameter log string
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public string BuildParameterLogString(DbCommand command)
    {
      var strBuilder = new StringBuilder();
      foreach (DbParameter param in command.Parameters)
      {
        strBuilder.Append("\t\t\tParameterName = ");
        strBuilder.Append(param.ParameterName);
        strBuilder.Append(", ");
        strBuilder.Append("DbType = ");
        strBuilder.Append(param.DbType);
        strBuilder.Append(", ");
        strBuilder.Append("Value = ");
        strBuilder.Append(param.Value);
        strBuilder.Append(";\n");
      }
      return strBuilder.ToString();
    }

    /// <summary>
    /// Tests that this db instance can connect to the database
    /// </summary>
    public void TestConnection()
    {
      DbConnection con = CreateOpenConnection();
      con.Dispose();
    }


    /// <summary>
    /// Map a SystemException to an Exception of the System.Data.DataException hierarchy
    /// </summary>
    /// <param name="exception">The SystemException-object to map</param>
    /// <param name="connection"></param>
    /// <returns>A matching object of the System.Data.DataException hierarchy</returns>
    public abstract DataAccessException MapException(SystemException exception, DbConnection connection);

    #endregion
    #endregion

    #region Protected Methods

    /// <summary>
    /// Adapts the sql dialect to the dialect supported by the underlaying database.
    /// </summary>
    /// <param name="query">sql query to adapt</param>
    /// <returns>adapted sql query</returns>
    protected abstract string AdaptSqlDialect(string query);

    /// <summary>
    /// Translates a value to the appropriate representation for the database
    /// </summary>
    /// <param name="valueToTranslate">
    /// The value to translate
    /// </param>
    /// <returns>
    /// The translated value
    /// </returns>
    /// <remarks>
    /// This method translates null values to instances of DBNull. If other translations
    /// are necessary, derived classes need to override this method.
    /// </remarks>
    protected virtual object ToDbRepresentation(object valueToTranslate)
    {
      return valueToTranslate ?? DBNull.Value;
    }

    /// <summary>
    /// Translate a value as sent by the database to a value suitable for programming
    /// </summary>
    /// <param name="valueToTranslate">
    /// The value as returned from the database
    /// </param>
    /// <returns>
    /// The translated value
    /// </returns>
    /// <remarks>
    /// This method translates DBNull values
    /// </remarks>
    protected virtual object FromDbRepresentation(object valueToTranslate)
    {
      if (valueToTranslate is DBNull)
      {
        return null;
      }
      return valueToTranslate;
    }

    /// <summary>
    /// Replaces parmeter indicators within an SQL query
    /// </summary>
    /// <param name="query">SQL query</param>
    /// <param name="oldParameterIndicator">old parameter indicator</param>
    /// <param name="newParameterIndicator">new parameter indicator</param>
    /// <returns>modified SQL query</returns>
    protected static string ReplaceParameterIndicator(string query, char oldParameterIndicator, char newParameterIndicator)
    {
      // check if the argument is not null
      if (query == null)
        throw new ArgumentNullException("query");

      int index = query.IndexOf('\'');

      if (index == -1)
      {
        return query.Replace(oldParameterIndicator, newParameterIndicator);
      }

      var retvalBuilder = new StringBuilder();
      int counter = 0;

      while (index > -1)
      {
        // increase counter
        counter++;
        // build substring
        string tmp = query.Substring(0, index + 1);
        // do replacement if allowed
        if ((counter % 2) != 0)
          tmp = tmp.Replace(oldParameterIndicator, newParameterIndicator);
        // add substring to retval
        retvalBuilder.Append(tmp);
        // shorten query
        int length = query.Length - tmp.Length;
        query = query.Substring(index + 1, length);
        // calculate new index
        index = query.IndexOf('\'');
      }

      if (query.Length > 0)
        retvalBuilder.Append(query.Replace(oldParameterIndicator, newParameterIndicator));

      return retvalBuilder.ToString();
    }

    /// <summary>
    /// Creates a new DbCommand.
    /// </summary>
    /// <param name="query">sql query</param>
    /// <returns>created DbCommand</returns>
    protected DbCommand CreateCommand(string query)
    {
      // create command
      DbCommand command = _provider.CreateCommand();
      command.CommandText = query;
      command.CommandType = CommandType.Text;

      // TODO: Remove this hack! Cache the PropertyInfo, 
      PropertyInfo info = command.GetType().GetProperty("BindByName");
      if (info != null)
      {
        info.SetValue(command, true, new object[0]);
      }

      return command;
    }

    /// <summary>
    /// Queries the autonumber key (identity) from the database
    /// </summary>
    /// <param name="query">sql query</param>
    /// <param name="transaction">
    ///   the transaction to use. 
    ///   <c>null</c> if no transaction shall be used.
    /// </param>
    /// <returns>autonumber key (identity)</returns>
    protected decimal QueryIdentity(string query, DbTransaction transaction)
    {
      // create command
      DbCommand command = CreateCommand(query);
      // read identity
      IDataReader reader = ExecuteReader(command, transaction);

      try
      {
        reader.Read();
        if (Log.IsDebugEnabled)
        { Log.DebugFormat("Read Identity [{0}]", reader.GetDecimal(0)); }
        return reader.GetDecimal(0);
      }
      catch (Exception ex)
      {
        if (Log.IsErrorEnabled)
        { Log.Error("Reading the identity failed!"); }
        throw new UnspecifiedDataException(ex.Message, ex.Message, ex);
      }
      finally
      {
        reader.Close();
        command.Dispose();
      }
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a log entry containing the methodName and the DbCommand
    /// </summary>
    /// <param name="command"></param>
    /// <param name="methodName"></param>
    private void LogDbCommand(DbCommand command, string methodName)
    {
      if (Log.IsInfoEnabled)
      {
        try
        {
          Log.InfoFormat("Exception caught in {0} ... \n\t\t\tcommandtext = [{1}], \n\t\t\tparameters are: \n[{2}]", methodName, command.CommandText, BuildParameterLogString(command));
        }
        catch (Exception e)
        {
          Log.Error("Exception while logging exception", e);
        }
      }
    }

    #endregion
  }
}

//******************************************************************************
//*     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      *
//******************************************************************************
