    
//******************************************************************************
// NAME: IStatistik.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Base.Statistik
{
  /// <summary>
  /// 
  /// </summary>
  public interface IStatistik
  {
    /// <summary>
    /// Diese Methode wird aufgerufen wenn ein Auftrag von SAP an das LVS gesendet wurde
    /// </summary>
    /// <param name="auftragId">die Id des Auftrags</param>
    /// <param name="sapBewegungsSchluessel">der Bewegungsschl�ssel und (NBV)</param>
    void AuftragBegonnen(decimal auftragId, string sapBewegungsSchluessel);

    ///<summary>
    /// Diese Methode wird aufgerufen, wenn ein Auftrag in allen Lagerteilen abgeschlossen wurde und in den Status
    /// "FERTIG" gesetzt wird
    ///</summary>
    ///<param name="auftragId">die Id des Auftrags</param>
    ///<param name="sapBewegungsSchluessel">der Bewegungsschl�ssel und (NBV)</param>
    void AuftragBeendet(decimal auftragId, string sapBewegungsSchluessel);

    ///<summary>
    /// Diese Methode wird aufgerufen, wenn eine Entnahme get�tigt wurde
    ///</summary>
    ///<param name="teilauftrag">der Teilauftrag</param>
    ///<param name="matId">die Sachnummer die entnommen wurde</param>
    ///<param name="menge">die Menge die entnommen wurde</param>
    ///<param name="bestandsart"></param>
    ///<param name="sonderBestandsNummer"></param>
    void EntnahmeGetaetigt(Teilauftrag teilauftrag, decimal matId, decimal menge, SgmTuPos.BestandsartValue bestandsart, string sonderBestandsNummer);

    /// <summary>
    /// Diese Methode wird aufgerufen, wenn ein Teilauftrag begonnen wurde
    /// </summary>
    /// <param name="lagerteil"></param>
    /// <param name="bewegungsart"></param>
    void TeilauftragBeendet(Teilauftrag.LagerTeilValue lagerteil, string bewegungsart);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************