  
//****************************************************************************
//  NAME: SqlDatabase.cs                                                          
//****************************************************************************
//                                                                            
//  Description:                                                              
//    Implements the specific IDatabase functionality for an MS SQL Server.
//    The implementation is based upon the ADO.Net System.Data.SqlClient
//    database provider.                                 
//                                                                            
//                                                                            
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      
//****************************************************************************
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using log4net;
using System.Reflection;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess.Metadata;
using Siemens.WarehouseManagement.Infrastructure.DataAccess.Visitors;

namespace Siemens.WarehouseManagement.DataAccess
{
    /// <summary>
    /// Implements the specific IDatabase functionality for an MS SQL Server.
    ///  The implementation is based upon the ADO.Net System.Data.SqlClient
    ///  database provider.  
    /// </summary>
    public class SqlDatabase : AbstractDatabase
    {
        #region Fields
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private const string ProviderName = "System.Data.SqlClient";
        private const string SysDateQuery = "SELECT CURRENT_TIMESTAMP";
        private const string IdentityQuery = "SELECT @@IDENTITY AS [SCOPE_IDENTITY]";
        #endregion

        #region Construction/Destruction/Initialisation
        /// <summary>
        /// Constructor of SqlDatabase
        /// </summary>
        /// <param name="connectionString">
        /// Connection string used to establish the initial connection to a MS SQL Server database.
        /// The connection string syntax is provided by ADO.NET 2.0 and the System.Data.SqlClient 
        /// data provider.
        /// </param>
        public SqlDatabase(string connectionString)
            : base(ProviderName, connectionString)
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
            get { return false; }
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
          get { return "SELECT {0} FROM {1} WITH (ROWLOCK UPDLOCK) WHERE {2}"; }
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
          get { return "SELECT {0} FROM {1} WITH (READPAST ROWLOCK UPDLOCK) WHERE {2}"; }
        }


        #endregion

        #region Protected Methods
        /// <summary>
        /// Adapts the sql dialect to the dialect supported by the MS SQL Server.
        /// </summary>
        /// <param name="query">sql query to adapt</param>
        /// <returns>adapted sql query</returns>
        protected override string AdaptSqlDialect(string query)
        {
            return ReplaceParameterIndicator(query, ':', '@');
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

            if (connection != null)
            {
              if (connection.State == ConnectionState.Closed)
                {
                    if (exception == null)
                    {
                        Log.Warn("Mapping <null> exception");
                        return new ConnectionLostException("No system exception available.", "No system exception available.");
                    }

                    return new ConnectionLostException(exception.Message, exception.Message, exception);
                }
            }

            if (exception == null)
            {
                Log.Warn("Mapping <null> exception");
                return new UnspecifiedDataException("No system exception available.");
            }
            // cast exception
            if(exception is SqlTypeException)
            {
                Log.Debug("Mapping SqlTypeException");
                return new ValueToLongException(exception.Message,
                                                "Datum liegt nicht im vorgeschriebenen Wertebereich.", exception);
            }

            var ex = exception as SqlException;
            if (ex == null)
            {
                Log.WarnFormat("Mapping non-sql exception, type [{0}]", exception.GetType().FullName);
                return new UnspecifiedDataException(exception.Message, exception.Message, exception);
            }
      
            Log.DebugFormat("Mapping SqlException, Number=[{0}]", ex.Number);
            
            // map exception according to its error numbers resp. error codes
            if (ex.Number.Equals(2627))
            {
                result = new DuplicateKeyException(exception.Message, exception.Message, exception);
            }
            else if (ex.Number.Equals(515) || ex.Number.Equals(547))
            {
                result = new ConstraintException(exception.Message, exception.Message, exception);
            }
            else if (ex.Number.Equals(1205))
            {
                result = new TransactionAbortedException(exception.Message, exception.Message, exception);
                //result = new ResourceLockedException(exception);
            }
            else if (ex.Number.Equals(563) || ex.Number.Equals(3960) /*Isolation Level Snapshot Exception*/)
            {
                result = new TransactionAbortedException(exception.Message, exception.Message, exception);
            }
            else if (ex.Number.Equals(8152) || ex.Number.Equals(8115))
            {
                result = new ValueToLongException(exception.Message, exception.Message, exception);
            }
            else if (ex.Number.Equals(2) || ex.Number.Equals(232) || ex.Number.Equals(6005))
            {
                result = new ConnectionLostException(exception.Message, exception.Message, exception);
            }
            else if (ex.Number.Equals(4405))
            {
                result = new ReadonlyEntityException(exception.Message, exception.Message, exception);
            }
            else
            {
                result = new UnspecifiedDataException(exception.Message, exception.Message, exception);
            }

            return result;
        }

    

        #endregion

        #region Public Methods
        /// <summary>
        /// Queries an autonumber key generated during insert.
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

            return QueryIdentity(IdentityQuery, transaction);
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
            return new SqlSelectBuildingVisitor(ems, query, parameters, holder);
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
            return new SqlHavingBuildingVisitor(ems, query, parameters, holder);
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

            return base.FromDbRepresentation(valueToTranslate);
        }

        #endregion
    }
}

//******************************************************************************
//*     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      *
//******************************************************************************