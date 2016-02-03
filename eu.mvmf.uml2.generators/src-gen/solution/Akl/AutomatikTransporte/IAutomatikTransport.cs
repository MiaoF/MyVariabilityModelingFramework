//****************************************************************************
//  NAME: IAutomatikTransport.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl.AutomatikTransporte
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAutomatikTransport
  {
    /// <summary>
    /// Transportiert eine Tu von der Quelle zum Ziel
    /// </summary>
    /// <param name="tu">Die Tu</param>
    /// <param name="quelle">Die Quelle</param>
    /// <param name="ziel">Das Ziel</param>
    /// <param name="moveType">The reason for the request to move Tu.</param>
    /// <param name="informImmediatelyOnInterrupt">if set to <c>true</c> an event will be raised immediately if the transport is interrupted due to physic state.</param>
    /// <param name="additionalData">Additional Data that will be passed on to the Transport Medium Adapters</param>
    bool Transportiere(SgmTu tu, SgmLocation quelle, SgmLocation ziel, MoveType moveType, bool informImmediatelyOnInterrupt, IDictionary<string, string> additionalData);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
