  
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
    /// Erzeugt einen TordRequest f�r die �bergebene Tu
    /// </summary>
    /// <param name="tu">Die Tu die eingelagert werden soll</param>
    void Einlagern(SgmTu tu);

    /// <summary>
    /// Erzeugt einen TordRequest f�r die �bergebene Tu zum Arbeitsplatz
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="arbeitsPlatzLocation"></param>
    void TablarAuslagern(SgmTu tu, SgmLocation arbeitsPlatzLocation, TordRequest.TransportTypValue transportTyp);

    /// <summary>
    /// L�scht alle TordRequest zu einem Ziel eines bestimmten TransportTypes
    /// </summary>
    /// <param name="zielLocationName"></param>
    /// <param name="transportTyp"></param>
    void DeleteTordRequests(string zielLocationName, TordRequest.TransportTypValue transportTyp);

    /// <summary>
    /// �berpr�ft die G�ltigkeit der TordRequests f�r die �bergebenen Tu und l�scht
    /// ggf. ung�ltige TordRequests
    /// </summary>
    /// <param name="tu"></param>
    void CheckTordRequestsAfterStorno(SgmTu tu);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
