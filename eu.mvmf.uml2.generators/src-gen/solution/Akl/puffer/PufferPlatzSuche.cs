//****************************************************************************
//  NAME: PufferPlatzSuche.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgm.Base.SgmPhysik;
using Siemens.WarehouseManagement.DataAccess;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.MaterialManagement;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics.Queries;
using Siemens.WarehouseManagement.TransportManagement.ReservationsLocks;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.Puffer
{
  /// <summary>
  /// 
  /// </summary>
  public class PufferPlatzSuche : IPufferPlatzSuche
  {
    /// <summary>
    /// Define a static logger variable
    /// </summary>
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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


    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    /// <summary>
    /// Ermittelt einen freien Pufferplatz.
    /// </summary>
    /// <param name="sgmTu">die Tu, für die eine Location gefunden werden soll.</param>
    /// <returns>eine SgmLocation im Puffer</returns>
    public SgmLocation ErmittleFreienPufferPlatz(SgmTu sgmTu)
    {
      
      if(pufferGasse == null)
      {
        throw new LocationNotFoundException("BUFFER_GASSE");
      }

      LocationQuery unreservedLocations = ReservationModification.GetUnreservedLocations("freieLocations");

      unreservedLocations.WhereWarehouseTypeIs(2000);
      unreservedLocations.WhereTypeIs(Location.TypeValue.BinPosition);
      unreservedLocations.OrderBy(Location.Properties.X);

      IList<Location> freiePufferPlaetze = DataAccess.ExecuteQuery(unreservedLocations, row => (Location)row[Location.Properties.AliasName]);

      if(freiePufferPlaetze.Count == 0)
      {
        log.ErrorFormat("Kein freier Pufferplatz gefunden.");
        return null;
      }
      SgmLocation pufferPlatz = (SgmLocation) freiePufferPlaetze.First();

      log.InfoFormat("Pufferplatz [{0}] ist frei und wird zur Pufferung für Tu [{1}] verwendet", pufferPlatz.Name,
                     sgmTu.Name);

      ReservationModification.PreReserve(pufferPlatz, sgmTu);

      return pufferPlatz;
    }



    
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
