//******************************************************************************
// NAME: IAklLeerbehaelterBestellung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.GenericPresenter;

namespace Sgm.Akl.LeerbehaelterAuftraege
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAklLeerbehaelterBestellung
  {

    /// <summary>
    /// Fordert ein Leertablar zur Auslagerung zu dem entsprechenden Arbeitsplatz an.
    /// Wenn kein Tablar gefunden wird, wird ein Fehler geworfen
    /// </summary>
    /// <param name="arbeitsplatz">
    /// Der Name des Arbeitsplatzes 
    /// </param>
    /// <param name="anzQuadranten">Anzahl der ben�tigten leeren Quadranten auf dem Tablar</param>
    /// <param name="hoehe">Die Tablar Hoehe 2,3 oder 4</param>
    /// <param name="hasRahmen">Mit Rahmen?</param>
    /// <returns>Die Tablarnummer</returns>
    string LeerTablarAuslagern(string arbeitsplatz, int anzQuadranten, int hoehe, bool hasRahmen);

    /// <summary>
    /// L�scht einen LeerbehaelterAuftrag, den zugeh�rigen Teilauftrag, die zugeh�rige Einlagerreservierung, den zugh�rigen TordRequest 
    /// wenn noch kein Tord angelegt wurde.
    /// </summary>
    /// <param name="leerbehaelterAuftrag"></param>
    void DeleteNichtAktiviertenLeerbehaelterAuftrag(LeerbehaelterAuftrag leerbehaelterAuftrag);

    /// <summary>
    /// Liefert Query f�r Tablare zur Einlagerung
    /// </summary>
    /// <param name="hoehe"></param>
    /// <param name="hasRahmen"></param>
    /// <returns></returns>
    Query GetEinlagerTablarQuery(int hoehe, bool hasRahmen);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
