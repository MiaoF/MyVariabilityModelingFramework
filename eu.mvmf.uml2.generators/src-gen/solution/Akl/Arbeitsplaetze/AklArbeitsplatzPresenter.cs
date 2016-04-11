//******************************************************************************
// NAME: AklArbeitsplatzPresenter.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Commons;
using Sgm.Akl.AutomatikTransporte.Tma;
using Sgm.Akl.LeerbehaelterAuftraege;
using Sgm.Base;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.GenericPresenter;
using Siemens.WarehouseManagement.PresenterBase;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.UserManagement;
using Siemens.WarehouseManagement.UserManagementPresenter;
using Siemens.WarehouseManagement.Validation;
using Solution.Commons;

namespace Sgm.Akl.Arbeitsplaetze
{
  /// <summary>
  /// </summary>
  public class AklArbeitsplatzPresenter : PresenterBase, IAklMaterialEinlagernPresenter, IAklKommissionierungPresenter, IAklArbeitsplatzPresenter, IAklBetriebsartWechselnPresenter
  {
    private static log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    [RequiredProperty]
    public IUserManagement UserManagement { get; set; }

    [RequiredProperty]
    public IAklArbeitsplatzLogik AklArbeitsplatzLogik { get; set; }

    [RequiredProperty]
    public IAklArbeitsplatzVerwaltung AklArbeitsplatzVerwaltung { get; set; }

    [RequiredProperty]
    public IMiscTma MiscTma { get; set; }

    /// <summary>
    /// Zugriff auf die Leerbehaelter Bestellung
    /// </summary>
    [RequiredProperty]
    public IAklLeerbehaelterBestellung AklLeerbehaelterBestellung { get; set; }

    #region Implementation of IAklMaterialEinlagernPresenter

    /// <summary>
    /// Liefert die drei Hoehen der Tablare (niedrig, mittel, hoch)
    /// </summary>
    /// <returns></returns>
    public RemoteEntityContainer GetTablarHoehen()
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Liefert die Daten des Tablars, das grade auf dem Arbeitsplatz steht.
    /// </summary>
    /// <remarks>
    /// Liefert null wenn kein Tablar auf dem Arbeitsplatz gebucht ist
    /// </remarks>
    /// <returns>Die Daten eines Tablars</returns>
    public TablarDaten GetTablarDaten()
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Setzt den Konturenfehler der Tu am Arbeitsplatz zur�ck
    /// </summary>
    public void SetzeKonturenfehlerZurueck()
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Liefert den Status im Akl-Lagerteil.
    /// </summary>
    /// <returns></returns>
    public AklStatusLeisteDaten GetAklStatusLeisteDaten()
    {
		//TODO: need to be implement
      	return null;
    }

    private string GetQuadrantenBelegungString(decimal basisTuPosId, decimal? basisTuPosQuant)
    {
		//TODO: need to be implement
      	return null;
    }

    /// <summary>
    /// Liefert alle Tu-Positionen, die sich auf dem Arbeitsplatz befinden
    /// </summary>
    /// <returns></returns>
    public RemoteEntityContainer GetTablarInhalt(SgmTu dieTu, string arbeitsPlatz)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Liefert die Daten des Materials, das zu der �bergebenen AvisePos geh�rt
    /// </summary>
    /// <param name="taNum">die Sap TaNum. Entweder TaNum oder WeNum muss gesetzt sein</param>
    /// <param name="taPos">die Sap TaPos. Entweder TaPos oder WePos muss gesetzt sein</param>
    /// <param name="weNum">die Sap WeNum</param>
    /// <param name="wePos">die Sap WePos</param>
    /// <returns></returns>
    public MaterialDaten GetMaterialDaten(string taNum, string taPos, string weNum, string wePos)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <param name="anzTablare">Anzahl der ben�tigten Tablare</param>
    /// <param name="anzQuanten">Anzahl der ben�tigten leeren Quanten auf dem Tablar</param>
    /// <param name="hoehe">Die Hoehe des Tablars</param>
    /// <param name="hasRahmen">Mit Rahmen?</param>
    public void LeerTablarAuslagern(int anzTablare, int anzQuanten, int hoehe, bool hasRahmen)
    {
		//TODO: need to be implement
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
    /// <param name="taPos">Die gescannte Sap TaNum</param>
    /// <param name="gewicht">Das Gewicht des einzelnen Materials</param>
    /// <param name="endQuittierungsKennzeichen">Benutzer will die AvisePos schlie�en</param>
    public bool MaterialZugelagert(string tuName, IList<int> quanten, decimal menge, string weNum, string wePos, string taNum, string taPos, decimal gewicht, bool endQuittierungsKennzeichen)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Lagert Material innerhalb eines Tablares um
    /// </summary>
    /// <param name="neueTuPosen">ein RemoteEntityContainer mit der neuen Belegung</param>
    public void MaterialUmgelagert(RemoteEntityContainer neueTuPosen)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Die Materialmengen eines Tablares werden neu gesetzt
    /// </summary>
    /// <param name="tuPosen">ein RemoteEntityContainer mit der neuen Belegung</param>
    public bool InventurVerbuchen(RemoteEntityContainer tuPosen)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Transportiert ein Tablar vom Arbeitsplatz ins Lager.
    /// Der Arbeitsplatz wird in der UserSession �bergeben
    /// </summary>
    public void TransportiereTablarAb()
    {
		//TODO: need to be implement
    }


    /// <summary>
    /// Die Materialmenge wird korrigiert (Grund: entnahme aufgrund von G  ewichtsfehler oder/und Konturenfehler)
    /// </summary>
    /// <param name="entity">die TuPos, deren Menge korregiert werden soll</param>
    /// <param name="entnommeneMenge">Die neue Menge des Materials</param>
    public void MaterialMengenKorrektur(RemoteEntity entity, decimal entnommeneMenge)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Liefert einen RemoteEntityContainer mit allen TuPosen und der Menge die zu nicht abgeschlossenen AvisePositionen
    /// geh�ren. Die TuPosen befinden sich auf der Tu die am aktuellen Arbeitsplatz steht
    /// </summary>
    /// <returns></returns>
    public RemoteEntityContainer GetEntnehmbaresMaterial()
    {
		//TODO: need to be implement
		return null;
    }

    /// <summary>
    /// Liefert einen RemoteEntityContainer mit der Anzahl der freien Tablare gruppiert nach H�he
    /// </summary>
    /// <param name="einQuadrantFrei"></param>
    /// <param name="zweiQuadrantenFrei"></param>
    /// <param name="dreiQuadrantenFrei"></param>
    /// <param name="vierQuadrantenFrei"></param>
    /// <param name="hasRahmen"></param>
    /// <returns></returns>
    public RemoteEntityContainer GetFreieTablare(bool einQuadrantFrei, bool zweiQuadrantenFrei, bool dreiQuadrantenFrei, bool vierQuadrantenFrei, bool hasRahmen)
    {
      		//TODO: need to be implement
      		return null
    }



    #endregion

    #region Implementation of IAklBetriebsartWechselnPresenter

    public IDictionary<string, string> GetModus()
    {
		//TODO: need to be implement
		return null;
	}

    /// <summary>
    /// Liefert die aktuelle Betriebsart 
    /// </summary>
    /// <returns>
    /// 1 - Einlagerung
    /// 2 - Kommissionierung
    /// 3 - Inventur
    /// </returns>
    public decimal GetAktuelleBetriebsart()
    {
		//TODO: need to be implement
		return null;
    }

    /// <summary>
    /// Fordert die Betriebsart f�r diesen Arbeitsplatz an und setzt Sie zu gegebenem Zeitpunkt
    /// </summary>
    /// <param name="betriebsart">
    /// 1 - Einlagerung
    /// 2 - Kommissionierung
    /// 3 - Inventur
    /// </param>
    public void SetModus(decimal betriebsart)
    {
		//TODO: need to be implement
    }

    #endregion

    #region Implementation of IAklKommissionierungPresenter

    /// <summary>
    /// Legt einen neuen Beh�lter zur Kommissionierung an
    /// </summary>
    /// <param name="behaelterName">der Name des Beh�lters</param>
    public void KommissionierBehaelterAnlegen(string behaelterName)
    {
		//TODO: need to be implement
    }


    /// <summary>
    /// Liefert die EntnahmeDaten f�r die n�chste Entnahme. Wird keine Tu am Arbeitsplatz gefunden 
    /// so wird null zur�ckgegeben. Sind f�r die Tu am Arbeitsplatz keine 
    /// bereiten Entnahmen mehr vorhanden, wird das Property EntnahmeDaten.AnzahlKommissionierungen auf 0 gesetzt.
    /// </summary>
    /// <returns>die EntnahmeDaten</returns>
    public TablarUndEntnahmeDaten GetDaten()
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
    public bool EntnahmeBestaetigen(decimal entnahmeId, decimal entnahmeMenge, string komTuName)
    {
		//TODO: need to be implement
    }

    /// <summary>
    /// Liefert alle Tu-Positionen, die sich auf dem Kommissionierbeh�lter befinden
    /// </summary>
    /// <param name="tuName">Name des Kommissionierbeh�lters</param>
    /// <returns></returns>
    public RemoteEntityContainer GetKommissionierBehaelterDaten(string tuName)
    {
		//TODO: need to be implement
    }


    /// <summary>
    /// Liefert die Akl-Teilauftragspositionen inklusive Material zu einer Sap-Auftragsnummer.
    /// </summary>
    /// <param name="sapTaNum"></param>
    /// <returns>
    /// Die Akl-Teilauftragspositionen mit Material
    /// </returns>
    /// <remarks>
    /// Liefert null wenn keine Akl-Teilauftragspositionen gefunden werden
    /// </remarks>
    public RemoteEntityContainer GetAuftragDetails(string sapTaNum)
    {
		//TODO: need to be implement
    }

    #endregion

    #region IAklArbeitsplatzPresenter

    /// <summary>
    /// Fordert eine bestimme Tu an
    /// </summary>
    /// <param name="entity"></param>
    public void TablarAnfordern(RemoteEntity entity)
    {
		try
      {
        // Die Location zu Arbeitsplatz laden
        string arbeitsPlatzLocation = GetArbeitsplatzLocation();

        string tuName = (string)entity.GetValue("NAME");

        AklArbeitsplatzLogik.TablarAnfordern(tuName, arbeitsPlatzLocation);
      }
      catch (Exception excp)
      {
        _log.Info("Fehler bei TablarAnfordern", excp);
        throw ProcessException(excp);
      }
    }

    /// <summary>
    /// Es werden alle TUS mit einem bestimmten Material ausgelagert
    /// </summary>
    /// <param name="entity"></param>
    public void MaterialAnfordern(RemoteEntity entity)
    {
     //TODO: need to be implement
    }

    #endregion
    
       #region Hilfsfunktionen

    private string GetArbeitsplatzLocation()
    {
      string arbeitsPlatz = UserManagement.GetSessionDetail().LocationName;

      if (string.IsNullOrEmpty(arbeitsPlatz))
      {
        _log.Error("UserSession enth�lt keinen Arbeitsplatz");
        throw new UserManagementException("UserSession enth�lt keinen Arbeitsplatz", "LocationName");
      }
      return arbeitsPlatz;
    }

    private string GetTerminalName()
    {
      string terminalName = UserManagement.GetSessionDetail().TerminalName;

      if (string.IsNullOrEmpty(terminalName))
      {
        _log.Error("UserSession enth�lt keinen TerminalName");
        throw new UserManagementException("UserSession enth�lt keinen TerminalNmae", "TerminalName");
      }
      return terminalName;
    }


    #endregion
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************