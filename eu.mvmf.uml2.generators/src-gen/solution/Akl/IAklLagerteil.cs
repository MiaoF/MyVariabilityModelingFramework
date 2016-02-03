
//******************************************************************************
// NAME: IAklLagerteil.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Infrastructure.Configuration;

namespace Sgm.Akl
{
  public interface IAklLagerteil
  {
    /// <summary>
    /// Wird aufgerufen, wenn eine Tu in einen Pufferplatz transportiert wurde. Sie setzt den Status der Entnahme auf "fertig"
    /// und prüft notwendige Statusänderungen des Auftrags.
    /// </summary>
    /// <param name="tu">die gepufferte Tu</param>
    void TuBereitgestellt(SgmTu tu);

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu vom Puffer zum Zusammenführplatz transportiert wurde.
    /// </summary>
    /// <param name="tu"></param>
    void TuZusammengefuehrt(SgmTu tu);

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu aus dem Puffer abgerufen wird.
    /// </summary>
    /// <param name="tu"></param>
    void TuAbgerufen(SgmTu tu);

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu mit einem Teilauftrag verheiratet wird. Sie
    /// legt einen Eintrag in der AklTeilauftragTu Tabelle an.
    /// </summary>
    /// <param name="teilauftragId"></param>
    /// <param name="tu"></param>
    void TuMitTeilauftragVerheiraten(decimal teilauftragId, SgmTu tu);

    /// <summary>
    /// Liefert den Teilauftrag zu dem die übergebenen KommissioniertTu gehört
    /// </summary>
    /// <param name="tu">die KommissionierTu</param>
    [Synchronous]
    Teilauftrag GetTeilauftragZuTu(SgmTu tu);

      /// <summary>
    /// Liefert den Teilauftrag der Tu der Im Status "Aktiv" oder "Tabalare ausgelagert" ist
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="locationName"></param>
    /// <returns></returns>
    [Synchronous]
    Teilauftrag GetAktiverTeilauftragZuTu(SgmTu tu, string locationName);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
