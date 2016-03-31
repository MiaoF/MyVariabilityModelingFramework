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
  /// Stellt die Logik f�r die Arbeitspl�tze zur Verf�gung z.B. Wechsel der Betriebsart, Anforderung Leergut
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
    /// Liefert das Tablar, die auf dem �bergebenen Arbeitsplatz gebucht ist.
    /// </summary>
    /// <remarks>
    /// Liefert null wenn keine Tu auf dem Arbeitsplatz gebucht ist
    /// </remarks>
    /// <param name="arbeitsplatzLocName">Der Location Name des Arbeitsplatzes</param>
    /// <returns>Die Tu (Tablar) die auf dem Arbeitsplatz gebucht ist</returns>
    public SgmTu GetTablarDaten(string arbeitsplatzLocName)
    {
      
		//TODO: need to be implement
      	return null;
    }

    /// <summary>
    /// Liefert eine Query, die alle TuPosen der �bergebenen Tu enth�lt
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns>eine TuPos Query</returns>
    public IList<SgmTuPos> GetTablarInhalt(string tuName)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert eine Query, die alle TuPosen der �bergebenen Tu enth�lt
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
    /// Liefert das Material zu den �bergebenen Sap Daten
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
    /// Liefert das Material zu der �bergebenen Logition Material Id
    /// </summary>
    /// <param name="id">die Logition Material id</param>
    /// <returns></returns>
    public SgmMaterial GetMaterial(decimal id)
    {
      //TODO: need to be implementv
      return null;
    }

    /// <summary>
    /// Liefert die AvisePos zu den �bergebenen Sap-Daten.
    /// Falls keine oder mehere AvisePos gefunden werden, wird eine AvisePositionNichtGefundenException geworden
    /// </summary>
    /// <param name="weNum">die Sap We-Nummer</param>
    /// <param name="wePos">die Sap We-Position</param>
    /// <param name="taNum">die Sap Ta-Nummer</param>
    /// <param name="taPos">die Sap Ta-Pos</param>
    /// <returns>die AvisePosition</returns>
    public AvisePos GetAvisePositon(string weNum, string wePos, string taNum, string taPos)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert die Menge, die maximal noch f�r die �bergebenen AvisePos eingelagert
    /// werden darf.
    /// </summary>
    /// <param name="avisePos"></param>
    /// <returns></returns>
    public decimal GetMaxZulagerMenge(AvisePos avisePos)
    {
       //TODO: need to be implement
       return null;
    }

    /// <summary>
    /// �berpr�ft, ob ein Material gewogen werden muss.
    /// </summary>
    /// <param name="material">das Material</param>
    /// <returns>true oder false</returns>
    public bool MussMaterialGewogenWerden(SgmMaterial material)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <returns>Die Tablarnummer</returns>
    public string LeerTablarAuslagern(string arbeitsplatzLocName, int anzQuanten, int hoehe, bool hasRahmen)
    {
      //TODO: need to be implement
      return null;
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
     //TODO: need to be implement
      return true;
    }

    /// <summary>
    /// Transportiert ein Tablar vom Arbeitsplatz ins Lager.
    /// </summary>
    public void TransportiereTablarAb(string arbeitsplatzLocName)
    {
      if (IsAklArbeitsplatz(arbeitsplatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein g�ltiger AKL Arbeitsplatz", arbeitsplatzLocName);
        throw new SgmException(text, text);
      }

      //TODO: need to be implement
    }

    /// <summary>
    /// Legt einen neuen Beh�lter zur Kommissionierung an
    /// </summary>
    /// <param name="behaelterName">der Name des Beh�lters</param>
    /// <param name="arbeitsplatzLocName">der Name des Arbeitsplatzes</param>
    public void KommissionierBehaelterAnlegen(string behaelterName, string arbeitsplatzLocName)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Ermittelt alle Entnahmen im Status AufArbeitsplatz die f�r die �bergebenen Tu existieren
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns></returns>
    public IList<Entnahme> GetEntnahmenAufArbeitsplatzFuerTu(string tuName)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert die Entnahme zu der �bergebenen Id
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <returns>die Entnahme</returns>
    public Entnahme GetEntnahmeAusId(decimal entnahmeId)
    {
     //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert die SgmTu anhand des Namens
    /// </summary>
    /// <param name="tuName">der Name der Tu</param>
    /// <returns>die SgmTu</returns>
    public SgmTu GetTuAusName(string tuName)
    {
     //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert eine Liste mit allen Quadranten zur�ck, die von TuPositionen belegt sind, die die �bergebene TuPos
    /// als BasisTuPos haben
    /// </summary>
    /// <param name="basisTuPos">die TuPos mit der Menge > 0</param>
    /// <returns>Die List der TuPosen</returns>
    public IList<int> GetAlleQuadrantenVerlinktMit(SgmTuPos basisTuPos)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert alle Tus, die sich auf dem �bergebenen Arbeitsplatz befinden
    /// </summary>
    /// <param name="arbeitsplatzLocName">der Arbeitsplatz</param>
    /// <returns></returns>
    public IList<SgmTu> GetTusAufArbeitsplatz(string arbeitsplatzLocName)
    {
     //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert die EntnahmeDaten f�r die n�chste Entnahme. Wird keine Tu am Arbeitsplatz gefunden 
    /// so wird null zur�ckgegeben. Sind f�r die Tu am Arbeitsplatz keine 
    /// bereiten Entnahmen mehr vorhanden, wird das Property EntnahmeDaten.AnzahlKommissionierungen auf 0 gesetzt.
    /// </summary>
    /// <returns>die EntnahmeDaten</returns>
    public EntnahmeDaten GetEntnahmeDaten(string arbeitsplatzLocName)
    {
     //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert eine Liste mit Konturenfehlern, die sich hinter der �bergebenen Integer Zahl verbergen.
    /// </summary>
    /// <param name="alleKonturenfehler"></param>
    /// <returns></returns>
    public IList<string> GetKonturenfehler(int? alleKonturenfehler)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Setzt den Konturenfehler der Tu am Arbeitsplatz zur�ck
    /// </summary>
    public void SetzeKonturenfehlerZurueck(string arbeitsplatz)
    {
      //TODO: need to be implement
    }

    /// <summary>
    /// F�hrt die Umbuchung der kommissionierten Menge auf den Kommissionierbeh�lter aus.
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <param name="entnahmeMenge">die tats�chlich entnommene Menge</param>
    /// <param name="komTuName">Name des Kommissionierbeh�lters</param>
    /// <returns>Ist der aktuelle Teilauftrag mit dieser Entnahme abgeschlossen ja/nein</returns>
    public bool EntnahmeBestaetigen(decimal entnahmeId, decimal entnahmeMenge, string komTuName, string terminalName, string userName)
    {
     //TODO: need to be implement
      return null;
    }

    
    /// <summary>
    /// Liefert einen RemoteEntityContainer mit allen TuPosen und der Menge die zu nicht abgeschlossenen AvisePositionen
    /// geh�ren. Die TuPosen befinden sich auf der Tu die am aktuellen Arbeitsplatz steht
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
		//TODO: need to be implement
		return false;
    }

    /// <summary>
    /// Erzeugt einen Transportauftrag f�r die �bergebene Tu zum Arbeitsplatz
    /// </summary>
    /// <param name="tuName">der Name des Tablars</param>
    /// <param name="arbeitsPlatzLocName">die Location des Arbeitsplatzes</param>
    public void TablarAnfordern(string tuName, string arbeitsPlatzLocName)
    {
       if (string.IsNullOrEmpty(tuName)) { throw new ArgumentNullException("tuName"); }
      if (string.IsNullOrEmpty(arbeitsPlatzLocName)) { throw new ArgumentNullException("arbeitsPlatzLocName"); }

      if (IsAklArbeitsplatz(arbeitsPlatzLocName) == false)
      {
        string text = string.Format("[{0}] ist kein g�ltiger AKL Arbeitsplatz", arbeitsPlatzLocName);
        throw new SgmException(text, text);
      }

      //TODO: to invoke transport generation
    }

    /// <summary>
    /// Es werden alle Tablare mit einem bestimmten Material ausgelagert
    /// </summary>
    /// <param name="materialId"></param>
    /// <param name="arbeitsPlatzLocName"></param>
    public void MaterialAnfordern(decimal materialId, string arbeitsPlatzLocName)
    {
      //TODO: need to be implement
    }

    /// <summary>
    /// �berpr�ft, ob eine Inventurz�hlung f�r diese TuPos durchgef�hrt werden muss.
    /// </summary>
    /// <param name="tuPosId"></param>
    /// <returns></returns>
    public bool IstZeitstempelGueltig(decimal tuPosId)
    {
      //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Pr�ft ob durch das Picken der Entnahmen ein Nahe-0-Durchgang entsteht.
    /// </summary>
    /// <param name="arbeitsplatzLocation"></param>
    /// <returns></returns>
    public IList<decimal> GetTuPositionenMitNaheNullDurchgang(string arbeitsplatzLocation)
    {
     //TODO: need to be implement
      return null;
    }

    /// <summary>
    /// Liefert die Betriebsart des Arbeitsplatzes
    /// </summary>
    /// <returns></returns>
    public Arbeitsplatz.ModusValue GetArbeitsplatzBetriebsart(string arbeitsPlatzLocation)
    {
		//TODO: need to be implement
		return null;
    }

    /// <summary>
    /// Liefert den Arbeitsplatz dem der Teilauftrag entweder aktiv oder in der Queue zugeordnet ist.
    /// Liefert null wenn der Teilauftrag nicht in Arbeitsplatz/ArbeitsplatzQueue gefundenwird.
    /// </summary>
    /// <param name="teilauftragId"></param>
    /// <returns></returns>
    public Arbeitsplatz GetArbeitsplatzForTeilauftrag(decimal teilauftragId)
    {
		//TODO: need to be implement

    }
    #endregion

    #region Implementation von IAklTransportAktivierungStatus

    /// <summary>
    /// Diese Funktion wird aufgerufen, wenn eine Tu eingelagert wurde.
    /// Sie �berpr�ft, ob zu der Tu eine Avise existiert und meldet ggf. an die AviseVerwaltung
    /// </summary>
    /// <param name="tu">die eingelagerte Tu</param>
    public void TuEingelagert(SgmTu tu)
    {
		//TODO: need to be implement
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
		//TODO: need to be implement
    }


    /// <summary>
    /// Aktiviert den n�chsten Auftrag aus der Queue. Ist die Queue Leer, wird die TaId auf null gesetzt.
    /// </summary>
    /// <param name="vtwAbgabeLocation"></param>
    public void AktiviereTeilauftragAusQueue(string vtwAbgabeLocation)
    {
		//TODO: need to be implement
    }

    #endregion

  }
}

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************