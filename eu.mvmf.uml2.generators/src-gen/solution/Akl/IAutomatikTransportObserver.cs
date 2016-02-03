//****************************************************************************
//****************************************************************************
//  NAME: IAutomatikTransportObserver.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System.Collections.Generic;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.TransportManagement;

namespace Sgm.Akl
{
  /// <summary>
  /// Observer Interface für die AutomatikTransport-Komponente des AKL
  /// </summary>
  public interface IAutomatikTransportObserver
  {
    /// <summary>
    /// Wird aufgerufen, wenn sich eine Tu am Scanner im Pufferbereich gemeldet hat.
    /// Es wird ein Pufferplatz gesucht und dort abgestellt.
    /// </summary>
    /// <param name="tu">die Tu</param>
    /// <param name="additionalPlcData"></param>
    void TuAmPufferHeber(SgmTu tu, IDictionary<string, string> additionalPlcData);

    /// <summary>
    /// Eine Tu ist zum Arbeitsplatz hingefahren.
    /// </summary>
    /// <param name="tu">die Tu</param>
    /// <param name="location">die Location an der sich die Tu befindet</param>
    void TuAmArbeitsplatz(SgmTu tu, SgmLocation location);

    /// <summary>
    /// Diese Funktion wird von AutomatikTransport aufgerufen, wenn ein Transport abgeschlossen
    /// wurde.
    /// </summary>
    /// <param name="tu">Die transportierte Tu</param>
    /// <param name="tord">Der abgeschlossene Tord</param>
    void TransportAbgeschlossen(SgmTu tu, TransportOrder tord);

    /// <summary>
    /// Diese Funktion wird von AutomatikTransport aufgerufen, wenn eine Tu vom Puffer zum Zusammenführplatz
    /// transportiert wurde.
    /// </summary>
    /// <param name="tu">Die transportierte Tu</param>
    void TuAmZusammenfuehrPlatz(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird von AutomatikTransport aufgerufen, wenn eine Tu gepuffert wurde.
    /// </summary>
    /// <param name="tu">Die transportierte Tu</param>
    void TuGepuffert(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird aufgerufen wenn sich eine Tu am Uebergabeplatz des Verschiebewagens meldet.
    /// </summary>
    /// <param name="tu">die Tu</param>
    void TuAmUebergabePlatzVerschiebewagen(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird auferufen, wenn eine Tu am Rbg abgabeplatz gemeldet wird
    /// </summary>
    /// <param name="tu"></param>
    void TuAmRbgAbgabeplatz(SgmTu tu);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
