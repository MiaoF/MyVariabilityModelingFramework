    
//******************************************************************************
// NAME: IControlCenter.cs
//******************************************************************************
//
// Description:
//
//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************

using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Infrastructure.Configuration;

namespace Siemens.WarehouseManagement.ControlCenter
{
  public interface IControlCenter
  {
    /// <summary>
    /// Get all Tus at the storage bin location
    /// which are not connected with an open notificationPos
    /// which has no material reservation
    /// </summary>
    /// <returns>A query to recieve all tus</returns>
    /// <param name="workset">the workset location</param>
    [Synchronous]
    Query GetTuAndTup(string workset);

    /// <summary>
    /// gets all Tu linked with TuPos
    /// at the clear location
    /// </summary>
    /// <param name="workset">the workset location</param>
    [Synchronous]
    Query GetTuAtClearLoc(string workset);

    /// <summary>
    /// locks the tupos with clearing lock
    /// transports the given tu to the clearing location
    /// <remarks>IMPL:SVC_CTRL_TU_REQUEST:IMPL</remarks>
    /// </summary>
    /// <param name="tuName">the tu name</param>
    /// <param name="workset">the workset location</param>
    void TransportToClearLoc(string tuName, string workset);
  }
}


