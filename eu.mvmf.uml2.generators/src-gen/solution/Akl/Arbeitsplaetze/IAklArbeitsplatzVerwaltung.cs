//******************************************************************************
//****************************************************************************
//  NAME: IAklArbeitsplatzVerwaltung.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Infrastructure.Configuration;

namespace Sgm.Akl.Arbeitsplaetze
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAklArbeitsplatzVerwaltung
  {
    /// <summary>
    /// Ermittelt den Arbeitsplatz an Hand der Location
    /// </summary>
    /// <param name="locName"></param>
    /// <returns></returns>
    [Synchronous]
    Arbeitsplatz GetArbeitsplatzByLocation(string locName);

    /// <summary>
    /// Liefert alle Arbeitsplätze im übergebenen Modus
    /// </summary>
    /// <param name="modus">ein Arbeitsplatzmodus</param>
    /// <returns>Liste aller Arbeitsplätze im Modus</returns>
    [Synchronous]
    IList<Arbeitsplatz> AlleArbeitsplaetzeInModus(Arbeitsplatz.ModusValue modus);

    /// <summary>
    /// Liefert eine Liste aller Arbeitsplätze die im Modus Kommissionierung sind und keine Auftrag zugewiesen haben
    /// </summary>
    /// <returns>Liste mit Arbeitsplaetzen</returns>
    [Synchronous]
    IList<Arbeitsplatz> ArbeitsplaetzeBereitZurKommissionierung();

    /// <summary>
    /// Setzt den angeforderten Modus des Arbeitsplatzes und startet die Überprüfung ob der Modus auch gesetzt werden kann.
    /// </summary>
    /// <returns></returns>
    void SetArbeitsplatzModusAngefordert(string arbeitsplatzLocation, Arbeitsplatz.ModusAngefordertValue neuerModus);

    /// <summary>
    /// Setzt den Arbeitsplatz Modus ohne Überprüfung. Diese Methode sollte nur von der AklArbeitsplatzCheckModusWechsel aufgerufen werden. 
    /// </summary>
    /// <param name="arbeitsplatzLocation"></param>
    /// <param name="neuerModus"></param>
    void SetArbeitsplatzModus(string arbeitsplatzLocation, Arbeitsplatz.ModusValue neuerModus);

    /// <summary>
    /// Prüft ob der Modus und der angeforderte Modus mit dem Übergeben übereinstimmt.
    /// </summary>
    /// <param name="arbeitsplatz">Der Arbeitsplatz der zu überprüfen ist</param>
    /// <param name="modus">Der Modus auf den Überprüft wird</param>
    /// <returns></returns>
    [Synchronous]
    bool IsArbeitsplatzInModus(Arbeitsplatz arbeitsplatz, Arbeitsplatz.ModusValue modus);
   
    /// <summary>
    /// Liefert die Anzahl freier Plätze, die der Puffer vor dem Arbeitsplatz hat
    /// </summary>
    /// <param name="arbeitsplatz"></param>
    /// <returns></returns>
    [Synchronous]
    int GetFreieArbeitsplatzKapazitaet(Arbeitsplatz arbeitsplatz);

    ///// <summary>
    ///// Liefert den Teilauftrag, der dem Übergebenen Arbeitsplatz zugeordnet ist
    ///// </summary>
    ///// <param name="tuName">der Name der Tu</param>
    ///// <returns>den Teilfauftrag</returns>
    //Teilauftrag GetAktivenTeilauftrag(string tuName);

    ///// <summary>
    ///// Liefert den Auftrag, dessen Teilauftrag aktuell dem Arbeitsplatz zugeordnet ist
    ///// </summary>
    ///// <param name="tuName">der Name der Tu</param>
    ///// <returns>der Auftrag</returns>
    //Auftrag GetAktivenAuftrag(string tuName);

    /// <summary>
    /// Liefert den Arbeitsplatz
    /// </summary>
    /// <param name="arbeitsplatzName"></param>
    [Synchronous]
    Arbeitsplatz GetArbeitsplatzByName(string arbeitsplatzName);

    /// <summary>
    /// Liefert den Arbeitsplatz
    /// </summary>
    /// <param name="arbeitsplatzId"></param>
    [Synchronous]
    Arbeitsplatz GetArbeitsplatzById(decimal arbeitsplatzId);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
