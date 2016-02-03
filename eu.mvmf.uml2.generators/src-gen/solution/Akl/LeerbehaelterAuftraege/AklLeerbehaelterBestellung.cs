//******************************************************************************
// NAME: AklLeerbehaelterBestellung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sgm.Akl.AutomatikTransporte;
using Sgm.Base.Auftraege;
using Sgm.Base.SgmPhysik;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.GenericPresenter;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Siemens.WarehouseManagement.Validation;
using Sgm.Akl.AuftraegeBearbeitung;
using Sgm.Base;

namespace Sgm.Akl.LeerbehaelterAuftraege
{
  /// <summary>
  /// Ermittelt und Reserviert Leerbehälter für die Auslagerung
  /// </summary>
  public class AklLeerbehaelterBestellung : IAklLeerbehaelterBestellung, IAklTransportAktivierungObserver, IAutomatikTransportObserver
  {

    #region properties
    //Logger
    private static log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }
    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    /// <summary>
    /// zugriff auf die AklTeilAuftragVerwaltung
    /// </summary>
    [RequiredProperty]
    public ITeilauftragVerwaltung TeilauftragVerwaltung { get; set; }

    /// <summary>
    /// Gets or sets die AklTransportGenerierung
    /// </summary>
    [RequiredProperty]
    public IAklTransportGenerierung AklTransportGenerierung { get; set; }

    private IAklTransportAktivierung _aklTransportAktivierung;

    /// <summary>
    /// Zugriff auf die AklTransportAktivierung.
    /// Beim setzen wird die ArbeitsplatzLogik als Observer der
    /// AklTransportAktivierung registriert.
    /// </summary>
    [RequiredProperty]
    public IAklTransportAktivierung AklTransportAktivierung
    {
      get
      {
        return _aklTransportAktivierung;
      }
      set
      {
        if (_aklTransportAktivierung != null)
        {
          _aklTransportAktivierung.RemoveObserver(this);
        }
        _aklTransportAktivierung = value;
        _aklTransportAktivierung.AddObserver(this);
      }
    }

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

    #endregion properties

    #region Implementation of ILeerbehaelterBestellung

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <param name="arbeitsplatz">
    /// Der Name des Arbeitsplatzes 
    /// </param>
    /// <param name="anzQuadranten">Anzahl der benötigten leeren Quadranten auf dem Tablar</param>
    /// <param name="hoehe">Die Tablar Hoehe 2,3 oder 4</param>
    /// <param name="hasRahmen">Mit Rahmen?</param>
    /// <returns>Die Tablarnummer</returns>
    public string LeerTablarAuslagern(string arbeitsplatz, int anzQuadranten, int hoehe, bool hasRahmen)
    {

      _log.DebugFormat("Starte Suche nach Tablar im Lager ([{0}] freie Quadranten, mit der Hoehe [{1}]00mm, mit Rahmen(?) [{2}] - Angefordert von [{3}]", anzQuadranten, hoehe, hasRahmen, arbeitsplatz);

      SgmTu dasLeereTablar = GetTablarFuerEinlagerung(anzQuadranten, hoehe, hasRahmen);

      string tuName = dasLeereTablar.Name;

      _log.InfoFormat("Tablar [{0}] auf Platz [{1}] fuer LeerbehaelterAnforderung gefunden", tuName, dasLeereTablar.LocName);

      // Leerbehälterauftrag ablegen
      LeerbehaelterAuftrag neuerLeerbAuftrag = DataAccess.Create<LeerbehaelterAuftrag>();
      neuerLeerbAuftrag.QuantAufTablare = (LeerbehaelterAuftrag.QuantAufTablareValue)anzQuadranten;
      neuerLeerbAuftrag.MarkRahmen = hasRahmen;
      neuerLeerbAuftrag.ZielLoc = arbeitsplatz;
      neuerLeerbAuftrag.Hoehe = (LeerbehaelterAuftrag.HoeheValue)dasLeereTablar.Hoehe;
      neuerLeerbAuftrag.StatusErmittlung = LeerbehaelterAuftrag.StatusErmittlungValue.Angelegt;

      // In Db eintragen und um die Id zu füllen
      DataAccess.Insert(neuerLeerbAuftrag);

      // Neue EinlagerReservierung für die Tu anlegen
      EinlagerReservierung neueEinlRes = DataAccess.Create<EinlagerReservierung>();
      neueEinlRes.Status = EinlagerReservierung.StatusValue.Angelegt;
      neueEinlRes.LeerAuftId = neuerLeerbAuftrag.Id;
      neueEinlRes.TruName = tuName;

      // Neue EinlagerReservierung in die Datenbank eintragen
      DataAccess.Insert(neueEinlRes);

      _log.InfoFormat("LeerbehaelterAuftrag [{0}] mit EinlagerReservierung [{1}] angelegt", neuerLeerbAuftrag.Id, neueEinlRes.Id);

      // Teilauftrag anlegen
      TeilauftragVerwaltung.ErzeugeLeerbehaelterTeilauftrag(neuerLeerbAuftrag.Id);

      _log.Debug("AklTransportgenerierung wird aufgeweckt");
      // Transportgenerierung anstoßen
      AklTransportGenerierung.Aufwecken();

      _log.DebugFormat("Suche nach leerem Tablar beendet. Tabler [{0}] gefunden.", tuName);

      return tuName;
    }

    public void DeleteNichtAktiviertenLeerbehaelterAuftrag(LeerbehaelterAuftrag leerbehaelterAuftrag)
    {
      _log.DebugFormat("Lösche nicht aktivierten Leerbehaelterauftrag [{0}]",leerbehaelterAuftrag);
      
      EinlagerReservierung einlagerReservierung = DataAccess.SelectFirst<EinlagerReservierung>(
        EinlagerReservierung.Properties.LeerAuftId.Filter(leerbehaelterAuftrag.Id));

      // Es gibt pro LeerbehaelterAuftrag immer genau einen Teilauftrag und eine Tu,
      // deshalb können der Teilauftrag und der TordRequest so einfach selectiert werden
      Teilauftrag teilauftrag =
        DataAccess.SelectFirst<Teilauftrag>(Teilauftrag.Properties.LeerbehaelAuftragId.Filter(leerbehaelterAuftrag.Id)); 
      
      TordRequest tordRequest =
        DataAccess.SelectFirst<TordRequest>(TordRequest.Properties.TeilauftragId.Filter(teilauftrag.Id));

      if (tordRequest != null)
      {
        DataAccess.Delete(tordRequest);
        _log.DebugFormat("TordRequest [{0}] gelöscht", tordRequest.Id);
      }

      if (teilauftrag != null)
      {
        TeilauftragVerwaltung.DeleteTeilauftrag(teilauftrag);
      }

      if (einlagerReservierung != null)
      {
        DeleteEinlagerReservierung(einlagerReservierung);
      }

      DeleteLeerbehaelterAuftrag(leerbehaelterAuftrag);

      _log.InfoFormat("Löschen des nicht aktivierten Leerbehaelterauftrags [{0}] erfolgreich abgeschlossen", leerbehaelterAuftrag);
    }

  

    #endregion Implementation of ILeerbehaelterBestellung

    #region Hilfsmethoden

    private SgmTu GetTablarFuerEinlagerung(int anzQuadranten, int hoehe, bool hasRahmen)
    {
      // TODO Query aus der Physic/Reservation Locks holen!
      // Mögliche Tablars Selectieren
      Query queryTu = GetEinlagerTablarQuery(hoehe, hasRahmen);

      CalculatedProperty cp = new CalculatedProperty("ANZ_QUADR_BELEGT", Expression.CountDistinct(SgmTuPos.Properties.Quant));
      queryTu.Select(cp);

      // Daraus Ermitteln der Freien Quadrante
      CalculatedProperty cp1 = new CalculatedProperty("ANZ_FREI_QUADR_FREI", Expression.Subtract(new ConstantExpression(4), cp));
      queryTu.Select(cp1);

      // Sind genug freie Quadranten verfügbar ?
      queryTu.Having(Expression.IsEqual(cp1, new ConstantExpression(anzQuadranten)));

      // Ausführen
      //IList<IDictionary<string, object>> result = DataAccess.ExecuteQuery(queryTu);
      IList<SgmTu> result = DataAccess.ExecuteQuery(queryTu, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      // Wird kein Tablar gefunden das passt, wird eine Exception an den Benutzer zurückgeworfen
      if (result.Count == 0)
      {
        
      }
      // Nicht mehr als 8 Basis Tu Pos (WHERE TUP_BASIS_TU_POS = null)

      foreach (SgmTu tu in result)
      {
        Query query = new Query("AnzBasisTuPos");
        query.From<SgmTuPos>();
        query.Where(Expression.And(SgmTuPos.Properties.TruName.Filter(tu.Name),
                                   SgmTuPos.Properties.BasisTuPos.Filter(null)));

        int anzahlTuPosProTu = DataAccess.ExecuteCountQuery(query, "AnzBasisTuPos");

        _log.DebugFormat("Leerbehaelter Suche: Tu [{0}] hat [{1}] Basis Tu Positionen", tu.Name, anzahlTuPosProTu);

        if(anzahlTuPosProTu < ConstDirectory.ANZAHL_BASIS_TU_POS_PRO_TU)
        {
          return tu;
        }
      }

      _log.InfoFormat("Aktuell sind keine Tablare mit [{0}] freien Quadranten in Hoehe [{1}] mit Rahmen [{2}] verfuegbar", anzQuadranten, hoehe, hasRahmen);
      throw new TablarNichtGefundenException();
    }

    public Query GetEinlagerTablarQuery(int hoehe, bool hasRahmen)
    {
      Query queryTu = new Query("DieMoeglichenTus");
      queryTu.From<SgmTu>();

      // Die vom richtigen Typ und der richtige Hoehe sind und (k)einen Rahmen haben
      queryTu.Where(Expression.IsEqual(SgmTu.Properties.TutName, new ConstantExpression("TAB")));
      queryTu.Where(Expression.IsEqual(SgmTu.Properties.Hoehe, new ConstantExpression((SgmTu.HoeheValue)hoehe)));
      queryTu.Where(Expression.IsEqual(SgmTu.Properties.MarkRahmen, new ConstantExpression(hasRahmen)));

      // Die im AKL-Lager stehen
      queryTu.InnerJoin<SgmLocation>(Expression.IsEqual(SgmTu.Properties.LocName, SgmLocation.Properties.Name));
      queryTu.Where(Expression.IsEqual(SgmLocation.Properties.L, new ConstantExpression("1000"))); // L=1000 <=> Lager=AKL
      queryTu.Where(Expression.IsEqual(SgmLocation.Properties.Type, new ConstantExpression(Location.TypeValue.BinPosition)));

      // Die nicht auf einem (für Auslagerung-)Reservierten Platz steht
      queryTu.Where(Location.Properties.MarkLockStore.Filter(false));
      queryTu.Where(Location.Properties.MarkLockRetrieve.Filter(false));

      // Die nicht für die Leerguteinlagerung reserviert sind
      queryTu.LeftOuterJoin<EinlagerReservierung>(Expression.IsEqual(SgmTu.Properties.Name,
                                                                     EinlagerReservierung.Properties.TruName));
      queryTu.Where(EinlagerReservierung.Properties.Id.Filter(null));

      // Die nicht für die Auslagerung reserviert sind
      TuPos.TuPosAlias tuPosAlias1 = TuPos.GetAlias("PosFuerAuslagerungReserviert");

      queryTu.LeftOuterJoin(tuPosAlias1, Expression.IsEqual(tuPosAlias1.TruName, SgmTu.Properties.Name));
      queryTu.LeftOuterJoin<Entnahme>(Expression.IsEqual(tuPosAlias1.Id, Entnahme.Properties.TupId));
      queryTu.Where(Entnahme.Properties.Id.Filter(null));

      // Ermittteln der Belegten Quadranten
      queryTu.LeftOuterJoin<SgmTuPos>(Expression.IsEqual(SgmTu.Properties.Name, SgmTuPos.Properties.TruName));
      queryTu.GroupBy<SgmTu>();

      return queryTu;
    }

    private void UpdateLeerbehaelterAuftrag(LeerbehaelterAuftrag leerbehaelterAuftrag)
    {
      DataAccess.Update(leerbehaelterAuftrag);
    }

    private void UpdateEinlagerReservierung(EinlagerReservierung einlagerReservierung)
    {
      DataAccess.Update(einlagerReservierung);
    }

    private void DeleteLeerbehaelterAuftrag(LeerbehaelterAuftrag leerbehaelterAuftrag)
    {
      DataAccess.Delete(leerbehaelterAuftrag);
      _log.DebugFormat("LeerbehaelterAuftrag  [{0}] gelöscht", leerbehaelterAuftrag.Id);
    }

    private void DeleteEinlagerReservierung(EinlagerReservierung einlagerReservierung)
    {
      DataAccess.Delete(einlagerReservierung);
      _log.DebugFormat("EinlagerReservierung  [{0}] gelöscht", einlagerReservierung.Id);
    }
    #endregion Hilfsmethoden

    #region Implementation of IAklTransportAktivierungObserver

    public void TuEingelagert(SgmTu tu)
    {
      // nichts zu tun 
      return;
    }

    /// <summary>
    /// Diese Funktion wird von der AklTransportAktivierung aufgerufen, wenn eine Tu
    /// ausgelagert wurde.
    /// </summary>
    /// <param name="tu">die eingelagert Tu</param>
    public void TuAusgelagert(SgmTu tu)
    {
      _log.Debug("Leertablar ausgelagert?");

      //Sucht den aktiven Leerbehaelterauftrag 
      Query query = new Query("LeerbAuftrZuTu");
      query.From<LeerbehaelterAuftrag>();
      query.InnerJoin<EinlagerReservierung>(Expression.IsEqual(EinlagerReservierung.Properties.LeerAuftId,
                                                               LeerbehaelterAuftrag.Properties.Id));
      query.Where(Expression.IsEqual(EinlagerReservierung.Properties.TruName, new ConstantExpression(tu.Name)));
      query.Where(Expression.LessThan(EinlagerReservierung.Properties.Status,
                                      new ConstantExpression(EinlagerReservierung.StatusValue.Abgeschlossen)));
      query.Where(LeerbehaelterAuftrag.Properties.ZielLoc.Filter(tu.LocName));

      IList<IDictionary<string, object>> result = DataAccess.ExecuteQuery(query);
      // Es darf nur eine EinlagerReservierung pro Tablar geben
      if (result.Count != 1)
      {
        _log.Debug("Es wurde kein Leertablar ausgelagert.");
        // Auslagerung war nicht für einen Leerbehaelter Auftrag daher nichts zu tun
        return;
      }

      // Status der EinlagerReservierung und des LeerbehaelterAuftrags auf abgeschlossen setzen
      LeerbehaelterAuftrag leerbehaelterAuftrag = (LeerbehaelterAuftrag)result[0][LeerbehaelterAuftrag.Properties.AliasName];
      EinlagerReservierung einlagerReservierung = (EinlagerReservierung)result[0][EinlagerReservierung.Properties.AliasName];

      einlagerReservierung.Status = EinlagerReservierung.StatusValue.Abgeschlossen;
      UpdateEinlagerReservierung(einlagerReservierung);
      leerbehaelterAuftrag.StatusErmittlung = LeerbehaelterAuftrag.StatusErmittlungValue.Abgeschlossen;
      UpdateLeerbehaelterAuftrag(leerbehaelterAuftrag);
      
      _log.DebugFormat("Leerbehaelterauftrag [{0}] auf Status [{1}] gesetzt", leerbehaelterAuftrag.Id, leerbehaelterAuftrag.StatusErmittlung);

      // Status des Teilauftrags auf abgeschlossen setzen
      TeilauftragVerwaltung.SetzeTeilauftragStatusFuerLeerbehaelterauftrag(leerbehaelterAuftrag, Teilauftrag.StatusValue.Fertig);

      // Die abgeschlossene EinlagerReservierung den abgeschlossenen Leerbehaelterauftrag löschen 
      DeleteEinlagerReservierung(einlagerReservierung);
      DeleteLeerbehaelterAuftrag(leerbehaelterAuftrag);

      return;
    }

    public void LeerberhaelterAuslagerungBegonnen(SgmTu tu)
    {
      _log.Debug("Leertablar-Auslagerung angestossen?");

      //Sucht den aktiven Leerbehaelterauftrag 
      Query query = new Query("LeerbAuftrZuTu");
      query.From<LeerbehaelterAuftrag>();
      query.InnerJoin<EinlagerReservierung>(Expression.IsEqual(EinlagerReservierung.Properties.LeerAuftId,
                                                               LeerbehaelterAuftrag.Properties.Id));
      query.Where(Expression.IsEqual(EinlagerReservierung.Properties.TruName, new ConstantExpression(tu.Name)));
      query.Where(Expression.LessThan(EinlagerReservierung.Properties.Status,
                                      new ConstantExpression(EinlagerReservierung.StatusValue.TordAktiv)));

      IList<IDictionary<string, object>> result = DataAccess.ExecuteQuery(query);

      if (result.Count == 0)
      {
        _log.Debug("Es wurde keine Leertablar-Ausgelagerung angestoßen.");
        // Auslagerung war nicht für einen Leerbehaelter Auftrag daher nichts zu tun
        return;
      }

      LeerbehaelterAuftrag leerbehaelterAuftrag = (LeerbehaelterAuftrag)result[0][LeerbehaelterAuftrag.Properties.AliasName];
      EinlagerReservierung einlagerReservierung = (EinlagerReservierung)result[0][EinlagerReservierung.Properties.AliasName];

      leerbehaelterAuftrag.StatusErmittlung = LeerbehaelterAuftrag.StatusErmittlungValue.AuslagerungGestartet;
      UpdateLeerbehaelterAuftrag(leerbehaelterAuftrag);
      einlagerReservierung.Status = EinlagerReservierung.StatusValue.TordAktiv;
      UpdateEinlagerReservierung(einlagerReservierung);


      TeilauftragVerwaltung.SetzeTeilauftragStatusFuerLeerbehaelterauftrag(leerbehaelterAuftrag, Teilauftrag.StatusValue.Aktiv);
    }

    public void KommissionierAuslagerungBegonnen(Teilauftrag teilauftrag, Arbeitsplatz zielArbeitsplatz)
    {
      //nichts
    }

    #endregion

    #region Implementation of IAutomatikTransportObserver

    public void TuAmPufferHeber(SgmTu tu, IDictionary<string, string> additionalPlcData)
    {
      // nichts
    }

    public void TuAmArbeitsplatz(SgmTu tu, SgmLocation location)
    {
      _log.DebugFormat("TuAmArbeitsplatz für Tu [{0}] am Platz [{1}] wird gestartet", tu.Name, location.Name);
      // EinlaegrReservierung updaten falls es eine gibt
      IList<EinlagerReservierung> einlagerReservierungen = DataAccess.SelectAll<EinlagerReservierung>(Expression.And(
                                                                                                        EinlagerReservierung.Properties.TruName.Filter(tu.Name),
                                                                                                        EinlagerReservierung.Properties.Status.Filter(EinlagerReservierung.StatusValue.TordAktiv)));

      foreach (EinlagerReservierung einlagerReservierung in einlagerReservierungen)
      {
        einlagerReservierung.Status = EinlagerReservierung.StatusValue.AmArbeitsplatz;
        UpdateEinlagerReservierung(einlagerReservierung);
        _log.InfoFormat("EinlagerReservierung [{0}] für Tu [{1}] auf Status [AmArbeitsplatz] gesetzt.", einlagerReservierung.Id, einlagerReservierung.TruName);
      }
    }

    public void TransportAbgeschlossen(SgmTu tu, TransportOrder tord)
    {
      //nichts
    }

    public void TuAmZusammenfuehrPlatz(SgmTu tu)
    {
      //nichts
    }

    public void TuGepuffert(SgmTu tu)
    {
      //nichts
    }

    public void TuAmUebergabePlatzVerschiebewagen(SgmTu tu)
    {
      //nichts
    }

    public void TuAmRbgAbgabeplatz(SgmTu tu)
    {
      //nichts
    }

    #endregion
  }
}

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
