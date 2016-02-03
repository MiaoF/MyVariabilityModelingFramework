//****************************************************************************
//  NAME: SgmRbg.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgm.Base;
using Siemens.WarehouseManagement.Connectivity;
using Siemens.WarehouseManagement.Connectivity.Threading;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.TransportManagement.Physics;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Validation;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// 
  /// </summary>
  public class SgmRbgTma : AbstractRbgTma, IAsyncSgmRbgTma
  {
    // Define a static logger variable
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private Dictionary<string, GassenDaten> _gassenStatusCache = new Dictionary<string, GassenDaten>();

    #region Required Properties

    private IKopplung _kopplung;

    /// <summary>
    /// Gets or sets the Kopplung.
    /// </summary>
    /// <value>The Kopplung.</value>
    [RequiredProperty]
    public IKopplung Kopplung
    {
      get { return _kopplung; }
      set
      {
        if (_kopplung != null)
        {
          Kopplung.RemoveReceiver<TeleTaqt>(AsyncProcessTeleTaqt);
          Kopplung.RemoveReceiver<TeleLazu>(AsyncProcessTeleLazu);
        }

        _kopplung = value;

        if (_kopplung != null)
        {
          //Bei der Kopplung für den Emfpang von Taqt und Lazu Telegrammen anmelden
          Kopplung.AddReceiver<TeleTaqt>(AsyncProcessTeleTaqt);
          Kopplung.AddReceiver<TeleLazu>(AsyncProcessTeleLazu);
        }
      }
    }

    /// <summary>
    /// Gets or sets the Async Proxy to self
    /// </summary>
    [RequiredProperty]
    public IAsyncSgmRbgTma RbgTmaProxy { get; set; }

    /// <summary>
    /// Zugriff auf das ReservationLockSystem
    /// </summary>
    [RequiredProperty]
    public IReservationModification ReservationModification { get; set; }

    /// <summary>
    /// Gets or sets the telegram thread blocker.
    /// </summary>
    /// <value>
    /// The telegram thread blocker.
    /// </value>
    [RequiredProperty]
    public ITelegramThreadBlocker TelegramThreadBlocker { get; set; }

    #endregion

    #region Overrides

    /// <summary>
    /// Diese Methode prüft ob der übergebene Transportauftrag gefahren werden kann.
    /// </summary>
    /// <param name="tord">Der zu prüfende Transportauftrag.</param>
    /// <param name="sourceConstraintName">Der Name der Quell Prüfstrategie.</param>
    /// <param name="targetConstraintName">Der Name der Ziel Prüfstrategie.</param>
    /// <returns>
    /// 	<c>true</c> wenn der Transportauftrag gefahren werden kann, sonst false
    /// </returns>
    protected override bool TordCompliesWithConstraints(RbgTord tord, string sourceConstraintName, string targetConstraintName)
    {
      
    }

    /// <summary>
    /// Dieser Transportauftrag wurde zugeteilt und muss an das RBG übertragen werden.
    /// </summary>
    /// <param name="tord">Der Transportauftrag.</param>
    protected override void SendTordToRbg(RbgTord tord)
    {
     
    }

    /// <summary>
    /// Transports everything on source to the next NIO target.
    /// </summary>
    /// <remarks>This Method is called by the TransportControl if 
    /// a read error occured and the TransportControl doesn't know the
    /// Tu Name. It is assumed that the TMA or the PLC can just free the
    /// source coordinate from any tus left there.</remarks>
    /// <param name="source">The source Coordinate to set free.</param>
    //[AsyncCall]
    public override void TransportToNio(Coordinate source)
    {
      
    }

    protected override IEnumerable<RbgTord> ReorderTordsForMovement(IOrderedEnumerable<RbgTord> orderedTords, RbgMovement movement)
    {
      
    }

    private decimal GetAnzahlNichtZugeteilteRbgTordsFuerKommplatz(IList<Tord> tords, string locationName)
    {
      
    }

    /// <summary>
    /// Diese Methode sollte von einem Lösungs RBG Tma überschrieben werden
    /// um zusätzliche lösungsspezifische Daten des Tord zu verarbeiten
    /// bzw. zu speichern.
    /// </summary>
    /// <param name="tord">Der RBG Transportauftrag.</param>
    /// <param name="tuName">Der Name der Tu</param>
    /// <param name="moveType">Type of the move.</param>
    /// <param name="data">Zusätzliche Daten.</param>
    /// <remarks>
    /// Der Lösungs TMA kann Daten entweder in einer eigenen Tabelle ablegen oder an den
    /// tord anhängen. Der Tord wird erst nach Aufruf dieser Methode in der Datenbank abgelegt
    /// </remarks>
    protected override void HandleAdditionalData(RbgTord tord, string tuName, MoveType moveType, IDictionary<string, string> data)
    {
     
    }

    protected override decimal GetKorrekturMoveType()
    {
      return 40;
    }

    #endregion

    #region Process Telegram


    /// <summary>
    /// Holt alle gemeldeten Zustände aus dem Lazu Tele und speichert sie sowohl im RAM als auch in der
    /// Datenbank. Das ist ein oneway Zugriff! Zustände werde nicht wieder aus der Datenbank gelesen.
    /// </summary>
    /// <param name="aTele">The tele.</param>
    public void ProcessTeleLazu(Telegram aTele)
    {

    }


    public void ProcessTeleTaqt(Telegram tele)
    {
    
    }

    #endregion
   

    /// <summary>
    /// Holt aus dem GassenStatus Cache alle aktiven RBGs und gibt sie zurück
    /// </summary>
    /// <returns></returns>
    public IEnumerable<int> GetActiveRbgs()
    {
      var activeRbgs = from gd in _gassenStatusCache
                       where gd.Value.Rbg.Status == RbgStatus.StatusValue.Ready ||
                             gd.Value.Rbg.Status == RbgStatus.StatusValue.Busy
                       select int.Parse(gd.Value.Rbg.Name.Substring(5, 1));
      return activeRbgs;
    }

    public RbgTord GetRbgTordFor(string name)
    {
      return GetAssignedTordForRbg(name);
    }
  }

  public interface IAsyncSgmRbgTma
  {
    void ProcessTeleLazu(Telegram tele);
    void ProcessTeleTaqt(Telegram tele);
  }

  public class GassenDaten
  {
   
  }

  public enum PlatzStatus
  {
    Unbekannt = -1,
    Frei = 0,
    Belegt = 1,
    HandFrei = 2,
    HandBelegt = 3,
    Gestoert = 4,
  }

}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
