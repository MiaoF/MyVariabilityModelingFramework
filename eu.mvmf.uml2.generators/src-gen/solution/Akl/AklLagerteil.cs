
//******************************************************************************
// NAME: AklLagerteil.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sgm.Akl.Arbeitsplaetze;
using Sgm.Akl.AuftraegeBearbeitung;
using Sgm.Akl.AutomatikTransporte;
using Sgm.Akl.Puffer;
using Sgm.Base;
using Sgm.Base.Auftraege;
using Sgm.Base.SgmMaterialVerwaltung;
using Sgm.Base.SgmPhysik;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl
{
  /// <summary>
  /// Dient als Schnittstelle zum Zugriff auf die AklSteuerung
  /// </summary>
  public class AklLagerteil : LagerteilBase, IAklLagerteil, IAutomatikTransportObserver
  {
    private static log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Properties/Fields

    [RequiredProperty]
    public ISgmMaterialManagement MaterialManagement { get; set; }

    [RequiredProperty]
    public ISgmPhysik Physik { get; set; }

    [RequiredProperty]
    public IZyklisch AklAuftragAktivierung { get; set; }

    /// <summary>
    /// Zugriff auf die PufferVerwaltung
    /// </summary>
    [RequiredProperty]
    public IPufferVerwaltung PufferVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf den MessageStore
    /// </summary>
    [RequiredProperty]
    public IMessageAndExceptionHandling MessageStore { get; set; }

    /// <summary>
    /// Zugriff auf die AklTransportGenerierung
    /// </summary>
    [RequiredProperty]
    public IAklTransportGenerierung AklTransportGenerierung { get; set; }

    /// <summary>
    /// Zugriff auf die AklTransportAktivierung
    /// </summary>
    [RequiredProperty]
    public IZyklisch AklTransportAktivierung { get; set; }

    private IAutomatikTransport _automatikTransport;

    /// <summary>
    /// Zugriff auf die AutomatikTransport-Komponente
    /// Beim setzten wird die AklTransportAktivierung als Observer bei
    /// AutomatikTransport angemeldet. 
    /// </summary>
    [RequiredProperty]
    public IAutomatikTransport AutomatikTransport
    {
      get
      {
        return _automatikTransport;
      }
      set
      {
        if (_automatikTransport != null)
        {
          ((AutomatikTransport)_automatikTransport).RemoveObserver(this);
        }

        _automatikTransport = value;
        ((AutomatikTransport)_automatikTransport).AddObserver(this);
      }
    }

    #endregion Properties/Fields

    #region Implementation of ILagerteil

    /// <summary>
    /// Ermittelt wie viel Material in einem Lagerteil noch zur Verf�gung steht.
    /// </summary>
    /// <param name="materialId">das ben�tigte Material</param>
    /// <param name="ignoriereMaximaleKommissionierMenge">soll die maximale Kommisioniermenge beachtet werden.</param>
    /// <param name="bestandsart"></param>
    /// <param name="sonderbestandsNummer"></param>
    /// <returns>Menge an verf�gbarem Material</returns>
    public override decimal ErmittleVerfuegbareMenge(decimal materialId, bool ignoriereMaximaleKommissionierMenge, TeilauftragPos.BestandsartValue bestandsart, string sonderbestandsNummer, bool gesperrtesMaterial)
    {
      return ErmittleVerfuegbareMenge(materialId, ignoriereMaximaleKommissionierMenge, bestandsart,
                                           sonderbestandsNummer, gesperrtesMaterial, Lagerteil.Akl);
    }

    private SgmTuPos.BestandsartValue GetTuPosBestandsart(TeilauftragPos.BestandsartValue bestandsart)
    {
      
    }

    /// <summary>
    /// Erzeugt einen TeilAuftrag.
    /// </summary>
    /// <param name="auftragID"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override decimal ErzeugeTeilauftrag(decimal auftragID, Teilauftrag.TypValue type)
    {
      // Um Lagerteil AKL anreichern 
      return BaseTeilauftragVerwaltung.ErzeugeTeilauftrag(auftragID, Teilauftrag.LagerTeilValue.Akl, type);
    }


    /// <summary>
    /// Erzeugt eine TeilauftragPosition.
    /// </summary>
    /// <param name="teilauftragID">The teilauftrag ID.</param>
    /// <param name="materialID">The material ID.</param>
    /// <param name="menge">The menge.</param>
    /// <param name="bestandsArt">The bestands art.</param>
    /// <param name="sonderbestandsNummer">The sonderbestands nummer.</param>
    /// <param name="gesperrtesMaterialAuslagern">flag f�r die Auslagerung von gesperrtem Material</param>
    public override void ErzeugeTeilauftragPosition(decimal teilauftragID, decimal materialID, decimal menge, TeilauftragPos.BestandsartValue bestandsArt, string sonderbestandsNummer, bool gesperrtesMaterialAuslagern)
    {
      // Weiterreichen
      BaseTeilauftragVerwaltung.ErzeugeTeilauftragPosition(teilauftragID, materialID, menge, bestandsArt, sonderbestandsNummer, gesperrtesMaterialAuslagern);
    }

    /// <summary>
    /// Beendet den Anlegevorgang
    /// </summary>
    /// <param name="teilauftragId">die Id des Teilauftrags</param>
    public override void TeilauftragFertigAngelegt(decimal teilauftragId)
    {
      BaseTeilauftragVerwaltung.TeilauftragFertigAngelegt(teilauftragId);

      AklAuftragAktivierung.Aufwecken();
    }

    /// <summary>
    /// Ruft alle Tus zum �bergebenen Teilauftrag zum Zusammenf�hrplatz ab.
    /// </summary>
    /// <param name="auftragId">die AuftragsId des Teilauftrags</param>
    public override void TeilauftragAbrufen(decimal auftragId)
    {
     
    }

    /// <summary>
    /// Ein Nachschubauftrag wird verbucht.
    /// </summary>
    public override void NachschubVerbuchen(string nachschubTuName, Auftrag auftrag)
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Storniert einen Teilauftrag.
    /// </summary>
    /// <param name="auftragId"></param>
    /// <param name="material"></param>
    /// <param name="menge">die zu stornierende Menge</param>
    public override decimal StorniereBestellungFuerTeilauftrag(decimal auftragId, SgmMaterial material, decimal menge)
    {
    }

    #endregion

    #region Implementation of IAklLagerteil

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu in einen Pufferplatz transportiert wurde. Sie setzt den Status der Entnahme auf "fertig"
    /// und pr�ft notwendige Status�nderungen des Auftrags.
    /// </summary>
    /// <param name="tu">die gepufferte Tu</param>
    public void TuBereitgestellt(SgmTu tu)
    {
    }

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu vom Puffer zum Zusammenf�hrplatz transportiert wurde.
    /// </summary>
    /// <param name="tu"></param>
    public void TuZusammengefuehrt(SgmTu tu)
    {
    }

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu aus dem Puffer abgerufen wird.
    /// </summary>
    /// <param name="tu"></param>
    public void TuAbgerufen(SgmTu tu)
    {
    }

    /// <summary>
    /// Wird aufgerufen, wenn eine Tu mit einem Teilauftrag verheiratet wird. Sie
    /// legt einen Eintrag in der AklTeilauftragTu Tabelle an.
    /// </summary>
    /// <param name="teilauftragId"></param>
    /// <param name="tu"></param>
    public void TuMitTeilauftragVerheiraten(decimal teilauftragId, SgmTu tu)
    {
    }

    /// <summary>
    /// Liefert den Teilauftrag zu dem die �bergebenen KommissioniertTu geh�rt
    /// </summary>
    /// <param name="tu">die KommissionierTu</param>
    public Teilauftrag GetTeilauftragZuTu(SgmTu tu)
    {
    }

    /// <summary>
    /// Liefert den Teilauftrag der Tu der Im Status "Aktiv" oder "Tabalare ausgelagert" ist
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="locationName"></param>
    /// <returns></returns>
    public Teilauftrag GetAktiverTeilauftragZuTu(SgmTu tu, string locationName)
    {
    }

    #endregion

    #region Implementation of IAutomatikTransportObserver

    public void TuAmPufferHeber(SgmTu tu, IDictionary<string, string> additionalPlcData)
    {
      //leer
    }

    public void TuAmArbeitsplatz(SgmTu tu, SgmLocation location)
    {
      //leer
    }

    public void TransportAbgeschlossen(SgmTu tu, TransportOrder tord)
    {
      //leer
    }

    public void TuAmZusammenfuehrPlatz(SgmTu tu)
    {
      //leer
    }

    public void TuGepuffert(SgmTu tu)
    {
      //leer
    }

    public void TuAmUebergabePlatzVerschiebewagen(SgmTu tu)
    {
      //leer
    }

    public void TuAmRbgAbgabeplatz(SgmTu tu)
    {
    }

    #endregion

    #region Hilfsfunktionen

    #region Data Access

    private void UpdateAklTeilauftragTu(AklTeilauftragTu aklTeilauftragTu)
    {
      DataAccess.Update(aklTeilauftragTu);
    }

    private void InsertAklTeilauftragTu(AklTeilauftragTu teilauftragTu)
    {
      DataAccess.Insert(teilauftragTu);
    }

    // TODO: Wo ist das DeleteAklTeilauftragTu()?

    #endregion
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************