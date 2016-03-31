using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl.Puffer
{
  /// <summary>
  /// 
  /// </summary>
  public interface IPufferPlatzSuche
  {
    /// <summary>
    /// Ermittelt einen freien Pufferplatz.
    /// </summary>
    /// <param name="sgmTu">die Tu, f�r die eine Location gefunden werden soll.</param>
    /// <returns>eine SgmLocation im Puffer</returns>
    SgmLocation ErmittleFreienPufferPlatz(SgmTu sgmTu);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************