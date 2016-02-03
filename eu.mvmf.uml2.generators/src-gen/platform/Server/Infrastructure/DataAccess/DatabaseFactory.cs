  
//****************************************************************************
//  NAME: DatabaseFactory.cs                                                          
//****************************************************************************
//                                                                            
//  Description:                                                              
//    Factory class to create specific database instances.                                 
//                                                                            
//                                                                            
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      
//****************************************************************************
using System;
using System.Configuration;
using System.Reflection;
using log4net;

namespace Siemens.WarehouseManagement.DataAccess
{
  /// <summary>
  /// Factory class to create specific database instances.
  /// </summary>
  public static class DatabaseFactory
  {
    #region Fields
    // create logger instance
    private static readonly ILog Log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);

    private const string ConnStrName = "WMSDB";
    private const string MsSqlProviderName = "System.Data.SqlClient";
//    private const string OracleProviderName2 = "System.Data.OracleClient";
    private const string OracleProviderName = "Oracle.DataAccess.Client";
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates a specific database instances according to the connectionString
    /// configuration within the machine.config or the app.config file.
    /// </summary>
    /// <param name="connectionStringName">
    /// Name of the connectionString defined in the machine.config or the app.config file
    /// </param>
    /// <returns>new database instance</returns>
    public static IDatabase CreateDatabase(string connectionStringName)
    {
      IDatabase db = null;

      // check if the argument is not null
      if (connectionStringName == null)
        throw new ArgumentNullException("connectionStringName");

      // read the connectionString data out of the machine.config or the app.config
      ConnectionStringSettings connectionStringData = ConfigurationManager.ConnectionStrings[connectionStringName];
      if (connectionStringData == null)
      {
        throw new ArgumentException("There is no connectionString for the name '" +
            connectionStringName + "' in the machine.config or the app.config.");
      }

      // create database
      switch (connectionStringData.ProviderName)
      {
        case MsSqlProviderName:
          {
            db = new SqlDatabase(connectionStringData.ConnectionString);
            break;
          }
        //case OracleProviderName2:
        case OracleProviderName:
          {
            db = new OracleDatabase(connectionStringData.ConnectionString, connectionStringData.ProviderName);
            break;
          }
        default:
          {
            throw new System.InvalidOperationException("The provider = '' " +
              connectionStringData.ProviderName + " is not supported.");
          }
      }

      if (Log.IsInfoEnabled)
        Log.Info(db.ToString() + " successfully created.");

      return db;
    }

    /// <summary>
    /// Creates a specific database instances according to the connectionString
    /// configuration with the name 'WMSDB' within the machine.config or the app.config file.
    /// </summary>
    /// <returns>new database instance</returns>
    private static IDatabase CreateDatabase()
    {
      return CreateDatabase(ConnStrName);
    }
    #endregion
  }
}
//******************************************************************************
//*     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.      *
//******************************************************************************
