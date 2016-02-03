    
//****************************************************************************
//  NAME: MaterialManagementSynchronizer.cs
//****************************************************************************
//
//  Description: Synchronizes the Material management component.
//
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//****************************************************************************

using System.Reflection;
using Siemens.WarehouseManagement.AsyncMessaging;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Validation;

namespace Siemens.WarehouseManagement.ControlCenter
{
  public class ControlCenterSynchronizer : Synchronizer, IControlCenterSynchronizer, IControlCenter
  {
    // Define a static logger variable
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Gets or sets the real ControlCenter object.
    /// </summary>
    /// <value>The real ControlCenter.</value>
    [RequiredProperty]
    public IControlCenter RealControlCenter { get; set; }

    /// <summary>
    /// Gets or sets a proxy to the ControlCenter in synchronizer.
    /// </summary>
    /// <value>The goods in proxy.</value>
    [RequiredProperty]
    public IControlCenterSynchronizer ControlCenterProxy { get; set; }

    #region IControlCenter

    /// <summary>
    /// Get all Tus at the storage bin location
    /// which are not connected with an open notificationPos
    /// which has no material reservation
    /// </summary>
    /// <param name="workset">the workset location</param>
    /// <returns>A query to recieve all tus</returns>
    public Query GetTuAndTup(string workset)
    {
      log.DebugFormat("ControlCenterSynchronizer - {0}", MethodBase.GetCurrentMethod().Name);
      return RealControlCenter.GetTuAndTup(workset);
    }

    /// <summary>
    /// gets all Tu linked with TuPos
    /// at the clear location
    /// </summary>
    /// <param name="workset">the workset location</param>
    public Query GetTuAtClearLoc(string workset)
    {
      log.DebugFormat("ControlCenterSynchronizer - {0}", MethodBase.GetCurrentMethod().Name);
      return RealControlCenter.GetTuAtClearLoc(workset);
    }

    /// <summary>
    /// locks the tupos with clearing lock
    /// transports the given tu to the clearing location
    /// <remarks>IMPL:SVC_CTRL_TU_REQUEST:IMPL</remarks>
    /// </summary>
    /// <param name="tuName">the tu name</param>
    /// <param name="workset">the workset location</param>
    public void TransportToClearLoc(string tuName, string workset)
    {
      log.DebugFormat("ControlCenterSynchronizer - {0}", MethodBase.GetCurrentMethod().Name);
      Synchronize(guid => ControlCenterProxy.TransportToClearLoc(tuName, workset, guid));
    }

    #endregion

    #region IControlCenterSynchronizer

    /// <summary>
    /// locks the tupos with clearing lock
    /// transports the given tu to the clearing location
    /// <remarks>IMPL:SVC_CTRL_TU_REQUEST:IMPL</remarks>
    /// </summary>
    /// <param name="tuName">the tu name</param>
    /// <param name="workset">the workset location</param>
    /// <param name="token">the token</param>
    public void TransportToClearLoc(string tuName, string workset,  string token)
    {
      log.DebugFormat("ControlCenterSynchronizer - {0}", MethodBase.GetCurrentMethod().Name);
      Wrap(() => RealControlCenter.TransportToClearLoc(tuName, workset), token);

    }

    #endregion
  }
}



