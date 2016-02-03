//****************************************************************************
//  NAME: CoordinateDirectory.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICoordinateDirectory
  {
    /// <summary>
    /// Gets the location for locationName.
    /// </summary>
    /// <param name="locationName">The coordinate.</param>
    /// <returns></returns>
    /// <exception cref="TmaLocationNotFoundException">wird geworfen, wenn kein entsprechender Eintrag in der TmaLocationTabelle gefunden wurde</exception>
    TmaLocation GetTmaLocationForLocationName(string locationName);
    
    /// <summary>
    /// Gets the coordinate for plcLocName.
    /// </summary>
    /// <param name="plcLocName">The location.</param>
    /// <returns></returns>
    /// <exception cref="TmaLocationNotFoundException">wird geworfen, wenn kein entsprechender Eintrag in der TmaLocationTabelle gefunden wurde</exception>
    TmaLocation GetTmaLocationForPlcLocationName(string plcLocName);
  }

  /// <summary>
  /// 
  /// </summary>
  public class CoordinateDirectory : ICoordinateDirectory
  {
    // Define a static logger variable
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private IList<TmaLocation> tmaLocations = null;


    /// <summary>
    /// Gets or sets the data access registry.
    /// </summary>
    /// <value>The data access registry.</value>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    /// <summary>
    /// Convenience Method for retreiving the data access object.
    /// </summary>
    /// <value>The data access.</value>
    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    #region Implementation of ICoordinateDirectory

    /// <summary>
    /// Gets the location for locationName.
    /// </summary>
    /// <param name="locationName">The coordinate.</param>
    /// <returns></returns>
    public TmaLocation GetTmaLocationForLocationName(string locationName)
    {
      if (locationName == null) { throw new ArgumentNullException("locationName"); }

      CheckCache();

      var q = from tmaLocation in tmaLocations
              where string.Compare(tmaLocation.LocName, locationName) == 0
              select tmaLocation;

      if (q.Count() == 0)
      {
        log.WarnFormat("No TmaLocation entry for plcLocName [{0}] found", locationName);

        throw new TmaLocationNotFoundException(locationName, locationName);
      }

      return q.First();
    }

    /// <summary>
    /// Gets the coordinate for foerdertechnikName.
    /// </summary>
    /// <param name="foerdertechnikName">The location.</param>
    /// <returns></returns>
    public TmaLocation GetTmaLocationForPlcLocationName(string foerdertechnikName)
    {
      if (foerdertechnikName == null) { throw new ArgumentNullException("foerdertechnikName"); }

      CheckCache();

      var q = from tmaLocation in tmaLocations
              where string.Compare(tmaLocation.FotName, foerdertechnikName) == 0
              select tmaLocation;

      if (q.Count() == 0)
      {
        log.WarnFormat(string.Format("No TmaLocation entry for foerdertechnikName [{0}] found", foerdertechnikName), "foerdertechnikName");

        throw new TmaLocationNotFoundException(foerdertechnikName, "");
      }

      TmaLocation result = q.First();
      return result;
    }

    private void CheckCache()
    {
      if(tmaLocations == null)
      {
        tmaLocations = DataAccess.SelectAll<TmaLocation>(Expression.True);
      }
    }

    #endregion
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
