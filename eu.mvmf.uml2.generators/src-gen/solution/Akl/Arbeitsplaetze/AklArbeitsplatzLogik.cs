//******************************************************************************
//******************************************************************************
// NAME: AklArbeitsplatzLogik.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Commons;
using Sgm.Akl.AutomatikTransporte;
using Sgm.Akl.LeerbehaelterAuftraege;
using Sgm.Base;
using Sgm.Base.Auftraege;
using Sgm.Base.Drucken;
using Sgm.Base.Inventur;
using Sgm.Base.SgmMaterialVerwaltung;
using Sgm.Base.SgmPhysik;
using Sgm.Base.AvisenAuftraege;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.GenericPresenter;
using Siemens.WarehouseManagement.Infrastructure.SystemParameters;
using Siemens.WarehouseManagement.MaterialManagement;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics.Queries;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Validation;
using Sgm.Akl.AuftraegeBearbeitung;
using Solution.Commons;
using Siemens.WarehouseManagement.ContractInfrastructure.SystemParametersPresenter;

namespace Sgm.Akl.Arbeitsplaetze
{
  /// <summary>
  /// Stellt die Logik für die Arbeitsplätze zur Verfügung z.B. Wechsel der Betriebsart, Anforderung Leergut
  /// </summary>
  public class AklArbeitsplatzLogik : IAklArbeitsplatzLogik, IAklTransportAktivierungObserver
  {
    /// <summary>
    /// logger
    /// </summary>
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Required Properties
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
    /// Zugriff auf die Physik
    /// </summary>
    [RequiredProperty]
    public ISgmPhysik SgmPhysik { get; set; }

    /// <summary>
    /// Zugriff auf das Sgm MaterialManagement
    /// </summary>
    [RequiredProperty]
    public ISgmMaterialManagement SgmMaterialManagement { get; set; }


    /// <summary>
    /// Zugriff auf das Sgm MaterialManagement
    /// </summary>
    [RequiredProperty]
    public IAklArbeitsplatzVerwaltung AklArbeitsplatzVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf die Leerbehaelter Bestellung
    /// </summary>
    [RequiredProperty]
    public IAklLeerbehaelterBestellung AklLeerbehaelterBestellung { get; set; }

    /// <summary>
    /// Zugriff auf die AvisenVerwaltung
    /// </summary>
    [RequiredProperty]
    public IAvisenVerwaltung AvisenVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf die AklTransportGenerierung
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

    /// <summary>
    /// Zugriff auf die TeilauftragVerwaltung der Base
    /// </summary>
    [RequiredProperty]
    public ITeilauftragVerwaltung TeilauftragVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf das AklLagerteil
    /// </summary>
    [RequiredProperty]
    public IAklLagerteil Lagerteil { get; set; }

    /// <summary>
    /// zugriff auf den belegDruck
    /// </summary>
    [RequiredProperty]
    public IBelegDruck BelegDruck { get; set; }

    /// <summary>
    /// Zugriff auf die InventurVerwaltung
    /// </summary>
    [RequiredProperty]
    public IInventurVerwaltung InventurVerwaltung { get; set; }

    /// <summary>
    /// Gets or sets the system parameters access presenter.
    /// </summary>
    [RequiredProperty]
    public ISystemParameters SystemParameters { get; set; }

    // Zugriff auf die InventurAuslagerung
    [RequiredProperty]
    public IZyklisch InventurAuslagerung { get; set; }

    #endregion Required Properties

    #region Implementierung IAklArbeitsplatzLogik

    /// <summary>
    /// Liefert das Tablar, die auf dem übergebenen Arbeitsplatz gebucht ist.
    /// </summary>
    /// <remarks>
    /// Liefert null wenn keine Tu auf dem Arbeitsplatz gebucht ist
    /// </remarks>
    /// <param name="arbeitsplatzLocName">Der Location Name des Arbeitsplatzes</param>
    /// <returns>Die Tu (Tablar) die auf dem Arbeitsplatz gebucht ist</returns>
    public SgmTu GetTablarDaten(string arbeitsplatzLocName)
    {
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      _log.DebugFormat("Suche Tablar auf Arbeitsplatz-Location [{0}]", arbeitsplatzLocName);
      Query query = SgmPhysik.GetTusOn(arbeitsplatzLocName);
      query.OrderBy(OrderByDirection.Desc(Tu.Properties.OrderMove));

      //Ermittle die vorderste Tu
      IList<SgmTu> sgmTus = DataAccess.ExecuteQuery(query, row => (SgmTu)row[SgmTu.Properties.AliasName]);
      if (sgmTus.Count == 0)
      {
        return null;
      }
      SgmTu sgmTu = sgmTus.First();

      //Nur wenn die Tu keinen Einlager-TordRequest hat
      IList<TordRequest> tordRequests = DataAccess.SelectAll<TordRequest>(
         Expression.And(TordRequest.Properties.TuName.Filter(sgmTu.Name),
                                    TordRequest.Properties.TransportTyp.Filter(TordRequest.TransportTypValue.Einlagerung)));

      if (tordRequests.Count != 0)
      {
        _log.DebugFormat("Einlagern: Es existiert bereits ein Einlager-TordRequest für Tu [{0}].", sgmTu.Name);
        //weckt die TransportAktivierung auf. Diese versucht einen Tord zu erzeugen
        AklTransportAktivierung.Aufwecken();
        return null;
      }
      //Nur wenn die Tus keinen Tord hat
      IList<Tord> tords = DataAccess.SelectAll<Tord>(Tord.Properties.TruName.Filter(sgmTu.Name));
      if (tords.Count != 0)
      {
        Tord tord = tords.First();
        _log.DebugFormat("Einlagern: Es existiert bereits ein Tord [{0}] für Tu [{1}] von [{2}] nach [{3}].", tord.Id, sgmTu.Name, tord.LocNameFrom, tord.LocNameTo);
        return null;
      }

      return sgmTu;
    }

    /// <summary>
    /// Liefert eine Query, die alle TuPosen der übergebenen Tu enthält
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns>eine TuPos Query</returns>
    public IList<SgmTuPos> GetTablarInhalt(string tuName)
    {
      if (string.IsNullOrEmpty(tuName)) { throw new ArgumentNullException("tuName"); }

      Query query = GetTablarInhaltQuery(tuName);
      query.OrderBy(SgmTuPos.Properties.Quant);
      IList<SgmTuPos> tuPosen = DataAccess.ExecuteQuery(query, row => (SgmTuPos)row[SgmTuPos.Properties.AliasName]);

      return tuPosen;
    }

    /// <summary>
    /// Liefert eine Query, die alle TuPosen der übergebenen Tu enthält
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns>eine TuPos Query</returns>
    public Query GetTablarInhaltQuery(string tuName)
    {
      if (string.IsNullOrEmpty(tuName)) { throw new ArgumentNullException("tuName"); }

      Query query = SgmMaterialManagement.GetAllTuPos("tuPosQuery");
      query.Where(SgmTuPos.Properties.TruName.Filter(tuName));

      return query;
    }

    /// <summary>
    /// Liefert das Material zu den übergebenen Sap Daten
    /// </summary>
    /// <param name="taNum">die Sap TaNum</param>
    /// <param name="taPos">die Sap TaPos</param>
    /// <param name="weNum">die Sap WeNum</param>
    /// <param name="wePos">die Sap WePos</param>
    /// <returns>das gesuchte Material</returns>
    public SgmMaterial GetMaterial(string taNum, string taPos, string weNum, string wePos)
    {
      if (string.IsNullOrEmpty(weNum) && string.IsNullOrEmpty(taNum))
      {
        throw new ArgumentNullException("weNum oder taNum");
      }

      if (string.IsNullOrEmpty(wePos) && string.IsNullOrEmpty(taPos))
      {
        throw new ArgumentNullException("wePos oder taPos");
      }

      AvisePos avisePos = AvisenVerwaltung.GetAvisePosition(weNum, wePos, taNum, taPos);

      return (SgmMaterial)SgmMaterialManagement.GetMaterialById(avisePos.MatId);
    }

    /// <summary>
    /// Liefert das Material zu der übergebenen Logition Material Id
    /// </summary>
    /// <param name="id">die Logition Material id</param>
    /// <returns></returns>
    public SgmMaterial GetMaterial(decimal id)
    {
      if (id <= 0)
      {
        throw new ArgumentNullException("id");
      }

      return (SgmMaterial)SgmMaterialManagement.GetMaterialById(id);
    }

    /// <summary>
    /// Liefert die AvisePos zu den übergebenen Sap-Daten.
    /// Falls keine oder mehere AvisePos gefunden werden, wird eine AvisePositionNichtGefundenException geworden
    /// </summary>
    /// <param name="weNum">die Sap We-Nummer</param>
    /// <param name="wePos">die Sap We-Position</param>
    /// <param name="taNum">die Sap Ta-Nummer</param>
    /// <param name="taPos">die Sap Ta-Pos</param>
    /// <returns>die AvisePosition</returns>
    public AvisePos GetAvisePositon(string weNum, string wePos, string taNum, string taPos)
    {
      _log.InfoFormat(
        "Suche AvisePosition zu folgenden Daten: weNum = [{0}], wePos = [{1}], taNum = [{2}], taPos = [{3}]", weNum,
        wePos, taNum, taPos);

      AvisePos avisePos = AvisenVerwaltung.GetAvisePosition(weNum, wePos, taNum, taPos);

      return avisePos;
    }

    /// <summary>
    /// Liefert die Menge, die maximal noch für die übergebenen AvisePos eingelagert
    /// werden darf.
    /// </summary>
    /// <param name="avisePos"></param>
    /// <returns></returns>
    public decimal GetMaxZulagerMenge(AvisePos avisePos)
    {
      _log.DebugFormat("Berechne MaxZulagermenge für AvisePos [{0}]", avisePos.Id);

      decimal maxZulagerMenge = AvisenVerwaltung.GetMaxZulagerbareMenge(avisePos);

      _log.InfoFormat("Maximale Zulagermenge für AvisePos [{0}] ist [{1}]", avisePos.Id, maxZulagerMenge);

      return maxZulagerMenge;
    }

    /// <summary>
    /// Überprüft, ob ein Material gewogen werden muss.
    /// </summary>
    /// <param name="material">das Material</param>
    /// <returns>true oder false</returns>
    public bool MussMaterialGewogenWerden(SgmMaterial material)
    {
      if (material == null)
      {
        throw new ArgumentNullException("material");
      }

      return SgmMaterialManagement.MussMaterialGewogenWerden(material);
    }

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <returns>Die Tablarnummer</returns>
    public string LeerTablarAuslagern(string arbeitsplatzLocName, int anzQuanten, int hoehe, bool hasRahmen)
    {
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }
      var arbeitsplatz = AklArbeitsplatzVerwaltung.GetArbeitsplatzByLocation(arbeitsplatzLocName);
      if (arbeitsplatz.Modus != Arbeitsplatz.ModusValue.Einlagerung)
      {
        string text = string.Format("AKL Arbeitsplatz [{0}] befindet sich nicht im Modus [Einlagerung], sondern im Modus [{1}].  Es wird kein Leertablar angefordert.", arbeitsplatzLocName, arbeitsplatz.Modus);
        throw new SgmException(text, text);
      }
      if (arbeitsplatz.ModusAngefordert != Arbeitsplatz.ModusAngefordertValue.Einlagerung)
      {
        string text = string.Format("Für AKL Arbeitsplatz [{0}] ist der  Modus [{1}] angefordert. Es wird kein Leertablar angefordert.", arbeitsplatzLocName, arbeitsplatz.Modus);
        throw new SgmException(text, text);
      }

      // Wird in der AklLeerbehaelterbestellung bereitgestellt
      return AklLeerbehaelterBestellung.LeerTablarAuslagern(arbeitsplatzLocName, anzQuanten, hoehe, hasRahmen);
    }


    /// <summary>
    /// Legt eine Tu-Pos an und verlinkt sie mit der Avise-Pos
    /// </summary>
    /// <param name="tuName">Die Tu auf der sich das Material befindet</param>
    /// <param name="quanten">Eine Liste mit den Quanten auf denen das Material gelegt wurde</param>
    /// <param name="menge">Die Menge des Materials</param>
    /// <param name="weNum">Die gescannte Sap Wareneingangs-Nummer. Entweder TaNum oder WeNum muss gesetzt sein</param>
    /// <param name="wePos">Die gescannte Sap Wareneingangs-Position. Entweder TaPos oder WePos muss gesetzt sein</param>
    /// <param name="taNum">Die gescannte Sap TaNum</param>
    /// <param name="taPos">Die gescannte Sap TaPos</param>
    /// <param name="gewicht">Das Gewicht des einzelnen Materials</param>
    /// <param name="terminalName"></param>
    /// <param name="userName"></param>
    /// <param name="endQuittierungsKennzeichen"></param>
    public bool MaterialZugelagert(string tuName, IList<int> quanten, decimal menge, string weNum, string wePos, string taNum, string taPos, decimal gewicht, string terminalName, string userName, bool endQuittierungsKennzeichen)
    {
      if (string.IsNullOrEmpty(tuName))
      {
        throw new ArgumentNullException("tuName");
      }

      if (gewicht < 0)
      {
        throw new ArgumentOutOfRangeException("gewicht");
      }

      if (quanten == null)
      {
        throw new ArgumentNullException("quanten");
      }

      if (menge <= 0)
      {
        throw new ArgumentOutOfRangeException("menge");
      }

      if (string.IsNullOrEmpty(weNum) && string.IsNullOrEmpty(taNum))
      {
        throw new ArgumentException("weNum oder taNum muss gesetzt sein.");
      }

      if (string.IsNullOrEmpty(wePos) && string.IsNullOrEmpty(taPos))
      {
        throw new ArgumentException("wePos oder taPos muss gesetzt sein.");
      }

      //Zuerst muss die zugehörige AvisePos gefunden werden
      AvisePos avisePos = AvisenVerwaltung.GetAvisePosition(weNum, wePos, taNum, taPos);

      SgmTu tu = (SgmTu)SgmPhysik.GetTu(tuName);
      if (tu == null)
      {
        throw new TuNotFoundException(tuName);
      }

      SgmMaterial material = SgmMaterialManagement.GetMaterialById(avisePos.MatId);

      if (material == null)
      {
        throw new MaterialNichtGefundenException(avisePos.MatId.ToString(), "Unbekanntes Material");
      }

      // Bestimmt die Menge die noch maximal zugelagert werden darf, ohne die AvisePos-Sollmenge zu überschreiten
      decimal maxZulagerbareMenge = AvisenVerwaltung.GetMaxZulagerbareMenge(avisePos);

      if (menge > maxZulagerbareMenge)
      {
        throw new AvisePosSollmengeUeberschrittenException(avisePos.SollMenge, avisePos.Id, maxZulagerbareMenge);
      }

      //legt für jeden belegten Quadranten eine TuPos an. Nur eine TuPos erhält die übergebenen Materialmenge
      //alle anderen werden mit der Menge 0 belegt und nicht mit der AvisePos verknüpft.
      int counter = 0;
      SgmTuPos basisTuPos = null;
      foreach (int quant in quanten)
      {
        if (counter++ == 0)
        {
          basisTuPos = SgmMaterialManagement.TuPosAnlegenOderErweitern(tu.Name, material.Id, menge, avisePos.MindestHaltbarkeit,
                                                          avisePos.Sonderbestandsnummer, quant, (decimal)avisePos.Bestandskennzeichen, taNum, null);

          // Falls die Avise eine Transfer TuPos hat muss das ursprüngliche Einlagerdatum beibehalten werden.
          if (avisePos.TransferTupId.HasValue)
          {
            SgmTuPos transferTuPos = SgmMaterialManagement.GetTuPos(avisePos.TransferTupId.Value);
            if (transferTuPos != null)
            {
              _log.InfoFormat("Für Avise Pos [{0}] existiert Transfert TuPos [{1}] Transfer Tu [{2}] ursprüngliches Einlagerdatum [{3}] wird übernommen",
                avisePos.Id, transferTuPos.Id, transferTuPos.TruName, transferTuPos.DatiEinlagerung);

              basisTuPos.DatiEinlagerung = transferTuPos.DatiEinlagerung;
              SgmMaterialManagement.UpdateTuPos(basisTuPos);
            }
          }

          //Stellt die Verlinkung zwischen AvisePos und der TuPos her
          AvisenVerwaltung.MaterialZugelagert(avisePos, basisTuPos, menge);

          //Im AKL wird für jede Einlagerung eine neue TuPos angelegt. Die Menge der TuPos wurde gezählt, deshalb muss der
          //Inventurstempel gesetzt werden
          InventurVerwaltung.InventurDurchgefuehrt(basisTuPos, basisTuPos.Amount, userName, terminalName,
                                                   InventurZaehlung.InventurArtValue.WareneingangInventur);

          BelegDruck.DruckeBelegEinlagerungZulagerung(BelegSzenario.DruckZeitpunktValue.EinlagernZulagern, avisePos, basisTuPos, terminalName, DruckBereich.Akl, userName);

          _log.InfoFormat("Material [{0}] für AvisePosition [{1}] auf TuPos [{2}] zugelagert. Menge [{3}]", material.Description, avisePos.Id, basisTuPos.Id, basisTuPos.Amount);
        }
        else
        {
          if (basisTuPos == null)
          {
            continue;
          }
          SgmTuPos tuPos = SgmMaterialManagement.TuPosOhneMengeAnlegenOderErweitern(basisTuPos.Id, tu.Name, material.Id, avisePos.MindestHaltbarkeit,
                                                                       avisePos.Sonderbestandsnummer, quant, (decimal)avisePos.Bestandskennzeichen, taNum, null);

          _log.InfoFormat("SgmTuPos [{0}] auf SgmTu [{1}] auf Quadrant [{2}] angelegt. Menge [{3}]", tuPos.Id, tu.Name, tuPos.Quant, tuPos.Amount);
        }
      }

      if (gewicht != 0)
      {
        SgmMaterialManagement.MaterialGewogen(material, gewicht);
      }

      // Wenn das Endquittierungskennzeichen vom Benutzer gesetzt wurde, wird
      // versucht die AvisePos zu schließen
      if (endQuittierungsKennzeichen)
      {
        AvisePosSchliessen(avisePos);
      }

      // Gibt true zurück wenn die gesamte Menge eingelagert wurde
      return maxZulagerbareMenge == menge;
    }

    /// <summary>
    /// Überprüft alle TuPosen und legt ggf. Posen zusammen. Löscht alle 0-Mengen Tuposen und legt diese neu an.
    /// </summary>
    /// <param name="tuPosIds">Alle Tu-Pos-Ids auf der geaenderten Tu</param>
    /// <param name="quantenStrings">Die neuen Quanten als kommaseparierter String</param>
    public void MaterialUmgelagert(IList<int> tuPosIds, IList<string> quantenStrings)
    {

      if (tuPosIds == null || tuPosIds.Count == 0)
      {
        throw new ArgumentNullException("tuPosIds");
      }

      // ToDo: Avisen-Tabelle aktualisieren 

      // Die TuPos-Daten laden
      SgmTuPos ersteTuPos = SgmMaterialManagement.GetTuPos(tuPosIds[0]);
      IList<SgmTuPos> alleTuPosen = SgmMaterialManagement.GetAllTuPosForNamedTu(ersteTuPos.TruName);

      // Die 0-Mengen TuPosen interessieren bei der Neuverbuchung nicht. Sie werden gelöscht und neu angelegt.
      // Die TuPosen werden nach dem Einlagerdatum sortiert, damit immer die Pos mit dem ältesten Material die Basis ist.
      IList<SgmTuPos> tuPosen = alleTuPosen.Where(x => x.Amount != 0).OrderBy(x => x.DatiEinlagerung).ToList();

      if (tuPosen.Count != tuPosIds.Count)
      {
        throw new ArgumentException("Anzahl der TuPosIds stimmt nicht", "tuPosIds");
      }

      _log.DebugFormat("Material auf Tablar [{0}] wird umgelagert", ersteTuPos.TruName);


      IDictionary<SgmTuPos, IList<int>> anzulegendeTuPosen = new Dictionary<SgmTuPos, IList<int>>();
      IDictionary<SgmTuPos, SgmTuPos> tuPosIdGehtAufInTuPosId = new Dictionary<SgmTuPos, SgmTuPos>();

      foreach (SgmTuPos tuPos in tuPosen)
      {
        SgmTuPos localTuPos = tuPos;

        int posIndex = tuPosIds.IndexOf((int)localTuPos.Id);
        IList<int> quantenList = QuantenStringToList(quantenStrings[posIndex]);

        // Überprüfen ob das gleiche Material mit einer anderen Bestandsart auf dem Quadranten  gebucht ist
        int anzBestehendeMitAndereBestandsart =
          anzulegendeTuPosen.Count(x => x.Key.MatId == localTuPos.MatId && x.Key.Bestandsart != localTuPos.Bestandsart &&
                                        (x.Value.Contains(quantenList[0]) || quantenList.Contains(x.Value[0])));

        if (anzBestehendeMitAndereBestandsart != 0)
        {
          throw new SgmException("Gleiches Material mit unterschiedlicher Bestandsart darf nicht auf denselben Quadrant gebucht werden.");
        }

        // Überprüfen ob es auf diesem Quant eine TuPos gibt, die bereits das gleiche Material 
        // und einen älteren Zeitstempel hat
        int anzBestehende =
          anzulegendeTuPosen.Count(x => x.Key.MatId == localTuPos.MatId &&
                                        (x.Value.Contains(quantenList[0]) || quantenList.Contains(x.Value[0])));


        if (anzBestehende == 0)
        {
          anzulegendeTuPosen.Add(localTuPos, quantenList);
          _log.DebugFormat("Umlagern: TuPos (Tu:[{0}]) Pos:[{1}] Mat:[{2}] Menge:[{3}]) bleibt bestehen.",
                           localTuPos.TruName, localTuPos.Id, localTuPos.MatId, localTuPos.Amount);

        }
        else
        {
          KeyValuePair<SgmTuPos, IList<int>> aeltereTuPos = anzulegendeTuPosen.Where(x => x.Key.MatId == localTuPos.MatId &&
                                                                                          x.Key.Bestandsart == localTuPos.Bestandsart && (x.Value.Contains(quantenList[0]) || quantenList.Contains(x.Value[0]))).Select(x => x).First();

          anzulegendeTuPosen.Remove(aeltereTuPos);
          aeltereTuPos.Key.Amount += localTuPos.Amount;
          foreach (int quant in quantenList)
          {
            if (aeltereTuPos.Value.Contains(quant) == false)
            {
              aeltereTuPos.Value.Add(quant);
            }
          }

          anzulegendeTuPosen.Add(aeltereTuPos);
          tuPosIdGehtAufInTuPosId.Add(localTuPos, aeltereTuPos.Key);

          _log.DebugFormat("Umlagern: TuPos (Tu:[{0}] Pos:[{1}] Mat:[{2}] Menge:[{3}]) wurde zu TuPos (Tu:[{4}] Pos:[{5}] Mat:[{6}] Menge:[{7}]) hinzugefügt",
                           localTuPos.TruName, localTuPos.Id, localTuPos.MatId, localTuPos.Amount,
                           aeltereTuPos.Key.TruName, aeltereTuPos.Key.Id, aeltereTuPos.Key.MatId, (aeltereTuPos.Key.Amount - localTuPos.Amount));
        }
      } // foreach neueTuPosen

      DataAccess.ExecuteTransacted(() => UpdateUmgelagertenTuInhalt(anzulegendeTuPosen, tuPosIdGehtAufInTuPosId));

      return;
    }

    /// <summary>
    /// Transportiert ein Tablar vom Arbeitsplatz ins Lager.
    /// </summary>
    public void TransportiereTablarAb(string arbeitsplatzLocName)
    {
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      //liefert alle Tus am dem übergebenen Arbeitsplatz
      IList<SgmTu> tus = GetTusAufArbeitsplatz(arbeitsplatzLocName);

      SgmTu tu = tus.First();

      // Überprüfen ob das Tablar abtransportiert werden darf
      // Es darf nicht abtransportiert werden, wenn im Kommissioniermodus noch entnahemn aktiv sind
      Arbeitsplatz arbeitsplatz = AklArbeitsplatzVerwaltung.GetArbeitsplatzByLocation(arbeitsplatzLocName);
      if (arbeitsplatz.Modus == Arbeitsplatz.ModusValue.Kommissionierung)
      {
        IList<Entnahme> entnahmen = TeilauftragVerwaltung.ErmittleAlleEntnahmenAufArbeitsplatzFuerTu(tu.Name);
        if (entnahmen.Count > 0)
        {
          throw new SgmException("Das Tablar kann nicht abgefördert werden, es gibt es  noch offene Entnahmen.");
        }
      }

      /*
      * Laut CR vom 17.04.2012 sollen Tablare auch abtransportiert werden können, wenn noch Positionen zu inventarisieren sind.
      * Dies hat zur Folge, dass das Tablar von der InventurAuslagerung wieder ausgelagert wird.
      */

      //if (arbeitsplatz.Modus == Arbeitsplatz.ModusValue.Inventur)
      //{
      //  bool fertigInventarisiert = InventurVerwaltung.IstTuFertigInventarisiert(tu.Name);
      //  if (!fertigInventarisiert)
      //  {
      //    throw new SgmException("Das Tablar kann nicht abgefördert werden, es sind noch nicht alle Materialien gezählt.");
      //  }
      //  InventurAuslagerung.Aufwecken();
      //}

      //Konturenfehler von Tu entfernen
      SgmPhysik.DeleteKonturenfehler(tu);

      //Legt einen Tord zum entsprechenden Uebergabeplatz zum Verschiebewagen an.
      AklTransportAktivierung.TransportiereWegVomArbeitsplatz(tu);
    }

    /// <summary>
    /// Legt einen neuen Behälter zur Kommissionierung an
    /// </summary>
    /// <param name="behaelterName">der Name des Behälters</param>
    /// <param name="arbeitsplatzLocName">der Name des Arbeitsplatzes</param>
    public void KommissionierBehaelterAnlegen(string behaelterName, string arbeitsplatzLocName)
    {
      if (string.IsNullOrEmpty(behaelterName))
      {
        throw new ArgumentNullException("behaelterName");
      }
      if (string.IsNullOrEmpty(arbeitsplatzLocName))
      {
        throw new ArgumentNullException("arbeitsplatzLocName");
      }

      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      SgmLocation komPlatz = GetKomPlatzFuerArbeitsplatz(arbeitsplatzLocName);

      SgmTu tuAufKomPlatz = SgmPhysik.GetTuOnLocation(komPlatz.Name);
      if (tuAufKomPlatz != null && tuAufKomPlatz.Name == behaelterName)
      {
        string text = string.Format("Tu [{0}] befindet sich bereits auf [{1}] und kann nicht erneut angelegt werden",
                                    behaelterName,
                                    arbeitsplatzLocName);
        _log.Warn(text);
        throw new SgmException(text, text);
      }

      //überprüfen ob sich bereits eine andere Tu am KomPlatz befindet. Wenn ja auf KOMPLATZ_AUSLAUF buchen      
      TuQuery tuQuery = SgmPhysik.GetTusOn(komPlatz.Name);
      tuQuery.Where(SgmTu.Properties.Name.Filter(BinaryExpressionOperator.NotEqual, behaelterName));

      IList<SgmTu> tus = DataAccess.ExecuteQuery(tuQuery, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      // Alle Tus, die sich auf dem Kommissionierplatz befinden müssen bearbeitet werden.
      // Im Normalfall darf es nur eine Tu sein.
      if (tus.Count > 1)
      {
        _log.ErrorFormat("Auf Location [{0}] darf sich immer nur eine Tu befinden. Es sind jedoch [{1}] Stück",
                         komPlatz.Name, tus.Count);
      }

      foreach (SgmTu tu in tus)
      {
        Query q = SgmMaterialManagement.GetAllTuPos("allTuPos151");
        q.Where(TuPos.Properties.TruName.Filter(tu.Name));
        int count = DataAccess.ExecuteCountQuery(q, "cntTuPos151");

        if (count == 0)
        {
          SgmPhysik.DeleteTu(tu);
        }
        else
        {
          if (tu.TutName.Equals(ConstDirectory.TUT_NAME_GITTERBOX) ||
              tu.TutName.Equals(ConstDirectory.TUT_NAME_GITTERBOX_HALBHOCH))
          {
            GitterboxFuerTeilauftragBereitstellen(tu);
          }
          else
          {
            KommissionierBoxAufKomAuslaufBuchen(tu);
          }
        }
      }

      // Versuchen neue Tu anzulegen. Wenn es bereits eine Tu mit dem gleichen Namen gibt wird eine TuExists Exception geworfen.
      SgmTu newTu = SgmPhysik.CreateTu(behaelterName, komPlatz.Name);

      if (newTu.TutName != ConstDirectory.TUT_NAME_KISTE
        && newTu.TutName != ConstDirectory.TUT_NAME_GITTERBOX
        && newTu.TutName != ConstDirectory.TUT_NAME_GITTERBOX_HALBHOCH)
      {
        SgmPhysik.DeleteTu(newTu);
        string text = string.Format("Tu [{0}] ist keine Kommissionierbox und keine Gitterbox und damit nicht zulässig",
                                    behaelterName);
        _log.Error(text);
        throw new SgmException(text, text);
      }

      _log.InfoFormat("Tu [{0}] auf Location [{1}] angelegt", behaelterName, komPlatz.Name);
    }

    /// <summary>
    /// Ermittelt alle Entnahmen im Status AufArbeitsplatz die für die übergebenen Tu existieren
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns></returns>
    public IList<Entnahme> GetEntnahmenAufArbeitsplatzFuerTu(string tuName)
    {
      if (string.IsNullOrEmpty(tuName))
      {
        throw new ArgumentNullException("tuName");
      }

      //liefert alle Entnahmen mit Status bereit für die Tu am Arbeitsplatz
      IList<Entnahme> entnahmen = TeilauftragVerwaltung.ErmittleAlleEntnahmenAufArbeitsplatzFuerTu(tuName);

      return entnahmen;
    }

    /// <summary>
    /// Liefert die Entnahme zu der übergebenen Id
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <returns>die Entnahme</returns>
    public Entnahme GetEntnahmeAusId(decimal entnahmeId)
    {
      _log.DebugFormat("Suche Entnahme zu ID [{0}]", entnahmeId);

      Query query = TeilauftragVerwaltung.ErmittleAlleEntnahmen("entnahmen");
      query.Where(Entnahme.Properties.Id.Filter(entnahmeId));

      IList<Entnahme> entnahmen = DataAccess.ExecuteQuery(query, row => (Entnahme)row[Entnahme.Properties.AliasName]);

      if (entnahmen.Count == 0)
      {
        return null;
      }
      return entnahmen.First();
    }

    /// <summary>
    /// Liefert die SgmTu anhand des Namens
    /// </summary>
    /// <param name="tuName">der Name der Tu</param>
    /// <returns>die SgmTu</returns>
    public SgmTu GetTuAusName(string tuName)
    {
      if (string.IsNullOrEmpty(tuName))
      {
        throw new ArgumentNullException(tuName);
      }

      return (SgmTu)SgmPhysik.GetTu(tuName);
    }

    /// <summary>
    /// Liefert eine Liste mit allen Quadranten zurück, die von TuPositionen belegt sind, die die übergebene TuPos
    /// als BasisTuPos haben
    /// </summary>
    /// <param name="basisTuPos">die TuPos mit der Menge > 0</param>
    /// <returns>Die List der TuPosen</returns>
    public IList<int> GetAlleQuadrantenVerlinktMit(SgmTuPos basisTuPos)
    {
      if (basisTuPos == null)
      {
        throw new ArgumentNullException("basisTuPos");
      }

      IList<SgmTuPos> tuPosen = SgmMaterialManagement.GetAlleTuPosVerlinktMit(basisTuPos.Id);

      IEnumerable<int> quadranten = from q in tuPosen
                                    select (int)q.Quant;

      IList<int> retValue = quadranten.ToList();
      retValue.Add((int)basisTuPos.Quant);

      return retValue;
    }

    /// <summary>
    /// Liefert alle Tus, die sich auf dem übergebenen Arbeitsplatz befinden
    /// </summary>
    /// <param name="arbeitsplatzLocName">der Arbeitsplatz</param>
    /// <returns></returns>
    public IList<SgmTu> GetTusAufArbeitsplatz(string arbeitsplatzLocName)
    {
      if (string.IsNullOrEmpty(arbeitsplatzLocName)) throw new ArgumentNullException("arbeitsplatzLocName");
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      TuQuery tuQuery = SgmPhysik.GetTusOn(arbeitsplatzLocName);
      tuQuery.OrderBy(OrderByDirection.Desc(Tu.Properties.OrderMove));

      IList<SgmTu> tus = DataAccess.ExecuteQuery(tuQuery, row => (SgmTu)row[SgmTu.Properties.Name.AliasName]);

      if (tus == null || tus.Count == 0)
      {
        string text = string.Format("Keine Tu auf Location [{0}] gefunden", arbeitsplatzLocName);
        throw new TuNotFoundException(text);
      }
      return tus;
    }

    /// <summary>
    /// Liefert die EntnahmeDaten für die nächste Entnahme. Wird keine Tu am Arbeitsplatz gefunden 
    /// so wird null zurückgegeben. Sind für die Tu am Arbeitsplatz keine 
    /// bereiten Entnahmen mehr vorhanden, wird das Property EntnahmeDaten.AnzahlKommissionierungen auf 0 gesetzt.
    /// </summary>
    /// <returns>die EntnahmeDaten</returns>
    public EntnahmeDaten GetEntnahmeDaten(string arbeitsplatzLocName)
    {
      if (string.IsNullOrEmpty(arbeitsplatzLocName)) throw new ArgumentNullException("arbeitsplatzLocName");
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      EntnahmeDaten entnahmeDaten = new EntnahmeDaten();

      entnahmeDaten.KommissionierBehaelterName = GetKommissionierBehaelterAufKomplatz(arbeitsplatzLocName);

      SgmTu tablar = GetTablarDaten(arbeitsplatzLocName);

      if (tablar == null)
      {
        _log.DebugFormat("Kein Tablar auf Arbeitsplatz [{0}] vorhanden", arbeitsplatzLocName);
        return entnahmeDaten;
      }

      entnahmeDaten.TablarName = tablar.Name;
      if (tablar.Fehlerkennung != null)
      {
        entnahmeDaten.KonturenFehler = GetKonturenfehler((int)tablar.Fehlerkennung);
      }

      //alle Entnahmen im Status AufArbeitsplatz die für die übergebenen Tu existieren
      IList<Entnahme> entnahmenFuerTu = GetEntnahmenAufArbeitsplatzFuerTu(tablar.Name);

      if (entnahmenFuerTu.Count == 0)
      {
        _log.InfoFormat("Keine Entnahmen im Status [AufArbeitsplatz] für Tu [{0}] auf Arbeitsplatz [{1}] gefunden.", tablar.Name, arbeitsplatzLocName);
        entnahmeDaten.AnzahlKommissionierungen = 0;
        return entnahmeDaten;
      }
      Entnahme auszufuehrendeEntnahme = entnahmenFuerTu.First();

      entnahmeDaten.AnzahlKommissionierungen = entnahmenFuerTu.Count;
      entnahmeDaten.EntnahmeId = auszufuehrendeEntnahme.Id;
      entnahmeDaten.GreifMenge = auszufuehrendeEntnahme.Menge;

      //liefert die TuPos zur übergebenen Entnahme
      SgmTuPos tuPos = (SgmTuPos)auszufuehrendeEntnahme.GetTuPosZuEntnahme(DataAccess);

      // Die Gesamtmenge der TuPosition
      entnahmeDaten.Menge = tuPos.Amount;

      //Materialdaten ermitteln
      SgmMaterial material = GetMaterial(tuPos.MatId);
      entnahmeDaten.MaterialBezeichnung = material.Description;
      entnahmeDaten.MaterialNummer = material.SapMaterialNo;
      entnahmeDaten.GebindeTyp = material.GebindeTyp;
      entnahmeDaten.GebindeMenge = (material.GebindeMenge != null) ? material.GebindeMenge.ToString() : string.Empty;
      entnahmeDaten.GreifQuadranten = GetAlleQuadrantenVerlinktMit(tuPos);

      //Die Teilauftrag zur auszuführenden Entnahme ermitteln
      TeilauftragPos pos = auszufuehrendeEntnahme.GetTeilaufPosZuEntn(DataAccess);
      Teilauftrag teilauftrag = pos.GetTeilaufZuPos(DataAccess);

      Auftrag auftrag = teilauftrag.GetAuftZuTeilauft(DataAccess);
      entnahmeDaten.SapTaNum = auftrag.SapTanum;
      entnahmeDaten.SapBwlvs = auftrag.SapBwlvs;
      entnahmeDaten.SapFkAuftrag = auftrag.SapFkAuftrag;

      //Wenn bereits ein KomBehaelter zu diesem Teilauftrag vorhanden ist, dann weiter in diesen kommissionieren.
      //Wenn nicht, wird null an den Dialog gegeben und ein neuer KomBehaelter muss gescannt werden.
      entnahmeDaten.KommissionierBehaelterName = GetKommissionierBehaelterAufKomplatzFuer(teilauftrag, arbeitsplatzLocName);

      bool istHintergrundInvAktiv = SystemParameters.GetBool("HINTERGRUND_INVENTUR", false);

      Arbeitsplatz.ModusValue betriebsart = GetArbeitsplatzBetriebsart(arbeitsplatzLocName);

      IList<decimal> tuPositionenMitNaheNullDurchgang = GetTuPositionenMitNaheNullDurchgang(arbeitsplatzLocName);

      bool istZeitstempelGueltig = IstZeitstempelGueltig(tuPos.Id);

      // Im Inventurmodus muss gezählt werden falls kein gültiger Zeitstempel vorliegt
      if (betriebsart.Equals(Arbeitsplatz.ModusValue.Inventur) && !istZeitstempelGueltig)
      {
        entnahmeDaten.MengeAnzeigen = false;
      }

        // Bei Nahe-0-Durchgang muss gezählt werden
      else if (tuPositionenMitNaheNullDurchgang.Contains(tuPos.Id))
      {
        entnahmeDaten.MengeAnzeigen = false;
      }

        // Bei Hintergrundinventur und ungültigem Zeitstempel kann gezählt werden
      else if (istHintergrundInvAktiv && !istZeitstempelGueltig)
      {
        entnahmeDaten.MengeAnzeigen = false;
      }

        //Keine Zählung erforderlich
      else
      {
        entnahmeDaten.MengeAnzeigen = true;
      }
      _log.InfoFormat("Entnahme Id [{0}] von Tu [{1}] in Kommissionierbox [{2}] an Dialog gesendet.",
                      auszufuehrendeEntnahme.Id, tablar.Name,
                      (entnahmeDaten.KommissionierBehaelterName ?? "NEU"));

      return entnahmeDaten;
    }

    /// <summary>
    /// Liefert eine Liste mit Konturenfehlern, die sich hinter der übergebenen Integer Zahl verbergen.
    /// </summary>
    /// <param name="alleKonturenfehler"></param>
    /// <returns></returns>
    public IList<string> GetKonturenfehler(int? alleKonturenfehler)
    {
      IList<string> vorhandeneFehler = new List<string>();

      if (alleKonturenfehler == null)
      {
        return vorhandeneFehler;
      }

      int konturenFehler = (int)alleKonturenfehler;
      if ((konturenFehler & ConstDirectory.AKL_KONTURENFEHLER_UEBERSTAND_VORNE) > 0)
      {
        vorhandeneFehler.Add("Überstand an Vorderkante");
      }

      if ((konturenFehler & ConstDirectory.AKL_KONTURENFEHLER_UEBERSTAND_HINTEN) > 0)
      {
        vorhandeneFehler.Add("Überstand an Hinterkante");
      }

      if ((konturenFehler & ConstDirectory.AKL_KONTURENFEHLER_HOEHE) > 0)
      {
        vorhandeneFehler.Add("Tablar zu hoch");
      }

      if ((konturenFehler & ConstDirectory.AKL_KONTURENFEHLER_BREITE) > 0)
      {
        vorhandeneFehler.Add("Tablar zu breit");
      }

      if ((konturenFehler & ConstDirectory.AKL_KONTURENFEHLER_GEWICHT) > 0)
      {
        vorhandeneFehler.Add("Übergewicht");
      }

      return vorhandeneFehler;
    }

    /// <summary>
    /// Setzt den Konturenfehler der Tu am Arbeitsplatz zurück
    /// </summary>
    public void SetzeKonturenfehlerZurueck(string arbeitsplatz)
    {
      SgmTu tu = SgmPhysik.GetTuOnLocation(arbeitsplatz);

      if (tu == null)
      {
        _log.WarnFormat("Keine Tu am Arbeitsplatz [{0}] gefunden", arbeitsplatz);
        return;
      }

      SgmPhysik.DeleteKonturenfehler(tu);

      _log.InfoFormat("Konturenfehler von Tu [{0}] zurückgesetzt", tu.Name);
    }

    /// <summary>
    /// Führt die Umbuchung der kommissionierten Menge auf den Kommissionierbehälter aus.
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <param name="entnahmeMenge">die tatsächlich entnommene Menge</param>
    /// <param name="komTuName">Name des Kommissionierbehälters</param>
    /// <returns>Ist der aktuelle Teilauftrag mit dieser Entnahme abgeschlossen ja/nein</returns>
    public bool EntnahmeBestaetigen(decimal entnahmeId, decimal entnahmeMenge, string komTuName, string terminalName, string userName)
    {
      if (string.IsNullOrEmpty(komTuName))
      {
        throw new ArgumentNullException("komTuName");
      }
      Entnahme entnahme = GetEntnahmeAusId(entnahmeId);

      if (entnahme == null)
      {
        _log.ErrorFormat("Keine Entnahme zu Id = [{0}] gefunden", entnahmeId.ToString());
        return false;
      }

      if (entnahme.TupId == null)
      {
        throw new NullReferenceException("entnahme.TuPosId");
      }

      SgmTu kommissionierTu = GetTuAusName(komTuName);

      if (kommissionierTu == null)
      {
        throw new TuNotFoundException(komTuName);
      }

      Auftrag.AuftragsArtValue auftragsArt = TeilauftragVerwaltung.GetAuftragsArt(entnahme.AufId);

      decimal teilauftragId = entnahme.GetTeilaufPosZuEntn(DataAccess).TaId;

      _log.DebugFormat("Verbuche Entnahme [{0}] in Tu [{1}] AuftragsArt [{2}]", entnahme.Id, kommissionierTu.Name, auftragsArt);

      switch (auftragsArt)
      {
        case Auftrag.AuftragsArtValue.BksNachschub:
          {
            if (kommissionierTu.Name != ConstDirectory.NBV_PREFIX + ConstDirectory.LAGERTEIL_AKL + entnahme.AufId.ToString().Substring(3, 6))
            {
              string text = string.Format("Die Tu [{0}] ist nicht zulässig für einen Nbv", kommissionierTu.Name);

              throw new SgmException(text, text);
            }
            // Es handelt sich um einen NbvAuftrag. Dieser geht nicht über die Bühne sondern wird
            // sofort auf die NBV_TU gebucht, die sich auf der Location Transferbestand_Nbv befindet.
            // Der Auftrag bleibt bis zur Verbuchung im Bks auf auf FertigKommissioniert stehen
            VerbucheNbvAuftragEntnahme(entnahme, kommissionierTu, entnahmeMenge, terminalName, teilauftragId, userName);
            break;
          }
        case Auftrag.AuftragsArtValue.Umlagerung:
          {
            string umlTuName = ConstDirectory.GetUmlTuName(Base.Lagerteil.Akl, entnahme.AufId);
            if (kommissionierTu.Name != umlTuName)
            {
              string text = string.Format("Die Tu [{0}] ist nicht zulässig für einen Umlagerauftrag", kommissionierTu.Name);

              throw new SgmException(text, text);
            }
            // Es handelt sich um einen UmlAuftrag. Dieser geht nicht über die Bühne sondern wird
            // sofort auf die UML_TU gebucht, die sich auf der Location Transferbestand_UML befindet.
            VerbucheUmlAuftragEntnahme(entnahme, kommissionierTu, entnahmeMenge, terminalName, teilauftragId, userName);
            break;
          }
        default:
          {
            // Es handelt sich um einen normalen Auslagerauftrag von SAP          
            VerbucheSapAuftragEntnahme(entnahme, kommissionierTu, entnahmeMenge, terminalName, teilauftragId, userName);
            break;
          }
      }

      bool auftragFertigKommissioniert = TeilauftragVerwaltung.SindAlleEntnahmenInStatus(teilauftragId,
                                                                                         Entnahme.StatusValue.
                                                                                           FertigKommissioniert);
      return auftragFertigKommissioniert;
    }

    private void VerbucheNbvAuftragEntnahme(Entnahme entnahme, SgmTu nbvTu, decimal entnahmeMenge, string terminalName, decimal teilauftragId, string userName)
    {
      if (nbvTu.LocName != ConstDirectory.LOC_NAME_TRANSFER_BESTAND_NBV)
      {
        string text = string.Format(
          "Entnahme [{0}] für NBV-Auftrag [{1}] kann nicht in TU [{2}] kommissioniert werden.", entnahme.Id,
          entnahme.AufId, nbvTu);

        throw new SgmException(text, text);
      }

      // Bucht die entnommene Menge von dem Tablar auf die Transferlocation
      TeilauftragVerwaltung.EntnahmeVerbuchen(entnahme, nbvTu, entnahmeMenge, userName, terminalName);

      // Den Teilauftrag mit dem Kommissionierbehäter verheiraten, falls dies noch nicht der Fall ist.
      Lagerteil.TuMitTeilauftragVerheiraten(teilauftragId, nbvTu);

      bool teilauftragFertig = TeilauftragVerwaltung.SindAlleEntnahmenInStatus(teilauftragId,
                                                                               Entnahme.StatusValue.FertigKommissioniert);
      if (teilauftragFertig)
      {
        BelegDruck.DruckeBelegEntnahme(BelegSzenario.DruckZeitpunktValue.Pick, entnahme, terminalName,
                                       DruckBereich.Akl, userName);

        Lagerteil.TuBereitgestellt(nbvTu);

        Lagerteil.TuAbgerufen(nbvTu);

        Lagerteil.TuZusammengefuehrt(nbvTu);
      }
    }

    private void VerbucheUmlAuftragEntnahme(Entnahme entnahme, SgmTu umlTu, decimal entnahmeMenge, string terminalName, decimal teilauftragId, string userName)
    {
      if (umlTu.LocName != ConstDirectory.LOC_NAME_TRANSFER_BESTAND_UML)
      {
        string text = string.Format(
          "Entnahme [{0}] für UML-Auftrag [{1}] kann nicht in TU [{2}] kommissioniert werden.", entnahme.Id,
          entnahme.AufId, umlTu);

        throw new SgmException(text, text);
      }

      // Bucht die entnommene Menge von dem Tablar auf die Transferlocation
      TeilauftragVerwaltung.EntnahmeVerbuchen(entnahme, umlTu, entnahmeMenge, userName, terminalName);

      // Den Teilauftrag mit dem Kommissionierbehäter verheiraten, falls dies noch nicht der Fall ist.
      Lagerteil.TuMitTeilauftragVerheiraten(teilauftragId, umlTu);

      Teilauftrag teilauftrag = TeilauftragVerwaltung.ErmittleTeilauftragById(teilauftragId);
      Auftrag auftrag = teilauftrag.GetAuftZuTeilauft(DataAccess);

      if (entnahme.Menge != 0)
      {
        BelegDruck.DruckeBelegEntnahme(BelegSzenario.DruckZeitpunktValue.Pick, entnahme, terminalName, DruckBereich.Akl, userName);

        // Legt die Avise zum Einlagern des Materials an
        AvisenVerwaltung.UmlagerAviseAnlegen(auftrag.SapTanum, umlTu);
      }

      Lagerteil.TuBereitgestellt(umlTu);

      Lagerteil.TuAbgerufen(umlTu);

      Lagerteil.TuZusammengefuehrt(umlTu);
    }


    private void VerbucheSapAuftragEntnahme(Entnahme entnahme, SgmTu kommissionierTu, decimal entnahmeMenge, string terminalName, decimal teilauftragId, string userName)
    {
      // Wenn die KommissionierTu nicht mehr auf den Kommissionierplatz gebucht ist
      if (kommissionierTu.LocName.Contains("KOMPLATZ") == false)
      {
        throw new SgmException("KommissionierBox nicht mehr am Arbeitsplatzplatz");
      }

      // Bucht die entnommene Menge von dem Tablar auf die KommissionierTu
      TeilauftragVerwaltung.EntnahmeVerbuchen(entnahme, kommissionierTu, entnahmeMenge, userName, terminalName);

      BelegDruck.DruckeBelegEntnahme(BelegSzenario.DruckZeitpunktValue.Pick, entnahme, terminalName, DruckBereich.Akl, userName);

      // Den Teilauftrag mit dem Kommissionierbehäter verheiraten, falls dies noch nicht der Fall ist.
      Lagerteil.TuMitTeilauftragVerheiraten(teilauftragId, kommissionierTu);

      if (kommissionierTu.TutName == ConstDirectory.TUT_NAME_GITTERBOX || kommissionierTu.TutName == ConstDirectory.TUT_NAME_GITTERBOX_HALBHOCH)
      {
        _log.InfoFormat("Entnahme [{0}] zu Teilauftrag [{1}] wurde in ein Gitterbox kommissioniert", entnahme.Id,
                        teilauftragId);

        // Wenn alle Entnahmen zu diesem Teilauftrag kommissioniert sind, kann die Gitterbox bereitgestellt werden
        if (TeilauftragVerwaltung.SindAlleEntnahmenInStatus(teilauftragId, Entnahme.StatusValue.FertigKommissioniert))
        {
          _log.InfoFormat(
            "Alle Entnahmen zu Teilauftrag [{0}] wurden kommissioniert. Gitterbox [{1}] wird bereitgestellt",
            teilauftragId, kommissionierTu.Name);

          GitterboxFuerTeilauftragBereitstellen(kommissionierTu);
        }
      }
    }

    private void GitterboxFuerTeilauftragBereitstellen(SgmTu gitterbox)
    {
      // Die Gitterbox ist nicht lokalisierbar darum wird sie auf die Location "FREIE_LAGERBEWEGUNG" gebucht
      SgmLocation freieLagerbewegung = (SgmLocation)SgmPhysik.GetLocationByName(ConstDirectory.LOC_NAME_FREIE_LAGERBEWEGUNG);

      gitterbox.LocName = freieLagerbewegung.Name;
      SgmPhysik.UpdateTu(gitterbox);

      // Akl Lagerteil informieren, dass Gitterbox bereitgestellt wurde
      Lagerteil.TuBereitgestellt(gitterbox);

      _log.InfoFormat("Gitterbox [{0}] wurde an Akllagerteil als bereitgestellt gemeldet", gitterbox.Name);
    }

    /// <summary>
    /// Die Materialmenge wird korrigiert (Grund: entnahme aufgrund von Gewichtsfehler oder/und Konturenfehler)
    /// </summary>
    /// <param name="tuPosId">die TuPos, deren Menge korregiert werden soll</param>
    /// <param name="entnommeneMenge">Die entnommene Menge des Materials</param>
    /// <param name="avisePosId">die zugehörige AvisePosition</param>
    public void MaterialMengenKorrektur(decimal tuPosId, decimal entnommeneMenge, decimal avisePosId)
    {
      AvisePos avisePos = AvisenVerwaltung.GetAvisePosition(avisePosId);
      Avise avise = avisePos.GetAviseZuPosition(DataAccess);
      DataAccess.ExecuteTransacted(delegate
                                     {
                                       AvisenVerwaltung.ZulagerungMengeKorrigieren(tuPosId, avisePosId, entnommeneMenge);
                                       SgmMaterialManagement.TuPosMengeKorrigieren(tuPosId, entnommeneMenge, avise.SapTanum);
                                     });
    }

    /// <summary>
    /// Liefert einen RemoteEntityContainer mit allen TuPosen und der Menge die zu nicht abgeschlossenen AvisePositionen
    /// gehören. Die TuPosen befinden sich auf der Tu die am aktuellen Arbeitsplatz steht
    /// </summary>
    /// <returns></returns>
    public Query GetEntnehmbaresMaterial(string arbeitsplatzLocation)
    {
      Query query = new Query("entnehmbar");
      query.From<SgmTu>();
      query.InnerJoin<SgmTuPos>(Expression.IsEqual(SgmTuPos.Properties.TruName, SgmTu.Properties.Name));
      query.InnerJoin<Zulagerung>(Expression.IsEqual(SgmTuPos.Properties.Id, Zulagerung.Properties.TupId));
      //query.InnerJoin<AvisePos>(Expression.IsEqual(AvisePos.Properties.Id, Zulagerung.Properties.AvipId));
      query.InnerJoin<SgmMaterial>(Expression.IsEqual(SgmMaterial.Properties.Id, SgmTuPos.Properties.MatId));
      query.Where(SgmTu.Properties.LocName.Filter(arbeitsplatzLocation));
      //query.Where(AvisePos.Properties.Status.Filter(BinaryExpressionOperator.NotEqual,
      //                                                AvisePos.StatusValue.Fertig));
      return query;
    }

    public Query GetTeilauftragPositionen(string sapTaNum)
    {
      Query query = TeilauftragVerwaltung.GetAllAuftragMitTeilauftragUndTeilauftragPositionen("TeilauftraegeZuSapAuftrag", Teilauftrag.LagerTeilValue.Akl);
      query.InnerJoin<SgmMaterial>(Expression.IsEqual(TeilauftragPos.Properties.MatId, SgmMaterial.Properties.Id));
      query.Where(Auftrag.Properties.SapTanum.Filter(sapTaNum));
      return query;
    }


    /// <summary>
    /// Tus the pos amount korrektur.
    /// </summary>
    /// <param name="tuPosIds">The decimals.</param>
    /// <param name="amounts">The amounts.</param>
    /// <param name="user">The user.</param>
    /// <param name="terminalName">der Name des Terminals</param>
    public bool InventurVerbuchen(IList<int> tuPosIds, IList<decimal> amounts, string user, string terminalName, string arbeitsplatzLocation)
    {
      if (tuPosIds == null) { throw new ArgumentNullException("tuPosIds"); }
      if (amounts == null) { throw new ArgumentNullException("amounts"); }
      if (tuPosIds.Count != amounts.Count) { }

      StringBuilder sb = new StringBuilder();

      bool retVal = true;
      for (int i = 0; i < tuPosIds.Count; i++)
      {
        int tuPosId = tuPosIds[i];
        decimal amount = amounts[i];

        SgmTuPos tuPos = SgmMaterialManagement.GetTuPos(tuPosId);

        if (tuPos == null)
        {
          sb.AppendFormat("{0}, ", tuPosId);
          continue;
        }

        decimal mengenDifferenz = amount - tuPos.Amount;

        _log.InfoFormat("TuPos [{0}] wird von User [{1}] von Menge [{2}] auf Menge [{3}] geändert. Differenz [{4}]", tuPosId, user, tuPos.Amount, amount, mengenDifferenz);

        // liefert die Art der Inventur z.B. Nahe 0 - Durchgang, Hintergrund-Inventur....
        InventurZaehlung.InventurArtValue inventurArt = GetInventurArt(tuPos, arbeitsplatzLocation);

        // Führt die Inventurbuchungen durch
        bool inventurVerbucht = InventurVerwaltung.InventurDurchgefuehrt(tuPos, amount, user, terminalName, inventurArt);
        retVal = retVal && inventurVerbucht;

        tuPos = SgmMaterialManagement.GetTuPos(tuPosId);

        if (tuPos != null && tuPos.Amount == 0)
        {
          string text = string.Format("TuPos [{0}] wird nach einer Mengen Korrektur leer und daher gelöscht.", tuPos.Id);
          _log.Info(text);
          SgmMaterialManagement.DeleteTuPos(tuPos);

          continue;
        }
      }

      if (sb.Length > 0)
      {
        string text = string.Format("Menge für TuPosIds [{0}] konnte nicht korigiert werden, da sie nicht gefunden wurden.", sb);
        throw new SgmException(text, text);
      }

      return retVal;
    }



    /// <summary>
    /// Erzeugt einen Transportauftrag für die übergebene Tu zum Arbeitsplatz
    /// </summary>
    /// <param name="tuName">der Name des Tablars</param>
    /// <param name="arbeitsPlatzLocName">die Location des Arbeitsplatzes</param>
    public void TablarAnfordern(string tuName, string arbeitsPlatzLocName)
    {
      if (string.IsNullOrEmpty(tuName)) { throw new ArgumentNullException("tuName"); }
      if (string.IsNullOrEmpty(arbeitsPlatzLocName)) { throw new ArgumentNullException("arbeitsPlatzLocName"); }

      if (IsAklArbeitsplatz(arbeitsPlatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsPlatzLocName);
        throw new SgmException(text, text);
      }

      SgmTu tablar = (SgmTu)SgmPhysik.GetTu(tuName);
      if (tablar == null)
      {
        throw new TuNotFoundException(tuName);
      }
      if (tablar.TutName != ConstDirectory.TUT_NAME_TABLAR)
      {
        string text = string.Format("Tu [{0}] ist kein Tablar und kann daher nicht ausgelagert werden.", tablar.Name);
        throw new SgmException(text, text);
      }

      SgmLocation arbeitsPlatzLocation = (SgmLocation)SgmPhysik.GetLocationByName(arbeitsPlatzLocName);

      AklTransportGenerierung.TablarAuslagern(tablar, arbeitsPlatzLocation, TordRequest.TransportTypValue.ManuelleAuslagerung);
    }

    /// <summary>
    /// Es werden alle Tablare mit einem bestimmten Material ausgelagert
    /// </summary>
    /// <param name="materialId"></param>
    /// <param name="arbeitsPlatzLocName"></param>
    public void MaterialAnfordern(decimal materialId, string arbeitsPlatzLocName)
    {
      if (materialId < 0) { throw new ArgumentNullException("materialId"); }
      if (string.IsNullOrEmpty(arbeitsPlatzLocName)) { throw new ArgumentNullException("arbeitsPlatzLocName"); }

      if (IsAklArbeitsplatz(arbeitsPlatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsPlatzLocName);
        throw new SgmException(text, text);
      }

      SgmLocation arbeitsPlatzLocation = (SgmLocation)SgmPhysik.GetLocationByName(arbeitsPlatzLocName);

      Query tusMitMaterialQuery = SgmMaterialManagement.GetAllTuContainingMaterial("tusMitMat", materialId);
      // Nur Tus im Akl
      Location.LocationAlias locationAlias = Location.GetAlias("Location");
      tusMitMaterialQuery.Where(locationAlias.L.Filter(ConstDirectory.WHT_AKL));
      // Es kommt vor, das Kommboxen mit ausgelagert werden. Um dies zu uinterbinden nachfolgender Filter
      tusMitMaterialQuery.Where(SgmTu.Properties.TutName.Filter(ConstDirectory.TUT_NAME_TABLAR));
      tusMitMaterialQuery.GroupBy<SgmTu>();

      IList<SgmTu> tus = DataAccess.ExecuteQuery(tusMitMaterialQuery, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      //Der Benutzer muss benachrichtigt werden, das einige Tablare nicht asugelagert werden konnten
      string errorText = string.Empty;
      int errorCounter = 0;
      foreach (SgmTu tu in tus)
      {
        try
        {
          AklTransportGenerierung.TablarAuslagern(tu, arbeitsPlatzLocation, TordRequest.TransportTypValue.ManuelleAuslagerung);
        }
        catch (SgmException sgmExcp)
        {
          errorCounter++;
          errorText = string.Format("{0} {1};", errorText, sgmExcp.Text);
        }
      }

      if (errorCounter != 0)
      {
        string text = string.Format("[{0}]/[{1}] Tablare konnten nicht ausgelagert werden. Gründe [{2}]", errorCounter, tus.Count, errorText);
        throw new SgmException(text, text);
      }
    }

    /// <summary>
    /// Überprüft, ob eine Inventurzählung für diese TuPos durchgeführt werden muss.
    /// </summary>
    /// <param name="tuPosId"></param>
    /// <returns></returns>
    public bool IstZeitstempelGueltig(decimal tuPosId)
    {
      SgmTuPos tuPos = SgmMaterialManagement.GetTuPos(tuPosId);

      if (tuPos == null)
      {
        throw new TuPosDoesNotExistException("unbekannt", tuPosId);
      }

      return !InventurVerwaltung.MussInventurDurchgefuehrtWerden(tuPos);
    }

    /// <summary>
    /// Prüft ob durch das Picken der Entnahmen ein Nahe-0-Durchgang entsteht.
    /// </summary>
    /// <param name="arbeitsplatzLocation"></param>
    /// <returns></returns>
    public IList<decimal> GetTuPositionenMitNaheNullDurchgang(string arbeitsplatzLocation)
    {
      IList<decimal> tuPosenFuerInventur = new List<decimal>();
      SgmTu tablar = GetTablarDaten(arbeitsplatzLocation);

      if (tablar == null)
      {
        _log.DebugFormat("Kein Tablar auf Arbeitsplatz [{0}] vorhanden", arbeitsplatzLocation);
        return tuPosenFuerInventur;
      }

      tuPosenFuerInventur = SgmMaterialManagement.GetTuPositionenMitNaheNullDurchgang(tablar.Name);

      return tuPosenFuerInventur;
    }

    /// <summary>
    /// Liefert die Betriebsart des Arbeitsplatzes
    /// </summary>
    /// <returns></returns>
    public Arbeitsplatz.ModusValue GetArbeitsplatzBetriebsart(string arbeitsPlatzLocation)
    {
      Arbeitsplatz arbeitsplatz = AklArbeitsplatzVerwaltung.GetArbeitsplatzByLocation(arbeitsPlatzLocation);

      return arbeitsplatz.Modus;
    }

    /// <summary>
    /// Liefert den Arbeitsplatz dem der Teilauftrag entweder aktiv oder in der Queue zugeordnet ist.
    /// Liefert null wenn der Teilauftrag nicht in Arbeitsplatz/ArbeitsplatzQueue gefundenwird.
    /// </summary>
    /// <param name="teilauftragId"></param>
    /// <returns></returns>
    public Arbeitsplatz GetArbeitsplatzForTeilauftrag(decimal teilauftragId)
    {
      Arbeitsplatz arbeitsplatz = DataAccess.SelectFirst<Arbeitsplatz>(Arbeitsplatz.Properties.TaId.Filter(teilauftragId));

      if (arbeitsplatz != null)
      {
        _log.DebugFormat("Teilauftrag [{0}] ist aktiv am Arbeitplatz [{1}].", teilauftragId, arbeitsplatz.Name);
        return arbeitsplatz;
      }

      Query query = new Query("TeilauftragZuArbeitsplatz");
      query.From<Arbeitsplatz>();
      query.InnerJoin<ArbeitsplatzQueue>(Expression.IsEqual(ArbeitsplatzQueue.Properties.ArbId, Arbeitsplatz.Properties.Id));
      query.Where(ArbeitsplatzQueue.Properties.TaId.Filter(teilauftragId));

      IList<Arbeitsplatz> result = DataAccess.ExecuteQuery(query, row => (Arbeitsplatz)row[Arbeitsplatz.Properties.AliasName]);
      if (result.Count == 0)
      {
        _log.ErrorFormat("Teilauftrag [{0}] nicht in Arbeitsplatz/ArbetisplatzQueue gefunden.", teilauftragId);
        return null;
      }

      if (result.Count > 1)
      {
        _log.WarnFormat("Teilauftrag [{0}] ist mehreren ArbeistplatzQueues zugeordnet. ArbeitsplatzQueue [{1}] wird benutzt.", teilauftragId, result[0].Name);
        return result[0];
      }

      _log.DebugFormat("Teilauftrag [{0}] ist ArbeitsplatzQueue [{1}] zugeordnet.", teilauftragId, result[0].Name);
      return result[0];

    }
    #endregion

    #region Implementation von IAklTransportAktivierungStatus

    /// <summary>
    /// Diese Funktion wird aufgerufen, wenn eine Tu eingelagert wurde.
    /// Sie überprüft, ob zu der Tu eine Avise existiert und meldet ggf. an die AviseVerwaltung
    /// </summary>
    /// <param name="tu">die eingelagerte Tu</param>
    public void TuEingelagert(SgmTu tu)
    {
      Query query = AvisenVerwaltung.GetZulagerungenFuerTu(tu);

      IList<Zulagerung> zulagerungen = DataAccess.ExecuteQuery(query, row => (Zulagerung)row[Zulagerung.Properties.AliasName]);

      foreach (Zulagerung zulagerung in zulagerungen)
      {
        AvisenVerwaltung.TuPosFuerAvisePosEingelagert(zulagerung.AvipId, zulagerung.TupId);
      }
    }

    public void TuAusgelagert(SgmTu tu)
    {
      // Hier passiert nichts!
      return;
    }

    public void LeerberhaelterAuslagerungBegonnen(SgmTu tu)
    {
      //Hier passiert nichts
      return;
    }

    public void KommissionierAuslagerungBegonnen(Teilauftrag teilauftrag, Arbeitsplatz zielArbeitsplatz)
    {
      zielArbeitsplatz = DataAccess.SelectFirst<Arbeitsplatz>(Arbeitsplatz.Properties.Id.Filter(zielArbeitsplatz.Id));
      // Wenn am Arbeitsplatz grade kein Teilauftrag aktiv ist, diesen direkt aktivieren
      if (zielArbeitsplatz.TaId == null || zielArbeitsplatz.TaId == teilauftrag.Id)
      {
        AktiviereTeilauftragAmArbeitsplatz(zielArbeitsplatz, teilauftrag);
        return;
      }
      // ansonsten die ArbeitsplatzQueue füllen
      TrageTeilauftragInArbeitsplatzQueueEin(zielArbeitsplatz, teilauftrag);
    }

    private void TrageTeilauftragInArbeitsplatzQueueEin(Arbeitsplatz zielArbeitsplatz, Teilauftrag teilauftrag)
    {
      // Teilauftrag darf nur einmal in der Queue vorkommen
      ArbeitsplatzQueue arbQueue = GetArbeitsplatzQueueByTeilauftragId(teilauftrag.Id);
      if (arbQueue != null)
      {
        return;
      }
      ArbeitsplatzQueue apQ = DataAccess.Create<ArbeitsplatzQueue>();
      apQ.ArbId = zielArbeitsplatz.Id;
      apQ.TaId = teilauftrag.Id;

      _log.DebugFormat("Neuen Eintrag in die ArbeitsplatzQueue. Arbeitplatz [{0}], Teilauftrag [{1}]", apQ.ArbId, apQ.TaId);
      DataAccess.Insert(apQ);
    }

    private ArbeitsplatzQueue GetArbeitsplatzQueueByTeilauftragId(decimal teilauftragId)
    {
      return DataAccess.SelectFirst<ArbeitsplatzQueue>(ArbeitsplatzQueue.Properties.TaId.Filter(teilauftragId));
    }
    /// <summary>
    /// Setz die TeilauftragId im Arbeitsplatz auf den übergebenen Teilauftrag
    /// </summary>
    /// <param name="zielArbeitsplatz"></param>
    /// <param name="teilauftrag"></param>
    private void AktiviereTeilauftragAmArbeitsplatz(Arbeitsplatz zielArbeitsplatz, Teilauftrag teilauftrag)
    {
      //Teilauftrag ggf. aus Queue löschen
      ArbeitsplatzQueue arbQueue = GetArbeitsplatzQueueByTeilauftragId(teilauftrag.Id);
      if (arbQueue != null)
      {
        LoescheTeilauftragAusArbeitplatzQueue(teilauftrag.Id);
      }

      if (zielArbeitsplatz.TaId.HasValue)
      {
        Teilauftrag alterTeilauftrag = TeilauftragVerwaltung.ErmittleTeilauftragById(zielArbeitsplatz.TaId.Value);

        // wenn der alte Teilauftrag nicht mehr relevant ist, kann der Arbeitsplatz freigegeben werden
        if (alterTeilauftrag == null || alterTeilauftrag.Status > Teilauftrag.StatusValue.Kommissioniert)
        {
          ArbeitsplatzFreigeben(zielArbeitsplatz);
        }

        else if (alterTeilauftrag.Status > Teilauftrag.StatusValue.TablareAusgelagert && alterTeilauftrag.Status < Teilauftrag.StatusValue.Kommissioniert)
        {
          _log.ErrorFormat("Es wurde versucht Teilauftrag [{0}] im Status [{1}] von Arbeitsplatz [{2}] zu deaktivieren und Teilauftrag [{3}] zu aktivieren",
                           alterTeilauftrag.Id, alterTeilauftrag.Status, zielArbeitsplatz.Name, teilauftrag.Id);
          return;
        }

        _log.InfoFormat("Der Teilauftrag [{0}] wird von Arbeitsplatz [{1}] zurückgesetzt und neuer aktiviert", zielArbeitsplatz.TaId, zielArbeitsplatz.Name);
      }
      _log.InfoFormat("Teilauftrag [{0}] am Arbeitsplatz [{1}] aktiviert.", teilauftrag.Id, zielArbeitsplatz.Name);

      zielArbeitsplatz.TaId = teilauftrag.Id;

      DataAccess.Update(zielArbeitsplatz);
    }

    private void ArbeitsplatzFreigeben(Arbeitsplatz zielArbeitsplatz)
    {
      _log.InfoFormat("Der Arbeitsplatz [{0}] wird freigegeben da Teilauftrag [{1}] bereits kommissioniert wurde", zielArbeitsplatz.Name, zielArbeitsplatz.TaId);

      zielArbeitsplatz.TaId = null;
      DataAccess.Update(zielArbeitsplatz);
    }

    /// <summary>
    /// Setzt die TeilauftragId des übergebenen Arbeitsplatzes auf null
    /// </summary>
    /// <param name="zielArbeitsplatz"></param>
    private void SetTeilauftragAmArbeitsplatzZurueck(Arbeitsplatz zielArbeitsplatz)
    {
      if (zielArbeitsplatz.TaId.HasValue)
      {
        Teilauftrag alterTeilauftrag = TeilauftragVerwaltung.ErmittleTeilauftragById(zielArbeitsplatz.TaId.Value);

        if (alterTeilauftrag != null && alterTeilauftrag.Status > Teilauftrag.StatusValue.TablareAusgelagert && alterTeilauftrag.Status < Teilauftrag.StatusValue.Kommissioniert)
        {
          _log.ErrorFormat("Es wurde versucht Teilauftrag [{0}] im Status [{1}] von Arbeitsplatz [{2}] zurückzusetzen",
                           alterTeilauftrag.Id, alterTeilauftrag.Status, zielArbeitsplatz.Name);
          return;
        }

        _log.InfoFormat("Der Teilauftrag [{0}] wird von Arbeitsplatz [{1}] zurückgesetzt und auf null gesetzt", zielArbeitsplatz.TaId, zielArbeitsplatz.Name);
      }

      zielArbeitsplatz.TaId = null;
      DataAccess.Update(zielArbeitsplatz);
    }

    /// <summary>
    /// Aktiviert den nächsten Auftrag aus der Queue. Ist die Queue Leer, wird die TaId auf null gesetzt.
    /// </summary>
    /// <param name="vtwAbgabeLocation"></param>
    public void AktiviereTeilauftragAusQueue(string vtwAbgabeLocation)
    {
      string arbeitplatzLocation = GetArbeitsplatzLocationForVtwAbgabeLocation(vtwAbgabeLocation);
      Arbeitsplatz arbeitsplatz = AklArbeitsplatzVerwaltung.GetArbeitsplatzByLocation(arbeitplatzLocation);
      // Zwecks Überprüfung, ob noch Tablare für den Teilauftrag unterwegs sind
      bool isAktiviert = false;
      while (isAktiviert == false)
      {
        Teilauftrag teilauftrag = GetNextTeilauftragIdFromArbeitplatzQueue(arbeitsplatz.Id);

        if (teilauftrag == null)
        {
          _log.DebugFormat("Es wurde kein Teilauftrag gefunden der für Arbeitsplatz [{0}] aktiviert werden kann.", arbeitsplatz.Name);
          SetTeilauftragAmArbeitsplatzZurueck(arbeitsplatz);
          isAktiviert = true;
        }
        else
        {

          if (IsAnyTordAngelegt(teilauftrag) || IsAnyTablarAusgelagert(teilauftrag))
          {
            AktiviereTeilauftragAmArbeitsplatz(arbeitsplatz, teilauftrag);
            isAktiviert = true;
          }
          else
          {
            // ToDo: Was soll hier mit dem Teilauftrag und den Teilauftragpositionen passieren?
            // ToDo: Alles was noch nicht im Status kommissioniert ist, müsste auf TordRequest angelegt zurückgesetzt werden, damit der Teilauftrag nochmal aktiviert werden kann
            _log.ErrorFormat("Es wurde keine aktive Tu zu Teilauftrag [{0}] gefunden. Teilauftrag wird aus der ArbeitsplatzQueue gelöscht", teilauftrag.Id);
            LoescheTeilauftragAusArbeitplatzQueue(teilauftrag.Id);
          }
        }
      }
    }

    #endregion

    #region Hilfsfunktionen

    ///<summary>
    /// Schließt eine AvisePos 
    ///</summary>
    private void AvisePosSchliessen(AvisePos avisePos)
    {
      AvisenVerwaltung.AvisePosSchliessen(avisePos.Id);
    }

    private void TransferTuAnlegen(string tuName, string locName)
    {
      SgmTu tu = (SgmTu)SgmPhysik.GetTu(tuName);

      if (tu == null)
      {
        tu =
          (SgmTu)
          SgmPhysik.CreateTu(locName, ConstDirectory.TUT_NAME_KISTE, tuName, 0, 0);
      }
      else if (tu.LocName != locName)
      {
        string text = string.Format("Die Tu [{0}] befindet sich nicht auf der Location [{1}]", tu.Name, tu.LocName);
        throw new SgmException(text, text);
      }

    }
    private string GetArbeitsplatzLocationForVtwAbgabeLocation(string locationName)
    {

      switch (locationName)
      {         
    	
    	
   		case "1230" :
   			return "1234;
   		
   		case "1220" :
   			return "1225;
   		
   		case "1210" :
   			return "1214;
   		


	
     case "1255":
     	return "1256";
     
     case "1265":
     	return "1266";
     
     case "1275":
     	return "1276";
     

        default:
          string text = string.Format("LocationName [{0}] ist nicht zulässig.", locationName);
          _log.Error(text);
          throw new ApplicationException(text);
      }
    }


    /// <summary>
    /// Löscht den übergebene Teilauftrag aus der ArbeitsplatzQueue
    /// </summary>
    /// <param name="teilauftragId"></param>
    private void LoescheTeilauftragAusArbeitplatzQueue(decimal teilauftragId)
    {
      ArbeitsplatzQueue arbeitsplatzQueue = DataAccess.SelectFirst<ArbeitsplatzQueue>(ArbeitsplatzQueue.Properties.TaId.Filter(teilauftragId));
      if (arbeitsplatzQueue == null)
      {
        _log.WarnFormat("Es wurde kein Eintrag in der ArbeitsplatzQueue für Teilauftrag [{0}] gefunden. Konnte nicht gelöscht werden.", teilauftragId);
        return;

      }
      _log.InfoFormat("Eintrag in die ArbeitsplatzQueue [{0}] wird gelöscht", arbeitsplatzQueue);
      DataAccess.Delete(arbeitsplatzQueue);
      return;
    }

    /// <summary>
    /// Überprüft ob mindestens ein Tablar zu dem übergebenen Teilauftrag auf einem AklAuslagerstich steht. 
    /// </summary>
    /// <param name="teilauftrag"></param>
    /// <returns></returns>
    private bool IsAnyTablarAusgelagert(Teilauftrag teilauftrag)
    {
      Query query = SgmPhysik.GetAllTusOnAuslagersticheAkl();
      query.InnerJoin<TuPos>(Expression.IsEqual(TuPos.Properties.TruName, Tu.Properties.Name));
      query.InnerJoin<Entnahme>(Expression.IsEqual(Entnahme.Properties.TupId, TuPos.Properties.Id));
      query.InnerJoin<TeilauftragPos>(Expression.IsEqual(TeilauftragPos.Properties.Id, Entnahme.Properties.TapId));
      query.Where(TeilauftragPos.Properties.TaId.Filter(teilauftrag.Id));
      IList<SgmTu> result = DataAccess.ExecuteQuery(query, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      if (_log.IsDebugEnabled)
      {
        _log.DebugFormat("Es wurden [{0}] ausgelagerte Tablare zu Teilauftrag [{1}] gefunden. Tablare [{2}]", result.Count, teilauftrag.Id, string.Join("],[", result.Select(tu => tu.Name).ToArray()));
      }

      if (result.Count > 0)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Überprüft ob mindestens eine Entnahme zum übergebene Teilauftrag im Status TordAngelegt ist.
    /// </summary>
    /// <param name="teilauftrag"></param>
    /// <returns></returns>
    private bool IsAnyTordAngelegt(Teilauftrag teilauftrag)
    {
      IList<Entnahme> entnahmen = TeilauftragVerwaltung.ErmittleEntnahmenZuTeilauftrag(teilauftrag.Id);
      decimal anzEntnahmenInTordAngelegt = entnahmen.Where(x => x.Status == Entnahme.StatusValue.TordAngelegt).Count();

      _log.DebugFormat("Es wurden [{0}] Tablare für Teilauftrag [{1}] im Status [TordAngelegt] gefunden", anzEntnahmenInTordAngelegt, teilauftrag.Id);

      if (entnahmen.Any(x => x.Status == Entnahme.StatusValue.TordAngelegt))
      {
        return true;
      }
      return false;
    }

    private Teilauftrag GetNextTeilauftragIdFromArbeitplatzQueue(decimal arbeitsplatzId)
    {
      Query query = new Query("GetNextTeilauftragFromArbeitsplatzQueue");
      query.From<ArbeitsplatzQueue>();
      query.Where(ArbeitsplatzQueue.Properties.ArbId.Filter(arbeitsplatzId));
      query.InnerJoin<Teilauftrag>(Expression.IsEqual(Teilauftrag.Properties.Id, ArbeitsplatzQueue.Properties.TaId));
      query.OrderBy(ArbeitsplatzQueue.Properties.Id);

      IList<Teilauftrag> result = DataAccess.ExecuteQuery(query, row => (Teilauftrag)row[Teilauftrag.Properties.AliasName]);

      if (result.Count == 0)
      {
        // null zurückgeben wenn kein Teilauftrag gefunden wurde. 
        return null;
      }

      return result[0];
    }


    /// <summary>
    /// Liefert den Kommissionierbehälter, der sich am Kommissionierplatz zum Arbeitsplatz befindet und zum übergebenen
    /// Teilauftrag gehört
    /// </summary>
    /// <param name="teilauftrag">der aktuelle Teilauftrag</param>
    /// <param name="arbeitsplatzLocName">die Location des Arbeitsplatzes</param>
    /// <returns>null oder der Kommissionierbehälter</returns>
    private string GetKommissionierBehaelterAufKomplatzFuer(Teilauftrag teilauftrag, string arbeitsplatzLocName)
    {
      Auftrag.AuftragsArtValue auftragsArt = TeilauftragVerwaltung.GetAuftragsArt(teilauftrag.AufId.Value);

      // Falls es sich um einen Nachschub handelt
      if (auftragsArt == Auftrag.AuftragsArtValue.BksNachschub)
      {
        string nbvTuName = ConstDirectory.NBV_PREFIX + ConstDirectory.LAGERTEIL_AKL + teilauftrag.AufId.ToString().Substring(3, 6);

        //Legt die Tu an, falls sie noch nicht existiert.
        TransferTuAnlegen(nbvTuName, ConstDirectory.LOC_NAME_TRANSFER_BESTAND_NBV);

        return nbvTuName;
      }

      // Falls es sich um einen Nachschub handelt
      if (auftragsArt == Auftrag.AuftragsArtValue.Umlagerung)
      {
        string umlTuName = ConstDirectory.GetUmlTuName(Base.Lagerteil.Akl, teilauftrag.AufId.Value);

        //Legt die Tu an, falls sie noch nicht existiert.
        TransferTuAnlegen(umlTuName, ConstDirectory.LOC_NAME_TRANSFER_BESTAND_UML);

        return umlTuName;
      }

      string tuName = GetKommissionierBehaelterAufKomplatz(arbeitsplatzLocName);

      if (!string.IsNullOrEmpty(tuName))
      {
        AklTeilauftragTu teilauftragTu = DataAccess.SelectFirst<AklTeilauftragTu>(AklTeilauftragTu.Properties.TruName.Filter(tuName));

        if (teilauftragTu == null)
        {
          // Diese Tu ist noch keinem Teilauftrag zugeordnet und kann für diesen Teilauftrag verwendet werden.
          return tuName;
        }
        if (teilauftragTu.TaId.Equals(teilauftrag.Id))
        {
          // Diese Tu ist bereits mit dem Teilauftrag verheiratet und kann verwendet werden.
          return tuName;
        }
      }

      return null;
    }

    /// <summary>
    /// Liefert den Kommissionierbehälter, der sich am Kommissionierplatz zum Arbeitsplatz befindet und zum übergebenen
    /// Teilauftrag gehört
    /// </summary>
    /// <param name="teilauftragId">der aktuelle Teilauftrag</param>
    /// <param name="arbeitsplatzLocName">die Location des Arbeitsplatzes</param>
    /// <returns>null oder der Kommissionierbehälter</returns>
    private string GetKommissionierBehaelterAufKomplatz(string arbeitsplatzLocName)
    {
      if (string.IsNullOrEmpty(arbeitsplatzLocName)) throw new ArgumentNullException("arbeitsplatzLocName");
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein gültiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      SgmLocation komPlatz = GetKomPlatzFuerArbeitsplatz(arbeitsplatzLocName);

      TuQuery tuQuery = SgmPhysik.GetTusOn(komPlatz.Name);

      IList<SgmTu> tus = DataAccess.ExecuteQuery(tuQuery, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      if (tus.Count > 0)
      {
        SgmTu tu = tus.First();

        return tu.Name;
      }

      return null;
    }

    private bool IsAklArbeitsplatz(string name)
    {
      switch (name)
      {
        case ConstDirectory.AKL_ARBEITSPLATZ_1_LOCATION_NAME:
        case ConstDirectory.AKL_ARBEITSPLATZ_2_LOCATION_NAME:
        case ConstDirectory.AKL_ARBEITSPLATZ_3_LOCATION_NAME:
          return true;
        default:
          return false;
      }
    }

    private SgmLocation GetKomPlatzFuerArbeitsplatz(string arbeitsplatzLocation)
    {
      switch (arbeitsplatzLocation)
      {
        case "1234":
          {
            return (SgmLocation)SgmPhysik.GetLocationByName("KOMPLATZ03");
          }
        case "1225":
          {
            return (SgmLocation)SgmPhysik.GetLocationByName("KOMPLATZ02");
          }
        case "1214":
          {
            return (SgmLocation)SgmPhysik.GetLocationByName("KOMPLATZ01");
          }
      }
      throw new LocationNotFoundException("Kein Kommissionierplatz für Arbeitsplatz [{0}] gefunden", arbeitsplatzLocation);
    }

    private void KommissionierBoxAufKomAuslaufBuchen(SgmTu tu)
    {
      SgmLocation komAuslauf = (SgmLocation)SgmPhysik.GetLocationByName(ConstDirectory.LOC_NAME_KOMMPLATZ_AUSLAUF);

      Teilauftrag teilauftrag = Lagerteil.GetTeilauftragZuTu(tu);

      //Kommissionierbehälter ist keinem Teilauftrag zugeordnet und wird auf NIO gebucht.
      if (teilauftrag == null)
      {
        _log.WarnFormat("Kommissionierbehälter [{0}] ist keinem Teilauftrag zugeordnet und wird auf NIO gebucht",
                        tu.Name);

        tu.LocName = ConstDirectory.NIO;
        SgmPhysik.UpdateTu(tu);

        _log.DebugFormat("Tu [{0}] wurde auf [{1}] gebucht", tu.Name, tu.LocName);
        return;
      }

      tu.LocName = komAuslauf.Name;
      SgmPhysik.UpdateTu(tu);

      _log.DebugFormat("Tu [{0}] wurde auf [{1}] gebucht", tu.Name, tu.LocName);
    }

    /// <summary>
    /// Löscht nicht mehr benötigte TuPosen, updatet die geänderten TuPosen und legt neue 0-Mengen TuPosen an
    /// </summary>
    private void UpdateUmgelagertenTuInhalt(IDictionary<SgmTuPos, IList<int>> anzulegendeTuPosen, IDictionary<SgmTuPos, SgmTuPos> tuPosenZumUmbuchen)
    {
      // Alle TuPosen mit 0-Menge werden gelöscht
      IList<SgmTuPos> tuPosenZumLoeschen = SgmMaterialManagement.GetAllTuPosForNamedTu(anzulegendeTuPosen.First().Key.TruName);
      tuPosenZumLoeschen = tuPosenZumLoeschen.Where(x => x.Amount == 0).ToList();
      foreach (SgmTuPos tuPos in tuPosenZumLoeschen)
      {
        _log.InfoFormat("Umlagern: Lösche nicht mehr benötigte TuPos [{0}]", tuPos);
        SgmMaterialManagement.DeleteTuPos(tuPos);
      }

      // Entnahmen umrefferenzieren
      EntnahmenUmbuchen(tuPosenZumUmbuchen);

      // Zulagerungen umrefferenzieren
      ZulagerungenUmbuchen(tuPosenZumUmbuchen);

      // Alle TuPosen die zu anderen TuPosen hinzugebucht werden werden gelöscht
      foreach (SgmTuPos tuPos in tuPosenZumUmbuchen.Keys)
      {
        _log.InfoFormat("Umlagern: Lösche nicht mehr benötigte TuPos [{0}]", tuPos);
        SgmMaterialManagement.DeleteTuPos(tuPos);
      }
      // Alle anderen TuPosen updaten und die 0-Mengen TuPosen hinzufügen
      foreach (KeyValuePair<SgmTuPos, IList<int>> tuPos in anzulegendeTuPosen)
      {
        SgmTuPos localTuPos = tuPos.Key;
        IList<int> quanten = tuPos.Value.OrderBy(x => x).ToList();


        localTuPos.Quant = (SgmTuPos.QuantValue)quanten[0];
        _log.InfoFormat("Umlagern: Update TuPos [{0}]", localTuPos);
        SgmMaterialManagement.UpdateTuPos(localTuPos);

        quanten.RemoveAt(0);
        foreach (decimal quant in quanten)
        {
          _log.InfoFormat("Umlagern: Lege 0-Mengen TuPos zu BasisTuPos[{0}] auf Quant [{1}] an", localTuPos, quant);
          SgmMaterialManagement.TuPosOhneMengeAnlegenOderErweitern(localTuPos.Id, localTuPos.TruName, localTuPos.MatId, localTuPos.DatiMhd,
                                                                   localTuPos.Sonderbestandsnummer, (int)quant, (decimal)localTuPos.Bestandsart, null, null);
        }
      }
    }

    /// <summary>
    /// Selectiert alle Entnahmen zu den Key-TuPosen und bucht sie auf die neuen TuPosen um  
    /// </summary>
    /// <param name="zuUeberfuehrendeTuPosen"></param>
    private void EntnahmenUmbuchen(IDictionary<SgmTuPos, SgmTuPos> zuUeberfuehrendeTuPosen)
    {
      // TODO: Die Entnahme gehört der TeilauftragVerwaltung -> Methode umziehen. 
      Query query = new Query("Entnahmen");
      query.From<Entnahme>();

      IList<Expression> orExpressions = new List<Expression>();
      foreach (KeyValuePair<SgmTuPos, SgmTuPos> pair in zuUeberfuehrendeTuPosen)
      {
        orExpressions.Add(Entnahme.Properties.TupId.Filter(pair.Key.Id));
      }
      query.Where(Expression.Or(orExpressions.ToArray()));

      var entnahmen = DataAccess.ExecuteQuery(query, row => (Entnahme)row[Entnahme.Properties.AliasName]);
      foreach (Entnahme entnahme in entnahmen)
      {
        SgmTuPos quellTuPos = zuUeberfuehrendeTuPosen.Keys.Where(x => x.Id == entnahme.TupId).Select(x => x).First();
        TeilauftragVerwaltung.EntnahmeUmbuchen(entnahme, zuUeberfuehrendeTuPosen[quellTuPos]);
      }
    }

    /// <summary>
    /// Selectiert alle Zulagerungen zu den Key-TuPosen und bucht sie auf die neuen TuPosen um  
    /// </summary>
    /// <param name="zuUeberfuehrendeTuPosen"></param>
    private void ZulagerungenUmbuchen(IDictionary<SgmTuPos, SgmTuPos> zuUeberfuehrendeTuPosen)
    {
      Query query = new Query("Zulagerung");
      query.From<Zulagerung>();

      IList<Expression> orExpressions = new List<Expression>();
      foreach (KeyValuePair<SgmTuPos, SgmTuPos> pair in zuUeberfuehrendeTuPosen)
      {
        orExpressions.Add(Zulagerung.Properties.TupId.Filter(pair.Key.Id));
      }
      query.Where(Expression.Or(orExpressions.ToArray()));

      var zulagerungen = DataAccess.ExecuteQuery(query, row => (Zulagerung)row[Zulagerung.Properties.AliasName]);
      foreach (Zulagerung zulagerung in zulagerungen)
      {
        SgmTuPos quellTuPos = zuUeberfuehrendeTuPosen.Keys.Where(x => x.Id == zulagerung.TupId).Select(x => x).First();
        SgmTuPos zielTuPos = zuUeberfuehrendeTuPosen[quellTuPos];
        AvisenVerwaltung.ZulagerungUmbuchen(zulagerung, zielTuPos);
      }
    }

    /// <summary>
    /// Wandelt einen Kommaseparierten String mit Quanten in eine sortierte Integer-Liste um
    /// </summary>
    /// <param name="quantenString"></param>
    /// <returns></returns>
    private IList<int> QuantenStringToList(string quantenString)
    {
      IList<int> neueQuanten = new List<int>();

      string[] quanten = quantenString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string quant in quanten)
      {
        neueQuanten.Add(Convert.ToInt32(quant));
      }
      neueQuanten = neueQuanten.OrderBy(x => x).ToList();

      return neueQuanten;
    }

    private InventurZaehlung.InventurArtValue GetInventurArt(SgmTuPos tuPos, string arbeitsplatzLocation)
    {
      Arbeitsplatz arbeitsplatz = AklArbeitsplatzVerwaltung.GetArbeitsplatzByLocation(arbeitsplatzLocation);

      if (arbeitsplatz.Modus.Equals(Arbeitsplatz.ModusValue.Inventur))
      {
        return InventurZaehlung.InventurArtValue.Inventur;
      }

      if (tuPos.MarkZaehlen)
      {
        return InventurZaehlung.InventurArtValue.NaheNullDurchgang;
      }

      bool istHintergrundInvAktiv = SystemParameters.GetBool("HINTERGRUND_INVENTUR", false);
      if (istHintergrundInvAktiv)
      {
        return InventurZaehlung.InventurArtValue.HintergrundInventur;
      }

      return InventurZaehlung.InventurArtValue.MengenKorrektur;
    }

    #endregion
  }
}

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

