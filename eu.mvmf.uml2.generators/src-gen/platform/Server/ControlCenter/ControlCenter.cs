
//******************************************************************************
// NAME: ControlCenter.cs
//******************************************************************************
//
// Description:
//
//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************

using System.Collections.Generic;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics;
using Siemens.WarehouseManagement.Validation;

namespace Siemens.WarehouseManagement.ControlCenter
{
  public class ControlCenter : IControlCenter
  {
    // Define a static logger variable
/*
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
*/

    #region Properties

    #region required Properties

    /// <summary>
    /// Gets or sets the data access registry.
    /// </summary>
    /// <value>The data access registry.</value>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry
    {
      get;
      set;
    }

    //Convenience Property

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

    /// <summary>
    /// Gets or sets the Physic.
    /// </summary>
    /// <value>The Physic.</value>
    [RequiredProperty]
    public IPhysicInternal Physic
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the Physic.
    /// </summary>
    /// <value>The Physic.</value>
    [RequiredProperty]
    public ITransportControl TransportControl
    {
      get;
      set;
    }


    #endregion

    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    #endregion

    /// <summary>
    /// Get all Tus at the storage bin location
    /// which are not connected with an open notificationPos
    /// which has no material reservation
    /// </summary>
    /// <param name="workset">the workset location</param>
    /// <returns>A query to recieve all tus</returns>
    public Query GetTuAndTup(string workset)
    {
      Query query = new Query("GetTuAndTup");
      query.From<Tu>();
      query.InnerJoin<TuPos>(Expression.IsEqual(TuPos.Properties.TruName, Tu.Properties.Name));
      query.InnerJoin<Material>(Expression.IsEqual(TuPos.Properties.MatId, Material.Properties.Id));
      query.InnerJoin<Location>(Expression.IsEqual(Location.Properties.Name, Tu.Properties.LocName));

      //get all tu which are at the storagebin location
      query.Where(Location.Properties.Type.Filter(Location.TypeValue.BinPosition));

      //we need all tu not at the clear location
      IList<Location> locList = DataAccess.SelectAll<Location>(Location.Properties.UsaName.Filter("CLEARING"));

      foreach (Location loc in locList)
      {
        query.Where(Expression.NotEqual(Tu.Properties.LocName, new ConstantExpression(loc.Name)));
      }

      query = Physic.GetLocationsForQuery(query, workset);

      return query;
    }

    /// <summary>
    /// gets all Tu linked with TuPos
    /// at the clear location
    /// </summary>
    /// <param name="workset">the workset location</param>
    public Query GetTuAtClearLoc(string workset)
    {
      Query query = new Query("GetTuAtClearLoc");
      query.From<Tu>();
      query.InnerJoin<TuPos>(Expression.IsEqual(TuPos.Properties.TruName, Tu.Properties.Name));
      query.InnerJoin<Material>(Expression.IsEqual(TuPos.Properties.MatId, Material.Properties.Id));
      query.InnerJoin<Location>(Expression.IsEqual(Tu.Properties.LocName, Location.Properties.Name));
      query.Where(Location.Properties.UsaName.Filter("CLEARING"));

      query = Physic.GetLocationsForQuery(query, workset);
      
      Location loc = DataAccess.SelectFirst<Location>(Location.Properties.UsaName.Filter("CLEARING"));

      //if (loc == null)
      //  throw new LocationNotFoundException("Clearing location not found");

      query.Where(Tu.Properties.LocName.Filter(loc.Name));

      return query;
    }

    /// <summary>
    /// locks the tupos with clearing lock
    /// transports the given tu to the clearing location
    /// <remarks>IMPL:SVC_CTRL_TU_REQUEST:IMPL</remarks>
    /// </summary>
    /// <param name="tuName">the tu name</param>
    /// <param name="workset">the workset location</param>
    public void TransportToClearLoc(string tuName, string workset)
    {
      MoveType type = new MoveType { No = 3 };

      Tu tu = Physic.GetTu(tuName);
      if (tu == null)
      {
        throw new TuNotFoundException(tuName);
      }

      Location source = Physic.GetLocationByName(tu.LocName);

      // todo: target is the clearing loc class location
      IList<string> locUsage = new List<string> {"CLEARING"};
      IList<Location> locList = Physic.GetLocationForUsage(locUsage, workset);
      Location target = null;
      foreach (Location loc in locList)
      {
        target = loc;
        break;
      }

      if (target == null)
      {
        throw new LocationNotFoundException("No Clearing location", tuName);
      }

      TransportControl.Transport(tu, source, target, type, false, null, "");
    }
  }
}