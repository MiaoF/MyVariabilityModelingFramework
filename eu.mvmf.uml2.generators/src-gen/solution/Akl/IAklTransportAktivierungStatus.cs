//****************************************************************************
//******************************************************************************
// NAME: IAklTransportAktivierungStatus.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl
{
  public interface IAklTransportAktivierungStatus
  {
    /// <summary>
    /// Diese Funktion wird von der AklTransportAktivierung aufgerufen, wenn eine Tu
    /// eingelagert wurde.
    /// </summary>
    /// <param name="tu">die eingelagert Tu</param>
    void TuEingelagert(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird von der AklTransportAktivierung aufgerufen, wenn eine Tu
    /// ausgelagert wurde.
    /// </summary>
    /// <param name="tu">die eingelagert Tu</param>
    void TuAusgelagert(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird von der AklTransportAktivierung aufgerufen, wenn ein 
    /// Leerbehaelter-Auslagerauftrag gestartet wird.
    /// </summary>
    /// <param name="tu">die Tu</param>
    void LeerberhaelterAuslagerungBegonnen(SgmTu tu);

    /// <summary>
    /// Diese Funktion wird von der AklTransportAktivierung aufgerufen, wenn ein 
    /// Entnahme-Auslagerauftrag gestartet wird.
    /// </summary>
    /// <param name="teilauftrag"></param>
    /// <param name="zielArbeitsplatz"></param>
    void KommissionierAuslagerungBegonnen(Teilauftrag teilauftrag, Arbeitsplatz zielArbeitsplatz);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
