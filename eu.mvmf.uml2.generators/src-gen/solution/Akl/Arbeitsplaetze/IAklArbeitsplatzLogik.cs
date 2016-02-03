//******************************************************************************
// NAME: IAklArbeitsplatzLogik.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;
using Commons;
using Siemens.WarehouseManagement.Infrastructure.Configuration;
using Solution.Commons;

namespace Sgm.Akl.Arbeitsplaetze
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAklArbeitsplatzLogik
  {

    /// <summary>
    /// Liefert das Tablar, die auf dem �bergebenen Arbeitsplatz gebucht ist.
    /// </summary>
    /// <remarks>
    /// Liefert null wenn keine Tu auf dem Arbeitsplatz gebucht ist
    /// </remarks>
    /// <param name="arbeitsplatzLocName">Der Location Name des Arbeitsplatzes</param>
    /// <returns>Die Tu (Tablar) die auf dem Arbeitsplatz gebucht ist</returns>
    [Synchronous]
    SgmTu GetTablarDaten(string arbeitsplatzLocName);

    /// <summary>
    /// Liefert eine Liste, die alle TuPosen der �bergebenen Tu enth�lt
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns>eine Liste mit TuPosen</returns>
    [Synchronous]
    IList<SgmTuPos> GetTablarInhalt(string tuName);

    /// <summary>
    /// Liefert eine Query, die alle TuPosen der �bergebenen Tu enth�lt
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns>eine TuPos Query</returns>
    [Synchronous]
    Query GetTablarInhaltQuery(string tuName);

    /// <summary>
    /// Liefert das Material zu den �bergebenen Sap Daten
    /// </summary>
    /// <param name="taNum">die Sap TaNum</param>
    /// <param name="taPos">die Sap TaPos</param>
    /// <param name="weNum">die Sap WeNum</param>
    /// <param name="wePos">die Sap WePos</param>
    /// <returns>das gesuchte Material</returns>
    [Synchronous]
    SgmMaterial GetMaterial(string taNum, string taPos, string weNum, string wePos);

    /// <summary>
    /// Liefert das Material zu der �bergebenen Logition Material Id
    /// </summary>
    /// <param name="id">die Logition Material id</param>
    /// <returns></returns>
    [Synchronous]
    SgmMaterial GetMaterial(decimal id);

    /// <summary>
    /// Liefert die AvisePos zu den �bergebenen Sap-Daten.
    /// Falls keine oder mehere AvisePos gefunden werden, wird eine AvisePositionNichtGefundenException geworden
    /// </summary>
    /// <param name="weNum">die Sap We-Nummer</param>
    /// <param name="wePos">die Sap We-Position</param>
    /// <param name="taNum">die Sap Ta-Nummer</param>
    /// <param name="taPos">die Sap Ta-Pos</param>
    /// <returns>die AvisePosition</returns>
    [Synchronous]
    AvisePos GetAvisePositon(string weNum, string wePos, string taNum, string taPos);

    /// <summary>
    /// Liefert die Menge, die maximal noch f�r die �bergebenen AvisePos eingelagert
    /// werden darf.
    /// </summary>
    /// <param name="avisePos"></param>
    /// <returns></returns>
    [Synchronous]
    decimal GetMaxZulagerMenge(AvisePos avisePos);

    /// <summary>
    /// �berpr�ft, ob ein Material gewogen werden muss.
    /// </summary>
    /// <param name="material">das Materials</param>
    /// <returns>true oder false</returns>
    [Synchronous]
    bool MussMaterialGewogenWerden(SgmMaterial material);

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <param name="arbeitsPlatz">Die Location des Arbeitsplatzes</param>
    /// <param name="anzQuanten">Anzahl der ben�tigten leeren Quanten auf dem Tablar</param>
    /// <param name="hoehe">Die Hoehe des Tablars</param>
    /// <param name="hasRahmen">Mit Rahmen?</param>
    /// <returns>Die Tablarnummer</returns>
    string LeerTablarAuslagern(string arbeitsPlatz, int anzQuanten, int hoehe, bool hasRahmen);

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
    bool MaterialZugelagert(string tuName, IList<int> quanten, decimal menge, string weNum, string wePos, string taNum, string taPos, decimal gewicht, string terminalName, string userName, bool endQuittierungsKennzeichen);

    /// <summary>
    /// �berpr�ft alle TuPosen und legt ggf. Posen zusammen. L�scht alle 0-Mengen Tuposen und legt diese neu an.
    /// </summary>
    /// <param name="tuPosIds">Alle Tu-Pos-Ids auf der geaenderten Tu</param>
    /// <param name="quantenStrings">Die neuen Quanten als kommaseparierter String</param>
    void MaterialUmgelagert(IList<int> tuPosIds, IList<string> quantenStrings);
    
    /// <summary>
    /// Transportiert ein Tablar vom Arbeitsplatz ins Lager.
    /// </summary>
    /// <param name="arbeitsplatzLocation">Der Name der Arbeitsplatz-Location</param>
    void TransportiereTablarAb(string arbeitsplatzLocation);

    /// <summary>
    /// Legt einen neuen Beh�lter zur Kommissionierung an
    /// </summary>
    /// <param name="behaelterName">der Name des Beh�lters</param>
    /// <param name="arbeitsplatzLocNameLocation">der Name des Arbeitsplatzes</param>
    void KommissionierBehaelterAnlegen(string behaelterName, string arbeitsplatzLocNameLocation);

    /// <summary>
    /// Liefert alle Tus, die sich auf dem �bergebenen Arbeitsplatz befinden
    /// </summary>
    /// <param name="arbeitsplatzLocation">der Arbeitsplatz</param>
    /// <returns></returns>
    [Synchronous]
    IList<SgmTu> GetTusAufArbeitsplatz(string arbeitsplatzLocation);

    /// <summary>
    /// Ermittelt alle Entnahmen im Status AufArbeitsplatz die f�r die �bergebenen Tu existieren
    /// </summary>
    /// <param name="tuName">der Tu Name</param>
    /// <returns></returns>
    [Synchronous]
    IList<Entnahme> GetEntnahmenAufArbeitsplatzFuerTu(string tuName);

    /// <summary>
    /// Liefert die Entnahme zu der �bergebenen Id
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <returns>die Entnahme</returns>
    [Synchronous]
    Entnahme GetEntnahmeAusId(decimal entnahmeId);

    /// <summary>
    /// Liefert die SgmTu anhand des Namens
    /// </summary>
    /// <param name="tuName">der Name der Tu</param>
    /// <returns>die SgmTu</returns>
    [Synchronous]
    SgmTu GetTuAusName(string tuName);

    ///// <summary>
    ///// Liefert die Anzahl der Entnahemen mit Status Bereit, f�r die �bergebene Tu
    ///// </summary>
    ///// <param name="tuName"></param>
    ///// <returns></returns>
    ////int GetAnzahlBereiteEntnahmenFuerTu(string tuName);

    /// <summary>
    /// Liefert eine Liste mit allen Quadranten zur�ck, die von TuPositionen belegt sind, die die �bergebene TuPos
    /// als BasisTuPos haben
    /// </summary>
    /// <param name="basisTuPos">die TuPos mit der Menge > 0</param>
    /// <returns>Die List der TuPosen</returns>
    [Synchronous]
    IList<int> GetAlleQuadrantenVerlinktMit(SgmTuPos basisTuPos);

    /// <summary>
    /// Liefert die EntnahmeDaten f�r die n�chste Entnahme. Wird keine Tu am Arbeitsplatz gefunden 
    /// so wird null zur�ckgegeben. Sind f�r die Tu am Arbeitsplatz keine 
    /// bereiten Entnahmen mehr vorhanden, wird das Property EntnahmeDaten.AnzahlKommissionierungen auf 0 gesetzt.
    /// </summary>
    /// <returns>die EntnahmeDaten</returns>
    [Synchronous]
    EntnahmeDaten GetEntnahmeDaten(string arbeitsplatzLocation);

    /// <summary>
    /// Liefert eine Liste mit Konturenfehlern, die sich hinter der �bergebenen Integer Zahl verbergen.
    /// </summary>
    /// <param name="alleKonturenfehler"></param>
    /// <returns></returns>
    [Synchronous]
    IList<string> GetKonturenfehler(int? alleKonturenfehler);

    /// <summary>
    /// Setzt den Konturenfehler der Tu am Arbeitsplatz zur�ck
    /// </summary>
    void SetzeKonturenfehlerZurueck(string arbeitsplatz);

    /// <summary>
    /// F�hrt die Umbuchung der kommissionierten Menge auf den Kommissionierbeh�lter aus.
    /// </summary>
    /// <param name="entnahmeId">die Id der Entnahme</param>
    /// <param name="entnahmeMenge">die tats�chlich entnommene Menge</param>
    /// <param name="komTuName">Name des Kommissionierbeh�lters</param>
    /// <param name="terminalName">Name des Terminals</param>
    /// <returns>Ist der aktuelle Teilauftrag mit dieser Entnahme abgeschlossen ja/nein</returns>
    bool EntnahmeBestaetigen(decimal entnahmeId, decimal entnahmeMenge, string komTuName, string terminalName, string userName);

    /// <summary>
    /// Die Materialmenge wird korrigiert (Grund: entnahme aufgrund von Gewichtsfehler oder/und Konturenfehler)
    /// </summary>
    /// <param name="tuPosId">die TuPos, deren Menge korregiert werden soll</param>
    /// <param name="entnommeneMenge">Die entnommene Menge des Materials</param>
    /// <param name="avisePosId">die AvisePos Id</param>
    void MaterialMengenKorrektur(decimal tuPosId, decimal entnommeneMenge, decimal avisePosId);

    /// <summary>
    /// Liefert einen RemoteEntityContainer mit allen TuPosen und der Menge die zu nicht abgeschlossenen AvisePositionen
    /// geh�ren. Die TuPosen befinden sich auf der Tu die am aktuellen Arbeitsplatz steht
    /// </summary>
    /// <returns></returns>
    [Synchronous]
    Query GetEntnehmbaresMaterial(string arbeitsplatzLocation);

    /// <summary>
    /// Liefert alle AklTeilauftragPositionen mit Material zu einer SapTaNum
    /// </summary>
    /// <param name="sapTaNum">die SapAuftragsNummer</param>
    /// <returns>AklTeilauftragPositionen und Material</returns>
    [Synchronous]
    Query GetTeilauftragPositionen(string sapTaNum);

    /// <summary>
    /// Tus the pos amount korrektur.
    /// </summary>
    /// <param name="tuPosIds">The decimals.</param>
    /// <param name="amounts">The amounts.</param>
    /// <param name="user">The user.</param>
    /// <param name="terminalName">der Name des Terminals</param>
    /// <param name="arbeitsplatzLocation">der Name der Arbeitsplatzes Location</param>
    bool InventurVerbuchen(IList<int> tuPosIds, IList<decimal> amounts, string user, string terminalName, string arbeitsplatzLocation);

    /// <summary>
    /// Erzeugt einen Transportauftrag f�r die �bergebene Tu zum Arbeitsplatz
    /// </summary>
    /// <param name="tuName">der Name des Tablars</param>
    /// <param name="arbeitsPlatzLocName">die Location des Arbeitsplatzes</param>
    void TablarAnfordern(string tuName, string arbeitsPlatzLocName);

    /// <summary>
    /// Es werden alle TUS mit einem bestimmten Material ausgelagert
    /// </summary>
    /// <param name="materialId"></param>
    /// <param name="arbeitsPlatzLocName"></param>
    void MaterialAnfordern(decimal materialId, string arbeitsPlatzLocName);

    /// <summary>
    /// �berpr�ft, ob eine Inventurz�hlung f�r diese TuPos durchgef�hrt werden muss.
    /// </summary>
    /// <param name="tuPosId"></param>
    /// <returns></returns>
    [Synchronous]
    bool IstZeitstempelGueltig(decimal tuPosId);

    /// <summary>
    /// Pr�ft ob durch das Picken der Entnahmen ein Nahe-0-Durchgang entsteht.
    /// </summary>
    /// <param name="arbeitsplatzLocation"></param>
    /// <returns></returns>
    [Synchronous]
    IList<decimal> GetTuPositionenMitNaheNullDurchgang(string arbeitsplatzLocation);

    /// <summary>
    /// Liefert die Betriebsart des Arbeitsplatzes
    /// </summary>
    /// <returns></returns>
    [Synchronous]
    Arbeitsplatz.ModusValue GetArbeitsplatzBetriebsart(string arbeitsPlatzLocation);
   
    /// <summary>
    /// Liefert den Arbeitsplatz dem der Teilauftrag entweder aktiv oder in der Queue zugeordnet ist.
    /// Liefert null wenn der Teilauftrag nicht in Arbeitsplatz/ArbeitsplatzQueue gefundenwird.
    /// </summary>
    /// <param name="teilauftragId"></param>
    /// <returns></returns>
    [Synchronous]
    Arbeitsplatz GetArbeitsplatzForTeilauftrag(decimal teilauftragId);

    /// <summary>
    /// Aktiviert den n�chsten Auftrag aus der Queue. Ist die Queue Leer, wird die TaId auf null gesetzt.
    /// </summary>
    /// <param name="arbeitplatzLocation"></param>
    void AktiviereTeilauftragAusQueue(string arbeitplatzLocation);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
