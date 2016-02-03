//******************************************************************************
// NAME: MiscTma.cs
//******************************************************************************
//
// Description:
//
//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using Siemens.WarehouseManagement.Configuration;
using Siemens.WarehouseManagement.Connectivity;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.Infrastructure.Configuration;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// 
  /// </summary>
  public class MiscTma : ILifecycle, IMiscTma
  {
    // Define a static logger variable
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    // Ein Timer für das zyklische versenden eines KEAL
    private Timer _timer = new Timer { Enabled = false, Interval = 60000, AutoReset = true };
    object _locker = new object();
    private bool _sending;
   
    #region Required Properties

    /// <summary>
    /// Gets or sets the message store.
    /// </summary>
    /// <value>The message store.</value>
    [RequiredProperty]
    public IMessageAndExceptionHandling MessageStore
    {
      get;
      set;
    }

    private IKopplung _kopplung;

    /// <summary>
    /// Gets or sets the tele sender.
    /// </summary>
    /// <value>The tele sender.</value>
    [RequiredProperty]
    public IKopplung Kopplung
    {
      get { return _kopplung; }
      set
      {
        if (_kopplung != null)
        {
          Kopplung.RemoveReceiver<TeleKeal>(VerarbeiteTeleKeal);
          Kopplung.RemoveReceiver<TeleMeld>(VerarbeiteTeleMeld);
          Kopplung.RemoveReceiver<TeleLogb>(VerarbeiteTeleLogb);
        }

        _kopplung = value;

        if (_kopplung != null)
        {
          Kopplung.AddReceiver<TeleKeal>(VerarbeiteTeleKeal);
          Kopplung.AddReceiver<TeleMeld>(VerarbeiteTeleMeld);
          Kopplung.AddReceiver<TeleLogb>(VerarbeiteTeleLogb);
        }
      }
    }

    #endregion

    private Dictionary<string, DateTime> _datiLetztesKeal = new Dictionary<string, DateTime>();

    public MiscTma()
    {
      StartPriority = 50;
    }

    /// <summary>
    /// Wird zyklisch über den Timer aufgerufen und versucht ein Keal an alle Konfigurierten Verbindungspartner zu senden.
    /// Dabei wird als Dialogpunkt der RemoteName des Verbindungspartners genommen, da es momentan noch kein
    /// TODO Default Remote Punkt für eine Verbindung
    /// gibt.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
    private void SendTeleKeal(object sender, ElapsedEventArgs e)
    {
      lock (_locker)
      {
        if (_sending)
        {
          return;
        }
        
        _sending = true;
      }

      try
      {
        IList<TelePlcConnection> connections = Kopplung.GetConnections();

        foreach (TelePlcConnection con in connections)
        {
          // NOTE: als Dialogpunkt wird hier der RemoteName verwendet, da es momentan kein Konzept eines "default" Dialogpunktes gibt.
          SendTeleKeal(con.LocalName, con.RemoteName, con.RemoteName);
        }
      }
      finally
      {
        _sending = false;
      }
    }

    /// <summary>
    /// Verschickt eine Keal.
    /// </summary>
    /// <param name="localName">Absender Name.</param>
    /// <param name="remoteName">Empfaenger Name.</param>
    /// <param name="dialogPunkt">DialogPunkt.</param>
    private void SendTeleKeal(string localName, string remoteName, string dialogPunkt)
    {
      TeleKeal t = Kopplung.CreateTelegram<TeleKeal>();
      log.InfoFormat("KEAL empfangen");
      t.EnsureTransmission = Telegram.EnsureTransmissionValue.False;
      t.Datum = DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss", CultureInfo.InvariantCulture);
      t.DialogPunkt = dialogPunkt;
      t.Receiver = remoteName;
      t.Sender = localName;
      Kopplung.Send(t);
    }

    /// <summary>
    /// Verarbeitet das KEAL Telegram.
    /// </summary>
    /// <param name="telegram">The telegram.</param>
    public void VerarbeiteTeleKeal(TeleKeal telegram)
    {
      _datiLetztesKeal[telegram.Sender] = DateTime.Now;
      SendTeleKeal(telegram.Receiver, telegram.Sender, telegram.DialogPunkt);
    }

    public void VerarbeiteTeleLogb(TeleLogb logb)
    {
      if (logb.RetryCount == 0)
      {
        SpsMeldungInMessageStoreSpeichern(logb.Station, logb.MeldungsNr, logb.MeldungsText);
      }
      Kopplung.TelegramConsumedImmediate(logb);
    }

    public void VerarbeiteTeleMeld(TeleMeld meld)
    {
      if (meld.RetryCount == 0)
      {
        SpsMeldungInMessageStoreSpeichern(meld.Station, meld.MeldungsNr, meld.MeldungsText);
      }
      Kopplung.TelegramConsumedImmediate(meld);
    }

    private void SpsMeldungInMessageStoreSpeichern(string station, decimal meldungNr, string meldungsText)
    {
      string text = string.Format("Sps Station [{0}] Meldung Nr [{1}] Text [{2}]", station, meldungNr, meldungsText);
      log.Warn(text);
      MessageStore.ProcessExceptionOrMessage(new Exception(text));
    }

    #region Implementation of ILifecycle

    /// <summary>
    /// Will be called when the Application/Service starts
    /// </summary>
    public void Start()
    {
      // Initial ein Keal senden, damit die Steuerung sofort mitbekommt, dass der Rechner wieder da ist und evtl. die Verbindung von Seiten der Steuerung "aufgemacht" wird.
      SendTeleKeal(null, null);
      _timer.Elapsed += SendTeleKeal;
      _timer.Start();
    }


    /// <summary>
    /// Will be called when the Application/Service stops
    /// </summary>
    /// <remarks>
    /// All resources hold by the component should be released. 
    /// Timers should be stopped.
    /// </remarks>
    public void Stop()
    {
      _timer.Stop();
    }

    /// <summary>
    /// Gets or sets the start priority.
    /// </summary>
    /// <remarks>
    /// A Component returning a lower value will be started first. If two components 
    /// return the same value the start order will not be deterministic.
    /// 
    /// Stop() will be issued in reverse order
    /// </remarks>
    /// <value>The start priority.</value>
    public int StartPriority
    {
      get;
      set;
    }

    #endregion

    #region Implementation of IMiscTma

    public bool IsSteuerungAlive(string name)
    {
      if (_datiLetztesKeal.ContainsKey(name) == false)
      {
        return false;
      }

      DateTime letztesKeal = _datiLetztesKeal[name];
      if (DateTime.Now - letztesKeal > TimeSpan.FromMinutes(2))
      {
        return false;
      }

      return true;
    }

    public DateTime GetTimeOfLastKeal(string name)
    {
      if (_datiLetztesKeal.ContainsKey(name) == false)
      {
        return DateTime.MinValue;
      }

      return _datiLetztesKeal[name];
    }

    #endregion
  }

  public interface IMiscTma
  {
    [Synchronous]
    bool IsSteuerungAlive(string name);
    
    [Synchronous]
    DateTime GetTimeOfLastKeal(string name);
  }
}

//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************

