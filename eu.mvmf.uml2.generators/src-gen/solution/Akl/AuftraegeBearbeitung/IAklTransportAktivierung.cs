  
//****************************************************************************
//  NAME: IAklTransportAktivierung.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl.AuftraegeBearbeitung
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAklTransportAktivierung : IZyklisch
  {

    /// <summary>
    /// F�gt den �bergebenen Observer hinzu
    /// </summary>
    /// <param name="observer"></param>
    void AddObserver(IAklTransportAktivierungObserver observer);

    /// <summary>
    /// Entfernt den �bergebenen Observer
    /// </summary>
    /// <param name="observer"></param>
    void RemoveObserver(IAklTransportAktivierungObserver observer);

    /// <summary>
    /// Legt einen Tord vom Arbeitsplatz zum �bergabeplatz des Verschiebewagens an.
    /// </summary>
    /// <param name="tu">die Tu zum abtransportieren</param>
    /// <returns></returns>
    void TransportiereWegVomArbeitsplatz(SgmTu tu);

  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************