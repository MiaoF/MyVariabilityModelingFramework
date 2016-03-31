// *******************************************************************
// <copyright company="Siemens AG" file="SgmGassenSuche.cs">
//   Copyright (C) Siemens AG 2009. Confidential. All rights reserved.
// </copyright>
// *******************************************************************
// <summary>
//   
// </summary>
// *******************************************************************

using System.Collections.Generic;
using System.Linq;
using Sgm.Base;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.MaterialManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics.Queries;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AutomatikTransporte
{
  using System;

  /// <summary>
  /// 
  /// </summary>
  public class SgmGassenSucheAkl : IAisleFinder
  {
    /// <summary>
    /// Define a static logger variable
    /// </summary>
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    #region RequiredProperties

    [RequiredProperty]
    protected IReservationModification ReservationModification { get; set; }

    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    #endregion

    private IDataAccess DataAccess { get { return DataAccessRegistry.GetDataAccess(); } }

    public IList<Coordinate> FindAisles(Tu tu, Coordinate warehouseType, IDictionary<string, object> additionalData)
    {
      // Falls die Tu bereits in einer Gasse bzw. auf dem Einlagerstich einer Gasse steht kommt nur noch diese Gasse in Frage.
      if(warehouseType.Z > 0)
      {
        return new List<Coordinate> { new Coordinate(warehouseType.L, warehouseType.Z, 0, 0, 0, 0) };
      }

      // Unreservierte Einlagerplätze laden
      LocationQuery unreservierteEinlagerPlaetze = ReservationModification.GetArivableLocations("unresLocs", tu.Name);
      unreservierteEinlagerPlaetze.Where(Expression.Or(
      		Location.Properties.Name.Filter(1254),Location.Properties.Name.Filter(1264),Location.Properties.Name.Filter(1274),Location.Properties.Name.Filter(1254),Location.Properties.Name.Filter(1264),Location.Properties.Name.Filter(1274)
      ));
      IList<Location> unresEinlagerPlaetze = DataAccess.ExecuteQuery(unreservierteEinlagerPlaetze, row => (Location) row[unreservierteEinlagerPlaetze.LocationAlias.AliasName]);

      if(_log.IsDebugEnabled)
      {
        if(unresEinlagerPlaetze.Count > 0)
        {
          _log.DebugFormat("Gassen mit freien Plätzen auf dem Einlagerstich: [{0}]", string.Join("],[", unresEinlagerPlaetze.Select(l => l.Z.ToString()).ToArray()));
        }
        else
        {
          _log.Debug("Alle Einlagerstiche sind belegt.");
        }
      }

      // Gassen nach Anzahl freier Plätze in der benötigten Höhenklasse sortieren:
      Expression verfuegaberGassenFilter = GetVerfuegbareGassenFilter(unresEinlagerPlaetze);
      Query q2 = new Query("unreservedLocations");
      q2.From<Location>();
      q2.Where(Location.Properties.MarkLockStore.Filter(false));
      q2.Where(Location.Properties.MarkLockRetrieve.Filter(false));
      q2.Where(Location.Properties.L.Filter(warehouseType.L));
      q2.Where(Location.Properties.Type.Filter(Location.TypeValue.BinPosition));
      q2.LeftOuterJoin<Tu>(Expression.IsEqual(Location.Properties.Name, Tu.Properties.LocName));
      q2.LeftOuterJoin<Reservation>(Expression.IsEqual(Location.Properties.Name, Reservation.Properties.LocName));
      q2.Where(verfuegaberGassenFilter);
      q2.Where(Location.Properties.Type.Filter(Location.TypeValue.BinPosition));
      q2.Where(Tu.Properties.Name.Filter(null));
      q2.Where(Reservation.Properties.LocName.Filter(null));

      SgmTu sgmTu = (SgmTu)tu;
      q2.Where(SgmLocation.Properties.Hoehe.Filter(sgmTu.Hoehe));

      CalculatedProperty cp = new CalculatedProperty("FreieLocsCount", Expression.Count(Location.Properties.Name));
      q2.Select(cp);

      q2.GroupBy(Location.Properties.Z);
      q2.OrderBy(OrderByDirection.Desc(cp));

      var result = DataAccess.ExecuteQuery(q2, row => new { Gasse = (decimal)row["Location_LOC_Z"], AnzahlFrei = (decimal)row["FreieLocsCount"] });

      List<Coordinate> retVal = new List<Coordinate>();
      foreach(var gasse in result)
      {
        retVal.Add(new Coordinate(warehouseType.L, gasse.Gasse, 0, 0, 0, 0));
      }

      if (_log.IsDebugEnabled)
      {
        if (result.Count > 0)
        {
          _log.DebugFormat("Gassen [{0}] sortiert nach freien Plätzen [{1}]", string.Join("],[", result.Select(r => r.Gasse.ToString()).ToArray()), string.Join("],[", result.Select(r => r.AnzahlFrei.ToString()).ToArray()));
        }
      }

      return retVal;
    }

    private Expression GetVerfuegbareGassenFilter(IList<Location> locations)
    {
      Expression ex = Expression.False;

      foreach(Location location in locations)
      {
        ex = Expression.Or(ex, Location.Properties.Z.Filter(location.Z));
      }

      return ex;
    }
  }
}

// ******************************************************************************
// Copyright (C) Siemens AG 2009. Confidential. All rights reserved.
// ******************************************************************************
