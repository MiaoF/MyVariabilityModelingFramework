  
//****************************************************************************
//  NAME: OracleDatabase.cs                                                          
//****************************************************************************
//                                                                            
//  Description:                                                              
//    Implements the specific IDatabase functionality for an Oracle Database.
//    The implementation is based upon the ADO.Net System.Data.OracleClient
//    database provider.                                 
//                                                                            
//                                                                            
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      
//****************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using log4net;
using System.Reflection;
using System.Globalization;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess.Metadata;
using Siemens.WarehouseManagement.Infrastructure.DataAccess.Visitors;

namespace Siemens.WarehouseManagement.DataAccess
{
  /// <summary>
  /// Implements the specific IDatabase functionality for an Oracle Database.
  ///  The implementation is based upon the ADO.Net System.Data.OracleClient
  ///  database provider.  
  /// </summary>
  public class OracleDatabase : AbstractDatabase
  {
    #region Fields

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    //private const string ProviderName = "System.Data.OracleClient";
    private const string OracleUniqueConstraintViolatedError = "ORA-00001";
    private const string OracleDeadlockDetected = "ORA-00060";
    private const string OracleNullConstraintViolatedError = "ORA-01400";
    private const string OracleCheckConstraintViolatedError = "ORA-02290";
    private const string OracleIntegrityConstraintViolatedMissingParentError = "ORA-02291";
    private const string OracleIntegrityConstraintViolatedStillChildsError = "ORA-02292";
    private const string OracleNowaitFailedError = "ORA-00054";
    private const string OracleWaitExceededError = "ORA-30006";
    private const string OracleAccessNotSerializableError = "ORA-08177";
    private const string OracleValueToLong = "ORA-12899";
    private const string OracleIllegalVariableNameOrNumber = "ORA-01036";

    private const string OracleNotConnected = "ORA-03114";
    private const string OracleShutdownActive = "ORA-01089";
    private const string OracleReadonlyView = "ORA-01779";

    private const string SysDateQuery = "SELECT SYSDATE FROM DUAL";

    #endregion

    #region Construction/Destruction/Initialisation
    /// <summary>
    /// Constructor of OracleDatabase
    /// </summary>
    /// <param name="connectionString">Connection string used to establish the initial connection to an Oracle database.
    /// The connection string syntax is provided by ADO.NET 2.0 and the System.Data.OracleClient
    /// data provider.</param>
    /// <param name="providerName">Name of the provider.</param>
    public OracleDatabase(string connectionString, string providerName)
      : base(providerName, connectionString)
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Get-Property representing the sql string needed to query the database system time.
    /// </summary>
    protected override string SysTimeQuery
    {
      get { return SysDateQuery; }
    }

    /// <summary>
    /// Get-Property indicating wheter the identy column in an insert query has to be set or not.
    /// </summary>
    public override bool RequireIdentityColumn
    {
      get { return true; }
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
    public override string RowLockStatementTemplate
    {
      get { return "SELECT {0} FROM {1} WHERE {2} FOR UPDATE"; }
    }

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
    public override string TryRowLockStatementTemplate
    {
      get { return "SELECT {0} FROM {1} WHERE {2} FOR UPDATE NOWAIT"; }
    }

    #endregion

    #region Protected Methods
    /// <summary>
    /// Adapts the sql dialect to the dialect supported by the oracle database.
    /// </summary>
    /// <param name="query">sql query to adapt</param>
    /// <returns>adapted sql query</returns>
    protected override string AdaptSqlDialect(string query)
    {
      return ReplaceParameterIndicator(query, '@', ':');
    }

    /// <summary>
    /// Map a SystemException to an Exception of the System.Data.DataException hierarchy
    /// </summary>
    /// <param name="exception">The SystemException-object to map</param>
    /// <param name="connection"></param>
    /// <returns>A matching object of the System.Data.DataException hierarchy</returns>
    public override DataAccessException MapException(SystemException exception, DbConnection connection)
    {
      DataAccessException result;
      if (connection != null && connection.State == ConnectionState.Closed)
      {
        if (exception == null)
        {
          result = new ConnectionLostException("No system exception available.", "No system exception available.");
        }
        else
        {
          result = new ConnectionLostException(exception.Message, exception.Message, exception);
        }
      }
      else if (exception == null)
      {
        result = new UnspecifiedDataException("No system exception available.", "No system exception available.", exception);
      }
      // NOTE: Graceful Exit on Oracle IllegalVariableName/Number Exception (might be bug in Oracle Client)
      else if (exception.Message.StartsWith(OracleIllegalVariableNameOrNumber, StringComparison.Ordinal))
      {
        string text = string.Format("Fatal Oracle Exception [{0}] received", exception.Message);
        log.Fatal(text, exception);
        Environment.Exit(-1);
        return null;
      }
      // NOTE: Graceful Exit on System.AccessViolationException (might be bug in Oracle Client)
      else if (exception is AccessViolationException)
      {
        string text = string.Format("Caught AccessViolationException: [{0}]", exception.Message);
        log.Fatal(text, exception);
        Environment.Exit(-1);
        return null;
      }
      else if (exception.Message.StartsWith(OracleUniqueConstraintViolatedError, StringComparison.Ordinal))
      {
        result = new DuplicateKeyException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleNullConstraintViolatedError, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleCheckConstraintViolatedError, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleIntegrityConstraintViolatedMissingParentError, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleIntegrityConstraintViolatedStillChildsError, StringComparison.Ordinal)
          )
      {
        result = new ConstraintException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleNowaitFailedError, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleWaitExceededError, StringComparison.Ordinal))
      {
        result = new ResourceLockedException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleAccessNotSerializableError, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleDeadlockDetected, StringComparison.Ordinal))
      {
        result = new TransactionAbortedException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleValueToLong, StringComparison.Ordinal))
      {
        result = new ValueToLongException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleNotConnected, StringComparison.Ordinal) ||
               exception.Message.StartsWith(OracleShutdownActive, StringComparison.Ordinal))
      {
        result = new ConnectionLostException(exception.Message, exception.Message, exception);
      }
      else if (exception.Message.StartsWith(OracleReadonlyView, StringComparison.Ordinal))
      {
        result = new ReadonlyEntityException(exception.Message, exception.Message, exception);
      }
      else
      {
        result = new UnspecifiedDataException(exception.Message, exception.Message, exception);
      }

      if (result is ConnectionLostException || result is UnspecifiedDataException)
      {
        log.Error("Discarding database connection because of a critical error.");
      }

      return result;
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Queries an autonamer key generated during insert.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <param name="transaction">
    ///   the transaciton to use when executing the command. 
    ///   <c>null</c> if no transaction shall be used.
    /// </param>
    /// <returns>autonumber key</returns>
    public override decimal GetKeyAfterInsert(string sequenceName, DbTransaction transaction)
    {
      // check if the argument is not null
      if (sequenceName == null)
        throw new ArgumentNullException("sequenceName");
      //build SQL String
      string sqlText = string.Format(CultureInfo.InvariantCulture, "select {0}.currval from dual", sequenceName);
      // sequenceName key
      return QueryIdentity(sqlText, transaction);
    }

    /// <summary>
    /// Gets a batabase specific selectbuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public override ExpressionVisitor<string> GetSelectVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder)
    {
      return new OracleSelectBuildingVisitor(ems, query, parameters, holder);
    }

    /// <summary>
    /// Gets a batabase specific wherebuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public override ExpressionVisitor<string> GetWhereVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder)
    {
      return new WhereBuildingVisitor(ems, query, parameters, holder);
    }

    /// <summary>
    /// Gets a batabase specific havingbuildingvisitor.
    /// </summary>
    /// <param name="ems">The needed EntityMetadata.</param>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The parameters for the Query.</param>
    /// <param name="holder">The palceHolders.</param>
    /// <returns></returns>
    public override ExpressionVisitor<string> GetHavingVisitor(Dictionary<string, EntityMetadata> ems, Query query, Dictionary<string, object> parameters, List<string> holder)
    {
      return new OracleHavingBuildingVisitor(ems, query, parameters, holder);
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
    protected override object FromDbRepresentation(object valueToTranslate)
    {
      if (valueToTranslate is byte)
      {
        return (decimal)(byte)valueToTranslate;
      }

      if (valueToTranslate is short)
      {
        return (decimal)(short)valueToTranslate;
      }

      if (valueToTranslate is int)
      {
        return (decimal)(int)valueToTranslate;
      }

      if (valueToTranslate is long)
      {
        return (decimal)(long)valueToTranslate;
      }

      if (valueToTranslate is float)
      {
        return (decimal)(float)valueToTranslate;
      }

      if (valueToTranslate is double)
      {
        return (decimal)(double)valueToTranslate;
      }

      if (valueToTranslate is string)
      {
        var stringRepresentation = (string)valueToTranslate;
        if (stringRepresentation == "\0")
        {
          return String.Empty;
        }
        return stringRepresentation;
      }

      return base.FromDbRepresentation(valueToTranslate);
    }

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
    protected override object ToDbRepresentation(object valueToTranslate)
    {
      if (valueToTranslate is string)
      {
        if (String.IsNullOrEmpty((string)valueToTranslate))
        {
          return "\0";
        }
        return valueToTranslate;
      }
      return base.ToDbRepresentation(valueToTranslate);
    }

#if FALSE
    /// <summary>
    /// Read the value of a single column from the current row
    /// </summary>
    /// <param name="reader">the database reader to read data from.</param>
    /// <param name="ordinal">the ordinal number of the column to read</param>
    /// <returns>The method returns the read object. If it is a value type, it is boxed</returns>
    /// <remarks>
    /// The oracle driver tries to find a minimal representation for a NUMBER() value
    /// in the database. Since the data access code expects decimals, this is undone by 
    /// this method.
    /// </remarks>
    public override object Read(IDataReader reader, int ordinal)
    {
      try
      {
        Type columnType = reader.GetFieldType(ordinal);

        if (reader.IsDBNull(ordinal))
        {
          return DBNull.Value;
        }

        if (columnType == typeof(System.Byte) ||
            columnType == typeof(System.Int16) ||
            columnType == typeof(System.Int32) ||
            columnType == typeof(System.Int64) ||
            columnType == typeof(System.Single) ||
            columnType == typeof(System.Double) ||
            columnType == typeof(System.Decimal)
          )
        {
          return reader.GetDecimal(ordinal);
        }
        else
        {
          return reader.GetValue(ordinal);
        }
      }
      catch (SystemException ex)
      {
        if (Log.IsErrorEnabled)
          Log.Error("Read failed! - ", ex);
        throw MapException(ex);
      }

    }

    /// <summary>
    /// Read the value of a single column from the current row
    /// </summary>
    /// <param name="reader">the database reader to read data from.</param>
    /// <param name="columnName">the name of the column to read</param>
    /// <returns>The method returns the read object. If it is a value type, it is boxed</returns>
    /// <remarks>
    /// The oracle driver tries to find a minimal representation for a NUMBER() value
    /// in the database. Since the data access code expects decimals, this is undone by 
    /// this method.
    /// </remarks>
    public override object Read(IDataReader reader, string columnName)
    {
      return this.Read(reader, reader.GetOrdinal(columnName));
    }
#endif

    #endregion
  }
}

//******************************************************************************
//*     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      *
//******************************************************************************