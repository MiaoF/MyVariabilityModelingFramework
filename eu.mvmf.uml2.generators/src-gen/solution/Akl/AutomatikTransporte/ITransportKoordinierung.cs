//******************************************************************************
// NAME: ITransportKoordinierung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl.AutomatikTransporte
{
  /// <summary>
  /// Stellt eine Schnittstelle da mit deren Hilfe �berpr�ft werden kann ob ein Transportauftrag momentan fahrbar ist
  /// </summary>
  public interface ITransportKoordinierung
  {
    /// <summary>
    /// �berprueft ob ein Tablar vom VTW zu einem Arbeitsplatz uebersgetzt werden kann
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="locationName"></param>
    /// <returns></returns>
    bool CanTablarUebersetzten(SgmTu tu, string locationName);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************