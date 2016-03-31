//******************************************************************************
// NAME: AutomatikTransport.cs
// This is a generated class! Do not modify! 
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sgm.Base.SgmPhysik;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.Infrastructure.Configuration;
using Siemens.WarehouseManagement.MaterialManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics.Queries;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Validation;
using Siemens.WarehouseManagement.TransportManagement;
using Sgm.Akl.Puffer;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Sgm.Akl.AuftraegeBearbeitung;
using Siemens.WarehouseManagement.Sgm.Base;
using Sgm.Base;

namespace Sgm.Akl.AutomatikTransporte
{
  /// <summary>
  /// Leitet die erstellten und aktivierten Transporte an die Kopplung
  /// </summary>
  public class AutomatikTransport : Observable<IAutomatikTransportObserver>, IAutomatikTransport, ITransportOrderStatus, ITransportStatus, ITransportKoordinierung
  {
    //Logger
    private static log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Required Properties

    /// <summary>
    /// Gets or sets the storage bin search.
    /// </summary>
    /// <value>The storage bin search.</value>
    [RequiredProperty]
    public IStorageBinSearch StorageBinSearch { get; set; }

    private ITransportControl _realeTransportKontrolle;
    /// <summary>
    /// Zugriff auf die Reale TransportKontrolle der Plattform
    /// </summary>
    [RequiredProperty]
    public ITransportControl RealeTransportKontrolle
    {
      get
      {
        return _realeTransportKontrolle;
      }
      set
      {
        if (_realeTransportKontrolle != null)
        {
          _realeTransportKontrolle.RemoveTordStatusListener(this);
          _realeTransportKontrolle.RemoveTransportObserver(this);
        }

        _realeTransportKontrolle = value;

        if (_realeTransportKontrolle != null)
        {
          _realeTransportKontrolle.AddTordStatusListener(this);
          _realeTransportKontrolle.AddTransportObserver(this);
        }
      }
    }

    /// <summary>
    /// Zugriff auf das LockSystem
    /// </summary>
    [RequiredProperty]
    public ILockSystem SperrenSystem { get; set; }

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
    /// Zugriff auf den MessageStore
    /// </summary>
    [RequiredProperty]
    public IMessageAndExceptionHandling MessageStore { get; set; }

    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    #endregion

    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    private bool _transportAktiviert;
    private string _transportSyncGuid;

    /// <summary>
    /// Transportiert eine Tu von der Quelle zum Ziel
    /// </summary>
    /// <param name="tu">Die Tu</param>
    /// <param name="quelle">Die Quelle</param>
    /// <param name="ziel">Das Ziel</param>
    /// <param name="moveType">The reason for the request to move Tu.</param>
    /// <param name="informImmediatelyOnInterrupt">if set to <c>true</c> an event will be raised immediately if the transport is interrupted due to physic state.</param>
    /// <param name="additionalData">Additional Data that will be passed on to the Transport Medium Adapters</param>
    public bool Transportiere(SgmTu tu, SgmLocation quelle, SgmLocation ziel, MoveType moveType, bool informImmediatelyOnInterrupt, IDictionary<string, string> additionalData)
    {
      _transportAktiviert = false;
      _transportSyncGuid = Guid.NewGuid().ToString();
      RealeTransportKontrolle.Transport(tu, quelle, ziel, moveType, informImmediatelyOnInterrupt, additionalData,
                                        _transportSyncGuid);
      if (_transportAktiviert)
      {
        return true;
      }
      return false;
    }


    #region Implementierung von ITransportOrderStatus

    public void TransportAccepted(TransportOrder tord, string token)
    {
      if (string.CompareOrdinal(token, _transportSyncGuid) == 0)
      {
        _transportAktiviert = true;
      }

      SgmLocation quelle = (SgmLocation)SgmPhysik.GetLocationByName(tord.LocationNameFrom);

      // Wenn es sich um eine Auslagerung aus einem Fach handelt, wird eine Einlagersperre gesetzt,
      // bis der Tord in irgend einer Weise beendet wird
      if (quelle.Type.Equals(SgmLocation.TypeValue.BinPosition))
      {
        // Fach sperren
        LockReason grund = SperrenSystem.GetNamedLockReason(ConstDirectory.LOR_AUTOMATIC_IN);

        SperrenSystem.SetLock(quelle, grund);
      }
    }

    public void TransportException(string text, string token, Exception ex)
    {
      if (string.CompareOrdinal(token, _transportSyncGuid) == 0)
      {
        _transportAktiviert = false;
        _log.WarnFormat("Transportaktivierung fehlgeschlagen. Text: [{0}] Exception: [{1}]", text, ex);

      }
    }

    public void TransportCanceled(TransportOrder tord, CancelationReason reason, Tu transportee, string token)
    {
      if (transportee != null)
      {
        _log.InfoFormat("Lösche PreReservation für Tu [{0}]", transportee.Name);

        ReservationModification.DeletePrereservation(transportee);
      }

      VersucheEinlagerSperreZuLoeschen(tord);
    }



    public void CancelationNotPossible(TransportOrder tord, Tu transportee, string token)
    {
    }

    public void TransportAborted(TransportOrder tord, Tu transportee, string token)
    {
      if (transportee != null)
      {
        _log.InfoFormat("Lösche PreReservation für Tu [{0}]", transportee.Name);

        ReservationModification.DeletePrereservation(transportee);
      }
      VersucheEinlagerSperreZuLoeschen(tord);
    }

    public void TransportTmaTimeout(TransportOrder tord, Tu transportee, Location transportMedium)
    {
    }

    public void TransportConfirmationCompleted(TransportOrder tord, Tu transportee, string token)
    {
      VersucheEinlagerSperreZuLoeschen(tord);
    }

    /// <summary>
    /// Wird aufgerufen wenn die TransportControl einen Transport abgeschlossen hat
    /// </summary>
    /// <param name="tord">der erledigte TORD</param>
    /// <param name="transportee">die transportierte Tu</param>
    public void TransportCompleted(TransportOrder tord, Tu transportee)
    {
      if (transportee == null) { throw new ArgumentNullException("transportee"); }

      _log.DebugFormat("TransportCompleted für Tu [{0}], TordId [{1}] gestartet",
        transportee.Name, tord.TordID);

      InformObservers(call => call.TransportAbgeschlossen((SgmTu)transportee, tord));
	 
      // Wenn die Tu gepuffert wurde
      // This line need to be further refactored, but not very relevant to the research.
      //if (tord.LocationNameTo.StartsWith("XX") || tord.LocationNameTo.StartsWith("XX"))
      {
        InformObservers(call => call.TuGepuffert((SgmTu)transportee));
      }
      // Tu ist aus dem Puffer abgerufen worden.
      if (tord.LocationNameTo.Equals("1360"))
      {
        transportee.LocName = "ZUSAMMENFUEHRPLATZ_AKL";
        SgmPhysik.UpdateTu(transportee);

        _log.InfoFormat("Tu [{0}] wurde aus dem AKL-Puffer gefahren und auf ZUSAMMENFUEHRPATZ_AKL gebucht.",
                       transportee.Name);
        InformObservers(call => call.TuAmZusammenfuehrPlatz((SgmTu)transportee));
      }
      

      VersucheEinlagerSperreZuLoeschen(tord);
    }

    public void TransportWaitingForNewTarget(TransportOrder tord, Tu transportee)
    {
    }
    #endregion


    #region ITransportKoordinierung

    // ToDo: Aufruf aus FotTma VerarbeiteInfo()

    /// <summary>
    /// Eine Tu darf immer dann übersetzen, wenn es keinen Eintrag zu einer anderen Tu gibt,
    /// die zum gleichen Ziel soll und die vor dieser Tu ausgelagert wurde.
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="locationName"></param>
    /// <returns></returns>
    public bool CanTablarUebersetzten(SgmTu tu, string locationName)
    {
      _log.DebugFormat("CanTablarUebersetzen Tu [{0}], Location [{1}]", tu.Name, locationName);

      IList<AuslagerstichQueue> auslagerstichQueue = DataAccess.SelectAll<AuslagerstichQueue>(Expression.True);

      AuslagerstichQueue queueEntry = auslagerstichQueue.FirstOrDefault(q => q.TuName == tu.Name);
      if(queueEntry == null)
      {
        _log.DebugFormat("Zu Tu [{0}] gibt es keinen AuslagerqueueEntry. Darf weiterfahren.", tu.Name);
        return true;
      }

      bool vorherAusgelagerteTuVorhanden = auslagerstichQueue.Any(q => q.LocNameTo == queueEntry.LocNameTo && q.Id < queueEntry.Id);
      if(vorherAusgelagerteTuVorhanden)
      {
        if (_log.IsDebugEnabled)
        {
          string andereTus = string.Format("{0}", string.Join("],[", auslagerstichQueue.Where(q => q.LocNameTo == queueEntry.LocNameTo && q.Id < queueEntry.Id).Select(q => q.TuName).ToArray()));
          _log.DebugFormat("Tus [{0}] müssen vor Tu [{1}] ausgelagert werden. Daher muss Tu [{1}] warten", andereTus, tu.Name);
        }
        return false;
      }

      _log.DebugFormat("Tu [{0}] darf weiterfahren. Current Location [{1}] Target Location [{2}]",
                       tu.Name, queueEntry.LocNameQueue, queueEntry.LocNameTo);
      return true;
    }

    #endregion ITransportKoordinierung

    #region Implementation ITransportStatus

    public void TuAtIPoint(Tu identifiedTu, Location ipointLocation, IDictionary<string, string> additionalIPointData)
    {
    }

    /// <summary>
    /// Informs about the arrival of Transport Units at a specific location.
    /// </summary>
    /// <param name="transportee">The transportee.</param>
    /// <param name="source">The location this tu came from.</param>
    /// <param name="target">The current location.</param>
    /// <param name="tord">The current transport order or <c>null</c> if there is none.</param>
    /// <param name="circumstanceOfArrival">The circumstances that led to the arrival of the transport unit.</param>
    /// <param name="additionalPlcData">Additional data send by the PLC.</param>
    public void TuMoved(Tu transportee, Location source, Location target, TransportOrder tord, CircumstanceOfArrival circumstanceOfArrival, IDictionary<string, string> additionalPlcData)
    {
      if (transportee == null) throw new ArgumentNullException("transportee");
      //if (source == null) throw new ArgumentNullException("source");
      if (target == null) throw new ArgumentNullException("target");

      _log.DebugFormat("Tu [{0}] zu Location [{1}] bewegt", transportee.Name, target.Name);

      //Die Locations, für die die ArbeitsplatzVerwaltung informiert werden muss, falls sich eine Tu dort meldet.
      //IList<string> rbgAbgabeplaetze = new List<string>() { "1255", "1265", "1275" };
      IList<string> rbgAbgabeplaetze = new List<string>() { 
      "1255","1275","1265"
	  };

      if (rbgAbgabeplaetze.Contains(target.Name))
      {
        //Informationen in die Auslagerqueue eintragen
        TrageTuInAuslagerQueueEin((SgmTu)transportee, tord, target.Name);
        // Der Status für die Entnahme und den Teilauftrag wird im AklLagerteil 
        InformObservers(m => m.TuAmRbgAbgabeplatz((SgmTu)transportee));
      }

      //Die Locations, an der der Eintrag in der AuslagerQueue gelöscht wird, falls sich eine Tu dort meldet.
      //IList<string> auslagerstichVtwUebergabe= new List<string>() { "1256", "1266", "1276" };
       IList<string> auslagerstichVtwUebergabe = new List<string>() {
       		"1276","1266","1256"
	  };
			  
      if (auslagerstichVtwUebergabe.Contains(target.Name))
      {
        //Informationen aus der Auslagerqueue löschen
        DeleteTuAusAuslagerstichQueue((SgmTu)transportee,target.Name);
      }

      //Die Locations, für die die ArbeitsplatzVerwaltung informiert werden muss, falls sich eine Tu dort meldet.
      //IList<string> arbeitsplatzStiche = new List<string>() { "1230", "1220", "1210" };
      IList<string> arbeitsplatzStiche = new List<string>() {  "1210","1220","1230" };

		

      // Die Locations der Arbeitsplaetze selbst
      IList<string> locationsDieNurEinfachBelegtWerdenKoennen = 
       // new List<string>() {"1234", "1225", "1214",
       //                     "1241", "1240",
       //                     "1274", "1264", "1254", 
       //                     "1276", "1266", "1256", 
       //                     "1239", "1229", "1219",
       //                     "RBG001E", "RBG001A", "RBG002E", "RBG002A", "RBG003E", "RBG003A", };
      new List<string>() {"1254","1264","1274","1276","1266","1256","1219","1214","1229","1225","1239","1234","RBG001E","RBG001A","RBG002E","RBG002A","RBG003E","RBG003A",};

      if (arbeitsplatzStiche.Contains(target.Name))
      {
        //ArbeitsplatzVerwaltung informieren
        InformObservers(m => m.TuAmArbeitsplatz((SgmTu)transportee, (SgmLocation)target)); 
      }

      //Wenn sich eine Tu im Bufferbereich meldet, muss die Platzsuche im Pufferbereich gestartet werden
      if (target.Name == "")
      {
        //Pufferverwaltung informieren, dass Tu am Scannerplatz angekommen ist
        InformObservers(m => m.TuAmPufferHeber((SgmTu)transportee, additionalPlcData));
      }

      if (locationsDieNurEinfachBelegtWerdenKoennen.Contains(target.Name))
      {
        // Prüfen, dass nur eine Tu am Arbeitsplatz steht:
        SicherstellenDasLocationNurMitEinerTuBelegtIst(transportee, target);
      }

      //Tu befindet sich am Pufferausgang und hat keinen Tord
      if (target.Name == "1330" && tord == null)
      {
        //wurde eine Tu ohne Auftragsbezug gefahren, wird sie hier zu NIO gebucht.
        if (!IstTuAuftragZugeordnet((SgmTu)transportee))
        {
          _log.WarnFormat("Tu [{0}] wird auf NIO gebucht, da sie sich auf [1330] ohne Auftragbezug befindet",
                          transportee.Name);

          transportee.LocName = "NIO";
          SgmPhysik.UpdateTu(transportee);
        }
      }
    }

    /// <summary>
    /// Überprüft ob noch andere Tus auf der angegebenen Location sind und bucht diese nach NIO
    /// </summary>
    /// <param name="transportee">The transportee.</param>
    /// <param name="target">The target.</param>
    private void SicherstellenDasLocationNurMitEinerTuBelegtIst(Tu transportee, Location target)
    {
      TuQuery q = SgmPhysik.GetAllTus("allTus");
      q.Where(Tu.Properties.LocName.Filter(target.Name));
      q.Where(Tu.Properties.Name.Filter(BinaryExpressionOperator.NotEqual, transportee.Name));
      q.OrderBy(Tu.Properties.OrderMove);
      IList<SgmTu> result = DataAccess.ExecuteQuery(q, row => (SgmTu)row[SgmTu.Properties.AliasName]);
      foreach (Tu tu in result)
      {
        _log.ErrorFormat("Tu [{0}] war noch auf Location [{1}]. Neue Tu [{2}] ist aber bereits eingetroffen. Alte Tu [{3}] wird nach NIO gebucht",
                         tu.Name, target.Name, transportee.Name, tu.Name);
        tu.LocName = ConstDirectory.NIO;
        SgmPhysik.UpdateTu(tu);
      }
    }

    public void TransportError(Tu transportee, Location currentLocation, TransportOrder currentTransportOrder, ErrorCondition errorCondition, IDictionary<string, string> additionalData)
    {
      if (transportee == null) throw new ArgumentNullException("transportee");
      if (errorCondition == null) throw new ArgumentNullException("errorCondition");

      if (currentTransportOrder == null)
      {
        _log.WarnFormat("TransportError() gemldete Tu [{0}] Grund: [{1}]", transportee.Name, errorCondition.Reason);
        return;
      }

      _log.WarnFormat("TransportError(): Tord [{0}] für Tu [{1}] wurde abgebrochen. Grund: [{2}]",
                      currentTransportOrder.TordID, transportee.Name, errorCondition.Reason);

      //Wenn ein Fachvoll-Konfilikt vorliegt, muss ein anderes Lagerfach in der gleichen Gasse gesucht werden.
      if (errorCondition.Reason == ErrorReason.LocationOccupied)
      {
        _log.WarnFormat("Fachvoll-Konflikt für Tu [{0}]. Starte Ersatzauftrag zu generieren.", transportee.Name);

        // Fach sperren
        LockReason grund = SperrenSystem.GetNamedLockReason("AUTOMATIC_LOC_OCCUP");

        SperrenSystem.SetLock(currentLocation, grund);

        try
        {
          // neues Fach in gleicher Gasse suchen
          SgmLocation neuesFach = (SgmLocation)StorageBinSearch.GetLocationFor(transportee);
          _log.InfoFormat("Ersatzfach [{0}] für Tu [{1}] gefunden", neuesFach.Name, transportee.Name);

          SgmLocation aktuelleLocation = (SgmLocation)SgmPhysik.GetLocationByTuName(transportee.Name);

          // ermittelt den Movetype für Ersatzaufträge
          MoveType moveType = RealeTransportKontrolle.GetMoveType(40);

          // TAKO schicken
          Transportiere((SgmTu)transportee, aktuelleLocation, neuesFach, moveType, false, additionalData);

          _log.InfoFormat("Ersatzauftrag für Tu [{0}] in Fach [{1}] generiert", transportee.Name, aktuelleLocation.Name);
        }
        catch (NoStorageBinFoundException e)
        {
          string text =
            string.Format(
              "Fachvoll-Konflikt für Tu [{0}] konnte nicht aufgelöst werden, da kein freies Fach in Gasse [{1}] vorhanden ist.",
              transportee.Name, currentLocation.Z);

          _log.Error(text, e);
          MessageStore.ProcessExceptionOrMessage(e);
        }
      }
      //Transport wurde abgebrochen
      if (errorCondition.Reason == ErrorReason.TransportCanceled)
      {
        // Wenn Auslagerung zurück ins Fach buchen
        if (currentTransportOrder.MoveTypeNo.Equals(2))
        {
          string text = string.Format("Auslagerung abgebrochen, Tu [{0}] wurde zurück ins Fach [{1}] gebucht",
                                      transportee.Name, currentTransportOrder.LocationNameFrom);
          _log.Warn(text);

          MessageStore.ProcessExceptionOrMessage(new SgmException(text));

          // Fach sperren
          LockReason grund = SperrenSystem.GetNamedLockReason("CLEARING");
          Location fach = SgmPhysik.GetLocationByName(currentTransportOrder.LocationNameFrom);

          SperrenSystem.SetLock(fach, grund);

          transportee.LocName = currentTransportOrder.LocationNameFrom;
          SgmPhysik.UpdateTu(transportee);
        }
        // Bei Einlagerung ins ZielFach buchen
        if (currentTransportOrder.MoveTypeNo.Equals(1))
        {
          EinlagerungAbgebrochen(transportee, currentTransportOrder);
        }
      }

      // Bei Fach leer wird das Fach gesperrt und die Tu ins Fach gebucht
      if (errorCondition.Reason == ErrorReason.LocationEmpty)
      {
        // Bei Einlagerung ins ZielFach buchen
        if (currentTransportOrder.MoveTypeNo.Equals(1))
        {
          EinlagerungAbgebrochen(transportee, currentTransportOrder);
        }
        else
        {
          // Fach sperren
          LockReason grund = SperrenSystem.GetNamedLockReason("AUTOMATIC_LOC_EMPTY");

          SperrenSystem.SetLock(currentLocation, grund);

          // Ziel laden falls ein Transportauftrag vorhanden ist.
          string ziel = currentTransportOrder == null ? "unbekannt" : currentTransportOrder.LocationNameTo;

          string text = string.Format("Fachleer-Konflikt, Tu [{0}] wurde ins Fach [{1}] gebucht und Fach gesperrt. Ursprüngliches Ziels [{2}]",
                                      transportee.Name, currentLocation.Name, ziel);

          _log.Warn(text);
          MessageStore.ProcessExceptionOrMessage(new SgmException(text));

          transportee.LocName = currentLocation.Name;
          SgmPhysik.UpdateTu(transportee);
        }
      }

      _log.InfoFormat("Lösche PreReservation für Tu [{0}]", transportee.Name);

      ReservationModification.DeletePrereservation(transportee);

      VersucheEinlagerSperreZuLoeschen(currentTransportOrder);
    }

    private void EinlagerungAbgebrochen(Tu transportee, TransportOrder currentTransportOrder)
    {
      string text = string.Format("Einlagerung abgebrochen, Tu [{0}] wurde ins Zielfach [{1}] gebucht",
                                  transportee.Name, currentTransportOrder.LocationNameTo);

      _log.Warn(text);
      MessageStore.ProcessExceptionOrMessage(new SgmException(text));

      // Fach sperren
      LockReason grund = SperrenSystem.GetNamedLockReason("CLEARING");
      Location fach = SgmPhysik.GetLocationByName(currentTransportOrder.LocationNameTo);

      SperrenSystem.SetLock(fach, grund);

      transportee.LocName = currentTransportOrder.LocationNameTo;
      SgmPhysik.UpdateTu(transportee);
    }

    /// <summary>
    /// Passes plant specific plc data to the solution i.e. weight/dimension information
    /// </summary>
    /// <param name="source">The source Location where the data was generated.</param>
    /// <param name="data">A dictionary containing key - value pairs.</param>
    public void AdditionalPlcData(Location source, IDictionary<string, string> data)
    {
      // Wenn sich eine Tu am Uebergabeplatz des Verschiebewagens meldet, muss ein 
      // Lagerfach gesucht werden
      //
      //IList<string> uebergabePlaetze = new List<string>() { "1219", "1229", "1239" };
		IList<string> uebergabePlaetze = new List<string>() { "1219","1229","1239"};

      if (uebergabePlaetze.Contains(source.Name))
      {
        TuQuery tuQuery = SgmPhysik.GetTusOn(source.Name);
        tuQuery.OrderBy(Tu.Properties.OrderMove);

        IList<SgmTu> tus = DataAccess.ExecuteQuery(tuQuery, row => (SgmTu)row[SgmTu.Properties.AliasName]);

        if (tus.Count == 0)
        {
          _log.WarnFormat("Keine Tu auf Platz [{0}] gefunden. AdditionalPlcData", source.Name);
          return;
        }

        // Tu die sich zuletzt auf dem Platz gemeldet hat, darf dort auch stehen.
        SgmTu neueTu = tus.Last();

        // Es darf nur eine Tu auf dem Übergabeplatz stehen. Alle anderen müssen nach  NIO gebucht werden
        for (int i = 0; i < tus.Count - 1; i++)
        {
          Tu wrongTu = tus[i];
          _log.WarnFormat("Tu [{0}] befindet sich noch am Uebergabeplatz [{1}] obwohl sich bereits [{2}] dort gemeldet hat. Wird nach NIO gebucht",
                          wrongTu.Name, source.Name, neueTu.Name);
          wrongTu.LocName = ConstDirectory.NIO;
          SgmPhysik.UpdateTu(wrongTu);
        }

        _log.InfoFormat("Tu [{0}] befindet sich am Uebergabeplatz [{1}] und wird nun versucht einzulagern",
                        neueTu.Name, source.Name);

        InformObservers(call => call.TuAmUebergabePlatzVerschiebewagen(neueTu));
      }
    }

    #endregion

    #region Hilfsfunktionen

    private void VersucheEinlagerSperreZuLoeschen(TransportOrder tord)
    {
      _log.InfoFormat("Versuche automatische Einlagersperre von Location [{0}] zu löschen", tord.LocationNameFrom);

      SgmLocation quelle = (SgmLocation)SgmPhysik.GetLocationByName(tord.LocationNameFrom);

      if (quelle.Type.Equals(SgmLocation.TypeValue.BinPosition))
      {
        // Einlagersperre löschen
        LockReason grund = SperrenSystem.GetNamedLockReason(ConstDirectory.LOR_AUTOMATIC_IN);

        try
        {
          SperrenSystem.RemoveLock(quelle, grund);
          _log.InfoFormat("Automatische Einlagersperre von Location [{0}] gelöscht", tord.LocationNameFrom);
        }
        catch (LockWasNotSetOnLocationException)
        {
          _log.InfoFormat("Automatische Einlagersperre von Location [{0}] konnte nicht gelöscht werden, da sie nicht existierte. Das ist ok.",
                          quelle.Name);
        }
      }
    }

    /// <summary>
    /// Überprüft, ob eine Tu einem Teilauftrag zugeordnet ist
    /// </summary>
    /// <param name="tu"></param>
    /// <returns></returns>
    private bool IstTuAuftragZugeordnet(SgmTu tu)
    {
      AklTeilauftragTu teilauftragTu =
        DataAccess.SelectFirst<AklTeilauftragTu>(AklTeilauftragTu.Properties.TruName.Filter(tu.Name));

      if (teilauftragTu == null)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Legt eine Eintrag in der Tu In AuslagerungQueue an
    /// </summary>
    /// <param name="tu"></param>
    /// <param name="transportOrder"></param>
    /// <param name="currentLocName"></param>
    private void TrageTuInAuslagerQueueEin(SgmTu tu, TransportOrder transportOrder, string currentLocName)
    {
      AuslagerstichQueue auslagerstichQueue = DataAccess.Create<AuslagerstichQueue>();
      auslagerstichQueue.TuName = tu.Name;
      string queueName = GetAuslagerQueueName(currentLocName);
      auslagerstichQueue.LocNameQueue = queueName;
      auslagerstichQueue.LocNameTo = transportOrder.LocationNameTo;
      InsertAuslagerstichQueue(auslagerstichQueue);
    }

    /// <summary>
    /// Die Locations 1256, 1266, 1276 an denen das Info geschickt wird sind die AuslagerQueueLocations
    /// wenn die Tu vom RBG abgegeben wird melden sie sich aber an 1255, 1265, 1275 -> dabei wird die 
    /// "Übersetzung" in die AuslagerqueueLocations benötigt.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    private string GetAuslagerQueueName(string name)
    {
      switch(name)
      {
     
     case "1255":
     	return "1256";
     
     case "1265":
     	return "1266";
     
     case "1275":
     	return "1276";
     
        default:
          throw new ArgumentException(name + " ist keine Auslager Location", "name");
      }
    }

    private void InsertAuslagerstichQueue(AuslagerstichQueue auslagerstichQueue)
    {
      _log.DebugFormat("AuslagerstichQueue angelegt [{0}]", auslagerstichQueue);
      DataAccess.Insert(auslagerstichQueue);
    }

    private void DeleteTuAusAuslagerstichQueue(SgmTu tu, string locationName)
    {
      // Alle AuslagerstichQueue-Elemente laden, um zu überprüfen, ob noch eine Tu in der Queue weiter vorne steht
      IList<AuslagerstichQueue> auslagerstichQueue = DataAccess.SelectAll<AuslagerstichQueue>(Expression.True);
      // Ermittelt den aktuellen Eintrag in der Queue 
      AuslagerstichQueue aktAuslagerstichQueue = auslagerstichQueue.Where(x => x.TuName == tu.Name).FirstOrDefault();

      if (aktAuslagerstichQueue != null)
      {
        // Ermittelt alle Eintraege in der Queue an derselben QueueLocation mit einer kleineren Id
        List<AuslagerstichQueue> uebrigeAuslagerstichQueue =
          auslagerstichQueue.Where(x => x.LocNameQueue == aktAuslagerstichQueue.LocNameQueue
                                        && x.Id < aktAuslagerstichQueue.Id).ToList();
        foreach (AuslagerstichQueue queue in uebrigeAuslagerstichQueue)
        {
          // diese Tus wurden überholt, deshalb müssen die Eintraege aus der Queue gelöscht werden
          _log.ErrorFormat(
            "Tu [{0}] zum Ziel [{1}] hat sich nicht an Location [{2}] gemeldet und wurde aus AuslagerQueue entfernt",
            queue.TuName, queue.LocNameTo, locationName);
          // ToDo: noch irgendwas wegen der Auftraege machen?
          DeleteAuslagerstichQueue(queue);
        }

        DeleteAuslagerstichQueue(aktAuslagerstichQueue);
      }
    }

    private void DeleteAuslagerstichQueue(AuslagerstichQueue auslagerstichQueue)
    {
      _log.DebugFormat("AuslagerstichQueue gelöscht [{0}]", auslagerstichQueue);
      DataAccess.Delete(auslagerstichQueue);
    }

    #endregion
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
