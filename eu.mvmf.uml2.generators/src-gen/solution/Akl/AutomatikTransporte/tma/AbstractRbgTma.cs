//******************************************************************************
// NAME: AbstractRbgTma.cs
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
using System.Linq;
using Siemens.WarehouseManagement.Configuration;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.Infrastructure.Configuration;
using Siemens.WarehouseManagement.TransactionManagement;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters.Rbg;
using Siemens.WarehouseManagement.Validation;
using TmaCancelationNotPossibleException =
  Siemens.WarehouseManagement.TransportManagement.TmaCancelationNotPossibleException;
using Siemens.WarehouseManagement.Sgm.Base;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// Diese Klasse verwaltet RBG Transportaufträge 
  /// </summary>
  public abstract class AbstractRbgTma : Observable<IRbgStatusObserver>, ITransportMediumAdapter, ILifecycle
  {
    // Define a static logger variable
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private Dictionary<string, RbgStatus> rbgStatusCache = new Dictionary<string, RbgStatus>();

    private object _locker = new object();

    protected object Locker
    {
      get { return _locker; }
    }

    #region Required Properties

    protected AbstractRbgTma()
    {
      //Setzen einer Standard Priorität von 20 -> kann von der Spring Konfiguration überschrieben werden.
      StartPriority = 50;

      log.InfoFormat("new RbgTma Hash [{0}]", this.GetHashCode());
    }

    /// <summary>
    /// Gets or sets the data access registry.
    /// </summary>
    /// <value>The data access registry.</value>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    /// <summary>
    /// Gets or sets the message store.
    /// </summary>
    /// <value>The message store.</value>
    [RequiredProperty]
    public IMessageAndExceptionHandling MessageStore { get; set; }

    /// <summary>
    /// Gets or sets the tma observer.
    /// </summary>
    /// <value>The tma observer.</value>
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
    public IPhysic Physik { get; set; }


    #endregion

    protected IDataAccess DataAccess
    {
      get { return DataAccessRegistry.GetDataAccess(); }
    }

    #region Implementation of ITransportMediumAdapter

    /// <summary>
    /// Requests a partial transport.
    /// </summary>
    /// <param name="tuName">The transport unit.</param>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="moveType">The movetype of this transport.</param>
    /// <param name="additionalData">Some optional additional data that was provided by the solution.</param>
    //[AsyncCall]
    public void RequestPartialTransport(string tuName, Coordinate source, Coordinate target, MoveType moveType, IDictionary<string, string> additionalData)
    {
      lock (_locker)
      {
        CheckPlausibilityOfRequest(tuName, source, target);

        log.InfoFormat("RbgTma has receivd requested to transport [{0}] from [{1}] to [{2}] with MoveType [{3}]", tuName, source, target, moveType);

        RbgTord rbgTord = null;
        try
        {
          rbgTord = PersistRbgTord(tuName, source, target, moveType, additionalData);

          if (rbgTord == null)
          {
            log.ErrorFormat("Could not persist RBG Tord for Tu [{0}] from [{1}] to [{2}] moveType [{3}]", tuName, source.ToString(), target.ToString(), moveType);
          }

          RunTordAssignement();
        }
        catch (Exception)
        {
          if (rbgTord != null) { DeleteRbgTord(rbgTord); }
          throw;
        }
      }
    }

    /// <summary>
    /// Cancels the partial transport.
    /// </summary>
    /// <param name="transporteeName">Name of the transport unit.</param>
    /// <param name="additionalData">Some optional additional data that was provided by the solution.</param>
    //[AsyncCall]
    public void CancelPartialTransport(string transporteeName, IDictionary<string, string> additionalData)
    {
      RbgTord tordToCancel = GetTordForTu(transporteeName);
      if (tordToCancel != null)
      {
        if (tordToCancel.Status == RbgTord.StatusValue.Processing)
        {
          MessageStore.ProcessExceptionOrMessage(new TmaTordActiveException(transporteeName, tordToCancel.SourceCoordinate, tordToCancel.TargetCoordinate));
          throw new TmaCancelationNotPossibleException(transporteeName);
        }

        DeleteRbgTord(tordToCancel);
        log.InfoFormat("Tma Tord for Tu [{0}] from [{1}] to [{2}] canceled", transporteeName, tordToCancel.SourceCoordinate, tordToCancel.TargetCoordinate);
      }
    }

    /// <summary>
    /// This tu was manually confirmed to its Destination.
    /// </summary>
    /// <param name="transporteeName">Name of the transportee.</param>
    /// <param name="additionalData">Some optional additional data that was provided by the solution.</param>
    /// <remarks>
    /// TransportMediumAdapter should not continue to monitor this
    /// TransportRequest.
    /// This Method will be called by the TCM if <see cref="ITransportControl.ConfirmTransport"/>
    /// was called to simulate arrival or if <see cref="ITransportControl.TransportAborted(TransportOrder,string)"/>
    /// was called to indicate that the user has aborted the Transport Order
    /// </remarks>
    //[AsyncCall]
    public void TransportAborted(string transporteeName, IDictionary<string, string> additionalData)
    {
      RbgTord tordToAbort = GetTordForTu(transporteeName);
      if (tordToAbort != null)
      {
        DeleteRbgTord(tordToAbort);
        log.InfoFormat("TransportAborted Tu [{0}] from [{1}] to [{2}]", transporteeName, tordToAbort.SourceCoordinate,
                       tordToAbort.TargetCoordinate);
      }
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
    public abstract void TransportToNio(Coordinate source);

    #endregion

    #region Inheritor Overridable Methods

    #region Must Ovverides

    /// <summary>
    /// Diese Methode prüft ob der übergebene Transportauftrag gefahren werden kann.
    /// </summary>
    /// <param name="tord">Der zu prüfende Transportauftrag.</param>
    /// <param name="sourceConstraintName">Der Name der Quell Prüfstrategie.</param>
    /// <param name="targetConstraintName">Der Name der Ziel Prüfstrategie.</param>
    /// <returns>
    /// 	<c>true</c> wenn der Transportauftrag gefahren werden kann, sonst false
    /// </returns>
    protected abstract bool TordCompliesWithConstraints(RbgTord tord, string sourceConstraintName, string targetConstraintName);

    /// <summary>
    /// Dieser Transportauftrag wurde zugeteilt und muss an das RBG übertragen werden.
    /// </summary>
    /// <param name="tord">Der Transportauftrag.</param>
    protected abstract void SendTordToRbg(RbgTord tord);

    /// <summary>
    /// Diese Methode sollte von einem Lösungs RBG Tma überschrieben werden
    /// um zusätzliche lösungsspezifische Daten des Tord zu verarbeiten
    /// bzw. zu speichern.
    /// </summary>
    /// <param name="tord">Der RBG Transportauftrag.</param>
    /// <param name="tuName">The tu.</param>
    /// <param name="moveType">Type of the move.</param>
    /// <param name="data">Zusätzliche Daten.</param>
    /// <remarks>
    /// Der Lösungs TMA kann Daten entweder in einer eigenen Tabelle ablegen oder an den
    /// tord anhängen. Der Tord wird erst nach Aufruf dieser Methode in der Datenbank abgelegt
    /// </remarks>
    protected virtual void HandleAdditionalData(RbgTord tord, string tuName, MoveType moveType, IDictionary<string, string> data) { }

    /// <summary>
    /// Diese Methode muss die MoveTypeNr. eines Ersatzauftrages zurückgegeben, dadurch wird diese Priorisiert und ohne Prüfung des RBG Status zugeteilt
    /// </summary>
    /// <returns></returns>
    protected virtual decimal GetKorrekturMoveType() { return -1; }

    #endregion

    #region Overridable Implementation of ILifecycle

    /// <summary>
    /// Will be called when the Application/Service starts
    /// </summary>
    /// <remarks>
    /// Abgeleitete Klassen müssen die Methode in dieser
    /// Klasse in ihrem Start() mit aufrufen, damit die
    /// Klasse korrekt funktioniert.
    /// </remarks>
    public virtual void Start()
    {
    }

    /// <summary>
    /// Will be called when the Application/Service stops
    /// </summary>
    /// <remarks>
    /// All resources hold by the component should be released. 
    /// Timers should be stopped.
    /// </remarks>
    public virtual void Stop()
    {
    }

    #endregion

    #endregion


    #region Inheritor Callable Methods

    /// <summary>
    /// Führt für alle RBGs die bereit für die Aufnahme 
    /// eines neuen Transportauftrages sind die Zuteilungsfunktion aus.
    /// 
    /// Eine priorisierte Liste von Transporttypen (z.B Ein-/Auslagerung) 
    /// wird auf Zuteilfähigkeit  geprüft. Der erste Transporttyp der 
    /// zuteilbar ist wird an das RBG vergeben.
    /// </summary>
    public void RunTordAssignement()
    {
      // Synchronisieren, damit TordAssignment auch nach Empfang eines LAZU aufgerufen werden kann.
      lock (_locker)
      {
        IList<RbgTord> allRbgTords = GetAllRbgTords();

        AssignErsatzAuftraege(allRbgTords);

        IEnumerable<RbgStatus> readyRbgs = GetReadyRBGs(allRbgTords);

        if (log.IsDebugEnabled)
        {
          log.DebugFormat("[{0}] Rbgs sind für die Zuteilung von Transportaufträgen bereit", readyRbgs.Count());
        }

        List<string> assignedRbgs = new List<string>();
        foreach (RbgStatus rbg in readyRbgs)
        {
          log.DebugFormat("Trying to assign a Tord to RBG [{0}]", rbg.Name);

          IEnumerable<RbgMovement> movements = GetOrderedMovementsForRbg(rbg.Name);
          foreach (RbgMovement movement in movements)
          {
            log.DebugFormat("Trying to get Tord for Movement [{0}]", movement.Description);

            RbgTord assignableTord = GetPrioritizedAssignableTordForMovement(movement, rbg.Name, allRbgTords);

            if (assignableTord != null)
            {
              SetMovementTimeStamp(movement);

              AssingTord(assignableTord);
              assignedRbgs.Add(rbg.Name);
              break;
            }
          }
        }

        string sReadyRbgs = string.Join(", ", readyRbgs.Select(r => r.Name).ToArray());
        string sAssignedRbgs = string.Join(", ", assignedRbgs.ToArray());
        log.InfoFormat("Zuteilungsergebniss:\n\tRBGs verfuegbar        [{0}]\n\tAuftraege zugeteilt an [{1}]",
                       sReadyRbgs, sAssignedRbgs);
      }
    }

    /// <summary>
    /// Update eines Rbg Status. Muss von abgeleiteten Klassen aufgerufen werden 
    /// wenn sich der Status eines RBGs ändert
    /// </summary>
    /// <param name="rbgStatus">The RBG status.</param>
    protected void UpdateRbgStatus(RbgStatus rbgStatus)
    {
      lock (_locker)
      {
        rbgStatusCache[rbgStatus.Name] = rbgStatus;
        DataAccess.Update(rbgStatus);
      }
    }

    /// <summary>
    /// Muss von der abgeleiteten Klasse aufgerufen werden wenn ein Transportauftrag
    /// vom RBG abgeschlossen (abgebrochen) wurde
    /// </summary>
    /// <param name="transportee">Name der Transporteinheit</param>
    /// <param name="additionalData">weitere Daten die das RBG gesendet hat.</param>
    /// <param name="transportError">Fehler oder null.</param>
    protected void TordCompleted(string transportee, IDictionary<string, string> additionalData, ErrorCondition transportError)
    {
      RbgTord tord = GetTordForTu(transportee);

      if (TordUnknown(transportee, tord))
      {
        // TODO: Falls Auftrag OK gemeldet wurde -> Daten dennoch an die TransportSteuerung weitergeben. Sonst gibt es keine Chance die Tu zu verbuchen.
        return;
      }

      if (tord.Status != RbgTord.StatusValue.Processing)
      {
        string transportErrorText = transportError == null ? "kein Fehler" : transportError.Reason.ToString();
        string text = string.Format("RbgTord [{0}] für Tord [{1}] Tu [{2}] ist in Status [{3}] nicht in Status [{4}] und kann daher nicht mit ErrorCondition [{5}] fertig gemeldet werden.",
          tord.No, tord.No, tord.TuName, tord.Status, RbgTord.StatusValue.Processing, transportErrorText);
        log.Error(text);
        SgmException e = new SgmException(text, text);
        MessageStore.ProcessExceptionOrMessage(e);
        return;
      }
      DeleteRbgTord(tord);

      InformTransportMediumObserver(tord, transportError, additionalData);

      RunTordAssignement();
    }

    #endregion

    #region Not overridable Implementation of ILifecycle

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
    public int StartPriority { get; set; }

    #endregion


    #region Help Functions

    private IList<RbgTord> GetAllRbgTords()
    {
      return DataAccess.SelectAll<RbgTord>(Expression.True);
    }

    private void AssignErsatzAuftraege(IList<RbgTord> rbgTords)
    {
      decimal moveType = GetKorrekturMoveType();
      if (moveType == -1) { return; }

      //Alle Tords mit Korrekturauftrag MoveType laden
      IOrderedEnumerable<RbgTord> korreturTords = from tord in rbgTords
                                                  where tord.Status == RbgTord.StatusValue.New
                                                        && tord.MoveType == moveType
                                                  orderby tord.Prio descending, tord.Created
                                                  select tord;

      // Korrektur Tord zuteilen.
      foreach (var tord in korreturTords)
      {
        AssingTord(tord);
      }
    }

    private void SetMovementTimeStamp(RbgMovement movement)
    {
      movement.TimestampOperated = DateTime.Now;
      DataAccess.Update(movement);
    }

    /// <summary>
    /// Überprüft ob es eine gültige Bewegungskonfiguration für diese Quell und Ziel Beziehung gibt
    /// und ob noch kein Transportauftrag für diese Tu vorliegt
    /// </summary>
    /// <param name="tuName">The transportee.</param>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    private void CheckPlausibilityOfRequest(string tuName, Coordinate source, Coordinate target)
    {
      if (tuName == null) { throw new ArgumentNullException("tuName"); }
      if (source == null) { throw new ArgumentNullException("source"); }
      if (target == null) { throw new ArgumentNullException("target"); }

      RbgTord tord = GetTordForTu(tuName);
      if (tord != null)
      {
        throw new TmaTordExistsException(tord.TuName, tord.SourceCoordinate, tord.TargetCoordinate);
      }

      //Quelle und Ziel müssen in der gleichen Gasse liegen, damit ein RBG die Kiste transportieren kann.)
      if (source.Z != target.Z)
      {
        throw new ArgumentException(string.Format("Quelle [{0}] und Ziel [{1}] liegen nicht in der gleichen Gasse", source, target));
      }

      IList<RbgMovement> movements = GetMovementsFor(source, target);
      if (movements.Count == 0)
      {
        throw new NoSuchMovementException(source.ToString(), target.ToString());
      }
    }

    private RbgTord PersistRbgTord(string tuName, Coordinate source, Coordinate target, MoveType moveType, IDictionary<string, string> additionalData)
    {
      RbgTord rbgTord = DataAccess.Create<RbgTord>();
      rbgTord.RbgName = string.Format("RBG{0:000}", target.Z);
      if (moveType.No.Equals(1) || moveType.No.Equals(GetKorrekturMoveType()))
      {
        rbgTord.RbgName += "E";
      }
      if (moveType.No.Equals(2))
      {
        rbgTord.RbgName += "A";
      }

      Location sourceLocation = Physik.GetLocationByCoordinate(source);
      Location targetLocation = Physik.GetLocationByCoordinate(target);

      rbgTord.SourceName = sourceLocation.Name;
      rbgTord.SourceCoordinate = source.CoordinateValue;

      rbgTord.TargetName = targetLocation.Name;
      rbgTord.TargetCoordinate = target.CoordinateValue;

      rbgTord.MoveType = moveType.No;
      rbgTord.Status = RbgTord.StatusValue.New;
      rbgTord.TuName = tuName;
      rbgTord.Created = DateTime.Now;
      rbgTord.Prio = RbgTord.PrioValue.Normal;

      //Lösungs Tma die Möglichkeit geben zusätzliche Daten zu persistieren.
      HandleAdditionalData(rbgTord, tuName, moveType, additionalData);

      //In DB einfügen
      DataAccess.Insert(rbgTord);
      return rbgTord;
    }

    /// <summary>
    /// Gibt alle möglichen Bewegungsarten für ein RBG zurück
    /// </summary>
    /// <param name="name">Der Name des RBGs in der Form llllzz l = lager z = gasse.</param>
    /// <returns></returns>
    private IEnumerable<RbgMovement> GetOrderedMovementsForRbg(string name)
    {
      Query q = new Query("orderedMovements");
      q = q.From<RbgMovement>().Where(RbgMovement.Properties.RbgName.Filter(name));
      MoveType.MoveTypeAlias rbgMoveTypeAlias = MoveType.GetAlias("MoveType");
      q = q.InnerJoin(rbgMoveTypeAlias, Expression.IsEqual(
                                          rbgMoveTypeAlias.No,
                                          RbgMovement.Properties.MoveType
                                          ));
      // q = q.OrderBy(OrderByDirection.Asc(rbgMoveTypeAlias.Prio));
      q = q.OrderBy(OrderByDirection.Asc(RbgMovement.Properties.TimestampOperated));

      IList<RbgMovement> result =
        DataAccess.ExecuteQuery(q, data => (RbgMovement)data["RbgMovement"]);

      return result;
    }

    /// <summary>
    /// Gibt alle möglichen Bewegungen für diese Quelle Ziel Beziehung zurück
    /// </summary>
    /// <param name="source">Quell Coordinate.</param>
    /// <param name="target">Ziel Coordinate.</param>
    /// <returns></returns>
    private IList<RbgMovement> GetMovementsFor(Coordinate source, Coordinate target)
    {
      //Gassen Coordinaten zu diesen Quell und Ziel Coordinaten
      Coordinate sourceGasse = new Coordinate(source.L, source.Z, 0, 0, 0, 0);
      Coordinate targetGasse = new Coordinate(target.L, target.Z, 0, 0, 0, 0);

      return DataAccess.SelectAll<RbgMovement>(
        Expression.And(
          Expression.Or(
            RbgMovement.Properties.SourceCoordinate.Filter(source.CoordinateValue),
            RbgMovement.Properties.SourceCoordinate.Filter(sourceGasse.CoordinateValue)
            ),
          Expression.Or(
            RbgMovement.Properties.TargetCoordinate.Filter(target.CoordinateValue),
            RbgMovement.Properties.TargetCoordinate.Filter(targetGasse.CoordinateValue)
            )
          )
        );
    }

    private IEnumerable<RbgStatus> GetReadyRBGs(IList<RbgTord> allRbgTords)
    {
      var q =
        from n in rbgStatusCache.Values
        where n.Status == RbgStatus.StatusValue.Ready
        select n;

      //Nur als Ready zurückgeben, wenn ausserdem kein Transportauftrag bereits zugeteilt ist.
      List<RbgStatus> ret = new List<RbgStatus>();

      foreach (RbgStatus r in q)
      {
        if (NoTordAssignedToRbg(r.Name, allRbgTords))
        {
          ret.Add(r);
        }
      }

      return ret;
    }

    private bool NoTordAssignedToRbg(string rbgName, IList<RbgTord> rbgTords)
    {
      var q = from tord in rbgTords
              where string.Compare(tord.RbgName, rbgName) == 0 &&
                    tord.Status == RbgTord.StatusValue.Processing
              select tord;

      return q.Count() == 0;
    }

    protected RbgTord GetAssignedTordForRbg(string rbgName)
    {
      IList<RbgTord> rbgTords = GetAllRbgTords();
      var q = from tord in rbgTords
              where string.Compare(tord.RbgName, rbgName) == 0 &&
                    tord.Status == RbgTord.StatusValue.Processing
              select tord;

      return q.FirstOrDefault();
    }

    /// <summary>
    /// Determines whether the specified tord coordinate matches the movementCoordinate.
    /// </summary>
    /// <param name="tordCoordinate">The tord coordinate.</param>
    /// <param name="movementCoordinate">The movement coordinate.</param>
    /// <returns>
    /// 	<c>true</c> if the specified tord coordinate matches the movementCoordinate; otherwise, <c>false</c>.
    /// </returns>
    private bool IsMatch(string tordCoordinate, string movementCoordinate)
    {
      //Falls die Strings übereinstimmen ist alles ok
      if (string.Compare(tordCoordinate, movementCoordinate) == 0)
      {
        return true;
      }

      //Ansonsten kann die Tord Coordinate evtl. noch auf eine Gassen Coordinate eingstimmt werden:
      string tordGassenCoordinate = string.Format(CultureInfo.InvariantCulture, "{0}0000000000", tordCoordinate.Substring(0, 6));
      if (string.Compare(tordGassenCoordinate, movementCoordinate) == 0)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Gibt den ältesten Zuteilbaren Transportauftrag für diese Bewegungsart zurück
    /// </summary>
    /// <param name="movement">The movement.</param>
    /// <param name="name">The name.</param>
    /// <param name="rbgTords"></param>
    /// <returns></returns>
    private RbgTord GetPrioritizedAssignableTordForMovement(RbgMovement movement, string name, IList<RbgTord> rbgTords)
    {
      //Alle Tords laden die mit dieser Bewegungsart gefahren werden können
      IOrderedEnumerable<RbgTord> tordsOrderdByAge = from tord in rbgTords
                                                     where tord.RbgName == movement.RbgName
                                                           && IsMatch(tord.SourceCoordinate, movement.SourceCoordinate)
                                                           && IsMatch(tord.TargetCoordinate, movement.TargetCoordinate)
                                                           && tord.Status == RbgTord.StatusValue.New
                                                     orderby tord.Prio descending, tord.Created
                                                     select tord;

      IEnumerable<RbgTord> tordsOrdered = ReorderTordsForMovement(tordsOrderdByAge, movement);

      if (log.IsDebugEnabled)
      { log.DebugFormat("Found [{0}] Tords for RbgMovement [{1}]", tordsOrderdByAge.Count(), name); }

      //Jeden Tord auf zuteilbarkeit prüfen
      foreach (RbgTord tord in tordsOrdered)
      {
        if (log.IsDebugEnabled)
        { log.DebugFormat("Applying constraints to Tord [{0}] for Rbg[{1}] for Tu [{2}] from [{3}] to [{4}]", tord.No, tord.RbgName, tord.TuName, tord.SourceCoordinate, tord.TargetCoordinate); }

        if (TordCompliesWithConstraints(tord, movement.SourceConstraintName, movement.TargetConstraintName))
        {
          return tord;
        }
      }

      //Kein Tord ist zuteilbar
      return null;
    }

    /// <summary>
    /// Give the inheritor the possibility to reorder the Tords before trying to assign them to an rbg
    /// </summary>
    /// <param name="orderedTords">The preordered tords (by age).</param>
    /// <returns></returns>
    protected virtual IEnumerable<RbgTord> ReorderTordsForMovement(IOrderedEnumerable<RbgTord> orderedTords, RbgMovement movement)
    {
      return orderedTords;
    }

    /// <summary>
    /// Ändert den Status auf Processing und ruft die Ableitung auf um den Tord an das RBG zu verschicken.
    /// </summary>
    /// <param name="tord">The tord.</param>
    private void AssingTord(RbgTord tord)
    {
      DataAccess.ExecuteTransacted(delegate
                                     {
                                       tord.Status = RbgTord.StatusValue.Processing;
                                       DataAccess.Update(tord);
                                       SendTordToRbg(tord);
                                     });

      log.InfoFormat("Tord [{0}] from [{1}] to [{2}] for Tu [{3}] assigned to RBG [{4}]", tord.No, tord.SourceCoordinate, tord.TargetCoordinate, tord.TuName, tord.RbgName);
    }

    /// <summary>
    /// Gibt den Tord für die Transporteinheit oder null zurück falls kein Tord existiert
    /// </summary>
    /// <param name="transporteeName">Name of the transportee.</param>
    /// <returns></returns>
    protected RbgTord GetTordForTu(string transporteeName)
    {
      IList<RbgTord> rbgTords = GetAllRbgTords();
      foreach (RbgTord tord in rbgTords)
      {
        if (string.Compare(tord.TuName, transporteeName) == 0)
        {
          return tord;
        }
      }

      return null;
    }

    private void DeleteRbgTord(RbgTord tord)
    {
      tord.Status = RbgTord.StatusValue.Done;
      tord.Quittiert = DateTime.Now;
      try
      {
        DataAccess.Update(tord);
        DataAccess.Delete(tord);
      }
      catch (Exception e)
      {
        string text = string.Format("Konnte RbgTord [{0}] von [{1}] nach [{2}] für Tu [{3}] nicht löschen.", tord.No, tord.SourceName, tord.TargetName, tord.TuName);
        log.Warn(text, e);
      }
    }

    /// <summary>
    /// Prüft ob der Transporauftrag bekannt ist. Falls nicht wird
    /// eine Fehlermeldung in die Message Tabelle geschrieben, so das das
    /// aufrufende TMA keine Exception erhält, der Bediener aber nachvollziehen kann, 
    /// dass etwas schief gelaufen ist
    /// </summary>
    /// <param name="transportee">The transportee.</param>
    /// <param name="tord">The tord.</param>
    /// <returns><c>true</c> falls der Transportauftrag nicht bekannt ist</returns>
    private bool TordUnknown(string transportee, RbgTord tord)
    {
      if (tord == null)
      {
        RbgTordUnknownMessage message = new RbgTordUnknownMessage(transportee);
        MessageStore.ProcessExceptionOrMessage(message);
        return true;
      }

      return false;
    }

    private void InformTransportMediumObserver(RbgTord tord, ErrorCondition transportError, IDictionary<string, string> additionalData)
    {
      if (transportError == null)
      {
        // Normalfall
        TmaObserver.TuAtLocation(tord.TuName, new Coordinate(tord.TargetCoordinate), additionalData);
      }
      else
      {
        if (transportError.Reason == ErrorReason.LocationEmpty)
        {
          TmaObserver.TransportError(tord.TuName, new Coordinate(tord.SourceCoordinate), transportError, additionalData);
        }
        else
        {
          TmaObserver.TransportError(tord.TuName, new Coordinate(tord.TargetCoordinate), transportError, additionalData);
        }
      }
    }

    #endregion
  }
}

//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************
