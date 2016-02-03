    
//****************************************************************************
//  NAME: IControlCenterSynchronizer.cs
//****************************************************************************
//
//  Description: Synchronizes the Control Center
//
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//****************************************************************************

namespace Siemens.WarehouseManagement.ControlCenter
{
  /// <summary>
  /// This interface should be used by the solution to synchronize
  /// Control center calls.
  /// </summary>
  public interface IControlCenterSynchronizer
  {
    /// <summary>
    /// locks the tupos with clearing lock
    /// transports the given tu to the clearing location
    /// <remarks>IMPL:SVC_CTRL_TU_REQUEST:IMPL</remarks>
    /// </summary>
    /// <param name="tuName">the tu name</param>
    /// <param name="workset">the workset location</param>
    /// <param name="token">the token</param>
    void TransportToClearLoc(string tuName, string workset, string token);
  }
}



