//****************************************************************************
//  NAME: SgmFotTma.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgm.Base;
using Sgm.Base.SgmPhysik;
using Siemens.WarehouseManagement.Connectivity;
using Siemens.WarehouseManagement.Connectivity.Threading;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Configuration;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// Beauftragt Transporte bei der F�rdertechnik
  /// </summary>
  public class SgmFotTma : ITransportMediumAdapter, IAsyncSgmFotTma
  {
    #region Required Properties

    private IKopplung _kopplung;

    /// <summary>
    /// Gibt oder setzt die Kopplung
    /// </summary>
    [RequiredProperty]
    public IKopplung Kopplung
    {
      get { return _kopplung; }
      set
      {
        if (_kopplung != null)
        {
          Kopplung.RemoveReceiver<TeleInfo>(AsyncVerarbeiteTeleInfo);
        }

        _kopplung = value;

        if (_kopplung != null)
        {
          // Bei der Kopplung f�r die Verarbeitung von INFO && FRAG && LESE anmelden.
          // Start() wird von SILOC erst aufgerufen wenn das komplette Objekt-Netzwerk, dass durch spring.net
          // beschrieben ist verschaltet und alle RequiredProperties versorgt wurden.
          Kopplung.AddReceiver<TeleInfo>(AsyncVerarbeiteTeleInfo);
        }
      }
    }

    /// <summary>
    /// Gibt oder setzt die DataAccessRegistry
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    /// <summary>
    /// Gibt oder setzt den MessageStore
    /// </summary>
    [RequiredProperty]
    public IMessageAndExceptionHandling MessageStore { get; set; }

    /// <summary>
    /// Gets or sets the transport medium status observer.
    /// </summary>
    /// <value>The transport medium status observer.</value>
    [RequiredProperty]
    public ITransportMediumStatus TmaObserver { get; set; }

    /// <summary>
    /// Gets or sets the coordinate directory.
    /// </summary>
    /// <value>The coordinate directory.</value>
    [RequiredProperty]
    public ICoordinateDirectory CoordinateDirectory { get; set; }

    /// <summary>
    /// Gets or sets the physic
    /// </summary>
    [RequiredProperty]
    public ISgmPhysik Physik { get; set; }

    /// <summary>
    /// Gets or sets the TransportKoordinierung
    /// </summary>
    [RequiredProperty]
    public ITransportKoordinierung TransportKoordinierung { get; set; }

    /// <summary>
    /// Gets or sets the Async Proxy to self
    /// </summary>
    [RequiredProperty]
    public IAsyncSgmFotTma FotTmaProxy { get; set; }

    /// <summary>
    /// Gets or sets the telegram thread blocker.
    /// </summary>
    /// <value>
    /// The telegram thread blocker.
    /// </value>
    [RequiredProperty]
    public ITelegramThreadBlocker TelegramThreadBlocker { get; set; }


    #endregion

    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }
    /// <summary>
    /// Define a static logger variable
    /// </summary>
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private object locker = new object();

    #region Konstanten

    private const int _constKonturVorderkante = 1;
    private const int _constKonturHinterkante = 2;
    private const int _constKonturBreite = 4;
    private const int _constKonturHoehe = 8;
    private const int _constUebergewicht = 16;

    #endregion

    /// <summary>
    /// Requests a partial transport.
    /// </summary>
    /// <param name="transporteeName">Der Name der Transporteinheit</param>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="moveType">The movetype of this transport.</param>
    /// <param name="additionalData">Some optional additional data that was provided by the solution.</param>
    //[AsyncCall]
    public void RequestPartialTransport(string transporteeName, Coordinate source, Coordinate target, MoveType moveType, IDictionary<string, string> additionalData)
    {
      
    }

    public void CancelPartialTransport(string transporteeName, IDictionary<string, string> additionalData)
    {
      throw new TmaCancelationNotPossibleException(transporteeName);
    }

    public void TransportToNio(Coordinate source)
    {
      throw new System.NotImplementedException();
    }

    public void TransportAborted(string transporteeName, IDictionary<string, string> additionalData)
    {
      log.DebugFormat("SgmFotTma.TransportAborted Tu [{0}]", transporteeName);
    }

    #region Verarbeite Telegramme

    /// <summary>
    /// Verarbeitet eingehende INFO Telegramme
    /// </summary>
    /// <param name="telegram"></param>
    public void VerarbeiteTeleInfo(Telegram telegram)
    {
     
    }

    #endregion
  }

  /// <summary>
  /// Wird von der Kopplung aufgerufen und f�hrt den Aufruf im AO-Thread aus
  /// </summary>
  public interface IAsyncSgmFotTma
  {
    ///<summary>
    /// Verarbeitet die eingehenden Info-Telegramme der Kopplung
    ///</summary>
    ///<param name="tele"></param>
    void VerarbeiteTeleInfo(Telegram tele);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
