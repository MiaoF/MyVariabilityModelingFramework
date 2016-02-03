  
//******************************************************************************
// NAME: IAklTransportGenerierung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
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
  public interface IAklTransportGenerierung : IZyklisch 
  {
    /// <summary>
    /// Erzeugt einen TordRequest für die übergebene Tu
    /// </summary>
    /// <param name="tu">Die Tu die eingelagert werden soll</param>
    void Einlagern(SgmTu tu);

    /// <summary>
    /// Erzeugt einen TordRequest für die übergebene Tu zum Arbeitsplatz
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="arbeitsPlatzLocation"></param>
    void TablarAuslagern(SgmTu tu, SgmLocation arbeitsPlatzLocation, TordRequest.TransportTypValue transportTyp);

    /// <summary>
    /// Löscht alle TordRequest zu einem Ziel eines bestimmten TransportTypes
    /// </summary>
    /// <param name="zielLocationName"></param>
    /// <param name="transportTyp"></param>
    void DeleteTordRequests(string zielLocationName, TordRequest.TransportTypValue transportTyp);

    /// <summary>
    /// Überprüft die Gültigkeit der TordRequests für die übergebenen Tu und löscht
    /// ggf. ungültige TordRequests
    /// </summary>
    /// <param name="tu"></param>
    void CheckTordRequestsAfterStorno(SgmTu tu);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
