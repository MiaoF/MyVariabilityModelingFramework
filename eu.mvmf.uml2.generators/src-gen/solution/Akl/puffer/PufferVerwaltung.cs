//****************************************************************************
//  NAME: PufferVerwaltung.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgm.Akl.AutomatikTransporte;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Siemens.WarehouseManagement.Validation;
using Sgm.Base.SgmPhysik;
using Sgm.Base.Auftraege;
using Sgm.Base; 

namespace Sgm.Akl.Puffer
{
  /// <summary>
  /// 
  /// </summary>
  public class PufferVerwaltung : Zyklisch, IAutomatikTransportObserver, IPufferVerwaltung
  {
    /// <summary>
    /// Define a static logger variable
    /// </summary>
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private IAutomatikTransport _automatikTransport;

    /// <summary>
    /// Zugriff auf die Transportsteuerung des AKL.
    /// Beim setzten wird die PufferVerwaltung als Observer bei
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

    /// <summary>
    /// Zugriff auf die Akl-Pufferplatz Suche
    /// </summary>
    [RequiredProperty]
    public IPufferPlatzSuche PufferPlatzSuche { get; set; }

    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    /// <summary>
    /// Zugriff auf die SgmPhysik
    /// </summary>
    [RequiredProperty]
    public ISgmPhysik SgmPhysik { get; set; }

    /// <summary>
    /// Zugriff auf das ReservationLockSystem
    /// </summary>
    [RequiredProperty]
    public IReservationModification ReservationModification { get; set; }

    /// <summary>
    /// Zugrff auf die TeilauftragVerwaltung
    /// </summary>
    [RequiredProperty]
    public IAklLagerteil AklLagerteil { get; set; }

    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    /// <summary>
    /// Wird verwendet, um den Timer zu stoppen
    /// </summary>
    public PufferVerwaltung()
    {
      //Timer beenden
      ZyklusZeit = 0;
    }

    private void TuPuffern(SgmTu transportee, IDictionary<string, string> additionalPlcData)
    {
      //�berpr�ft, ob Reservierungen f�r die Tu vorliegen
      IList<Reservation> reservierungen = ReservationModification.GetReservations(transportee);

      if (reservierungen.Count != 0)
      {
        _log.WarnFormat("Es liegt bereits eine Reservierung f�r Tu [{0}] vor.", transportee.Name);
        return;
      }

      SgmLocation heberPlatz = (SgmLocation)SgmPhysik.GetLocationByTuName(transportee.Name);

      SgmLocation pufferPlatz = PufferPlatzSuche.ErmittleFreienPufferPlatz((SgmTu)transportee);

      //Wenn kein Pufferplatz gefunden wurde, wird in der zyklischen Funktion versucht einen Platz zu finden
      if (pufferPlatz == null)
      {
        _log.ErrorFormat("Kein freier Platz im Pufferbereich f�r Tu [{0}] gefunden.", transportee.Name);
        return;
      }
      MoveType moveType = DataAccess.SelectFirst<MoveType>(MoveType.Properties.No.Filter(3));

      //Transportiert die Tu zum Pufferplatz
      AutomatikTransport.Transportiere(transportee, heberPlatz, pufferPlatz, moveType, false, additionalPlcData);
    }


      /// <summary>
    /// �berpr�ft zyklisch, ob eine Tu am ScannerPlatz des Pufferbereichs steht. Dies ist die Notfallstrategie,
    /// falls kein Pufferplatz gefunden werden konnte. Der Timer wird nur gestartet, wenn kein Pufferplatz gefunden wurde
    /// </summary>
    public override void ZyklischeAktion()
    {
      IList<SgmTu> tus = DataAccess.SelectAll<SgmTu>(SgmTu.Properties.LocName.Filter("1330"));

      if (tus.Count == 0)
      {
        _log.DebugFormat("Zyklische �berpr�fung auf Tus am Scannerplatz ergab keinen Treffer");
        return;
      }

      foreach (SgmTu tu in tus)
      {
        //�berpr�ft, ob reservierungen f�r die Tu vorliegen
        IList<Reservation> reservierungen = ReservationModification.GetReservations(tu);

        //Wenn keine Reservierungen vorliegen kann die Tu gepuffert werden
        if (reservierungen.Count == 0)
        {
          TuAmPufferHeber(tu, null);
        }
      }
    }

    #region IAutomatikTransportObserver

    /// <summary>
    /// Wird aufgerufen, wenn sich eine Tu am Scanner im Pufferbereich gemeldet hat.
    /// Es wird ein Pufferplatz gesucht und dort abgestellt.
    /// </summary>
    /// <param name="tu">die Tu</param>
    /// <param name="additionalPlcData"></param>
    public void TuAmPufferHeber(SgmTu tu, IDictionary<string, string> additionalPlcData)
    {
      if (tu == null) throw new ArgumentNullException("tu");

      _log.InfoFormat("Tu [{0}] hat sich im Puffer angemeldet", tu.Name);

      Teilauftrag teilauftrag = AklLagerteil.GetTeilauftragZuTu(tu);

      if (teilauftrag == null)
      {
        _log.WarnFormat("TuAmPufferHeber(): Kein Auftrag f�r Tu [{0}] gefunden.", tu.Name);
        return;
      }
      // Ein normaler SAP-Auftrag muss gepuffert werden
      if (teilauftrag.Typ.Equals(Teilauftrag.TypValue.Auftrag))
      {
        _log.InfoFormat("TuAmPufferHeber(): Tu [{0}] ist f�r Auftrag [{1}] Teilauftrag [{2}] unterwegs und wird gepuffert.", tu.Name,
                       teilauftrag.AufId, teilauftrag.Id);
        TuPuffern(tu, additionalPlcData);
      }

    }

    public void TuAmArbeitsplatz(SgmTu tu, SgmLocation location)
    {
    }

    public void TransportAbgeschlossen(SgmTu tu, TransportOrder tord)
    {
    }

    public void TuAmZusammenfuehrPlatz(SgmTu tu)
    {
      if (tu == null) throw new ArgumentNullException("tu");

      // Informiert das AklLagerteil �ber die Zusammenf�hrung der Tu
      AklLagerteil.TuZusammengefuehrt(tu);
    }

    public void TuGepuffert(SgmTu tu)
    {
      if (tu == null)
      {
        throw new ArgumentNullException("tu");
      }
      _log.InfoFormat("Tu [{0}] wurde erfolgreich auf Location [{1}] gepuffert", tu.Name, tu.LocName);
      AklLagerteil.TuBereitgestellt(tu);
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

    #region Implementation of IPufferVerwaltung

    /// <summary>
    /// Ruft alle Tus zum �bergebenen Teilauftrag zum Zusammenf�hrplatz ab.
    /// </summary>
    /// <param name="kommissionierBox">die AuftragsId des Teilauftrags</param>
    public void TeilauftragAbrufen(SgmTu kommissionierBox)
    {
      //SgmLocation ausschleusPlatz = (SgmLocation)SgmPhysik.GetLocationByName(ConstDirectory.LOC_NAME_1360);
      
      SgmLocation ausschleusPlatz = (SgmLocation)SgmPhysik.GetLocationByName(1350);
	  
      MoveType moveType = DataAccess.SelectFirst<MoveType>(MoveType.Properties.No.Filter(ConstDirectory.MOVE_TYPE_NO_AUSLAGERUNG));

      SgmLocation standort = (SgmLocation)SgmPhysik.GetLocationByTuName(kommissionierBox.Name);

      _log.InfoFormat("Transportiere Tu [{0}] zu Auftrag [{1}] zum AusschleusPlatz [{2}]", kommissionierBox.Name, kommissionierBox, ausschleusPlatz.Name);

      if (AutomatikTransport.Transportiere(kommissionierBox, standort, ausschleusPlatz, moveType, false, null))
      {
        //Informiert das Lagerteil �ber den Abruf der Tu
        AklLagerteil.TuAbgerufen(kommissionierBox);
      }
    }

    #endregion
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
