    

//******************************************************************************
// NAME: SgmPhysik.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.TransportManagement;
using Siemens.WarehouseManagement.TransportManagement.Physics;
using Siemens.WarehouseManagement.TransportManagement.Physics.Queries;
using LocationNotFoundException = Siemens.WarehouseManagement.UserManagementPresenter.LocationNotFoundException;

namespace Sgm.Base.SgmPhysik
{
  /// <summary>
  /// Erweitert die Physik Klasse der Plattform um SGM spezifische Eigenschaften und Methoden
  /// </summary>
  public class SgmPhysik : Physic, ISgmPhysik
  {
    //Logger
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    #region Implementation of ISgmPhysik

    public SgmTu GetTuOnLocation(string locationName, string tuName)
    {

      if (string.IsNullOrEmpty(locationName))
      {
        throw new ArgumentNullException("locationName");
      }
      if (string.IsNullOrEmpty(tuName))
      {
        throw new ArgumentNullException("tuName");
      }

      _log.DebugFormat("Suche Tu mit Namen = [{0}]", tuName);

      SgmTu sgmTu = (SgmTu)GetTu(tuName);

      SgmLocation location = (SgmLocation)GetLocationByName(locationName);

      //Tu existiert bereits => location �berpr�fen
      if (sgmTu != null)
      {
        //Tu steht auf gew�nschter Location
        if (sgmTu.LocName.Equals(locationName))
        {
          _log.DebugFormat("Tu [{0}] steht bereits auf Location [{1}]", tuName, locationName);
          return sgmTu;
        }

        _log.DebugFormat("Tu [{0}] steht auf Location [{1}] und wird nun auf Location [{2}] gebucht", tuName,
                        sgmTu.LocName, locationName);

        sgmTu.LocName = location.Name;
        UpdateTu(sgmTu);

        return sgmTu;
      }

      _log.DebugFormat("Erzeuge neue Tu [{0}] auf Location [{1}].", tuName, locationName);

      return CreateTuOnLocation(location, tuName, false);
    }

    public SgmTu GetTuOnLocation(string locationName)
    {
      TuQuery tuQuery = GetTusOn(locationName);
      IList<SgmTu> result = DataAccess.ExecuteQuery(tuQuery, row => (SgmTu)row[SgmTu.Properties.AliasName]);

      if (result.Count == 0)
      {
        return null;
      }

      if (result.Count == 1)
      {
        SgmTu tu = result.First();
        return tu;
      }

      string text = string.Format("Auf [{0}] stehen [{1}] Tus. GetTuOnLocation() funktioniert nur f�r eine Tu", locationName, result.Count);
      _log.Error(text);
      throw new ApplicationException(text);
    }

    public SgmTu CreateTu(string name, string locName)
    {
      SgmLocation location = (SgmLocation)GetLocationByName(locName);

      SgmTu createdTu = CreateTuOnLocation(location, name, false);

      return createdTu;
    }


    public SgmTu CreateTablar(string tuName, string locName, bool rahmen)
    {
      TuType tuType = GetTuTypeAusTuName(tuName);
      if (tuType.Name != ConstDirectory.TUT_NAME_TABLAR)
      {
        string text = string.Format("Tu name [{0}] ist nicht im Tablar Nummernkreis", tuName);
        throw new ArgumentException(text, "tuName");
      }

      SgmLocation location = (SgmLocation)GetLocationByName(locName);

      SgmTu createdTu = CreateTuOnLocation(location, tuName, rahmen);

      return createdTu;
    }

    /// <summary>
    /// Kennzeichnet die Tu mit Konturenfehler
    /// </summary>
    /// <param name="tuName"></param>
    /// <param name="konturenFehler"></param>
    public void SetzeKonturenfehler(string tuName, int konturenFehler)
    {
      if (string.IsNullOrEmpty(tuName)) throw new ArgumentNullException("tuName");
      if (konturenFehler <= 0) throw new ArgumentException("konturenFehler <= 0");

      SgmTu tu = (SgmTu)GetTu(tuName);

      if (tu == null)
      {
        throw new TablarNichtGefundenException(string.Format("Tablar [{0}] nicht gefunden.", tuName));
      }

      tu.Fehlerkennung = konturenFehler;
      DataAccess.Update(tu);
      _log.DebugFormat("Fehlerkennung [{0}] f�r Tu [{1}] gesetzt", konturenFehler, tu.Name);
    }

    /// <summary>
    /// Liefert alle Tus Auf den zugeh�rigen Plaetzen in einem Lagerteil
    /// </summary>
    /// <param name="name">Name der Query</param>
    /// <param name="lagerteil">LocationName des Lagerteils</param>
    /// <returns>Joint Tu, Location gefiltert nach Lagerteil</returns>
    public Query ErmittleAlleVerfuegbarenTusAufPlaetzenInLagerteil(string name, Lagerteil lagerteil)
    {
      string lagerteilName = GetLagerteilNameFromLagerteil(lagerteil);

      Query query = GetAllTus(name);

      Location.LocationAlias locationAlias = Location.GetAlias("LagerteilLocation");
      query.InnerJoin(locationAlias, Expression.IsEqual(locationAlias.L, SgmLocation.Properties.L));
      query.Where(locationAlias.Name.Filter(lagerteilName));

      // Nur Pl�tze aus denen die Tu auch entnommen werden darf.
      query.Where(SgmLocation.Properties.MarkLockRetrieve.Filter(0));

      return query;
    }

    /// <summary>
    /// Liefert alle Tus Auf den zugeh�rigen Plaetzen in einem Lagerteil
    /// </summary>
    /// <param name="name">Name der Query</param>
    /// <param name="lagerteil">LocationName des Lagerteils</param>
    /// <returns>Joint Tu, Location gefiltert nach Lagerteil</returns>
    public Query ErmittleAlleVerfuegbarenPlaetzeInLagerteil(string name, Lagerteil lagerteil)
    {
      string lagerteilName = GetLagerteilNameFromLagerteil(lagerteil);

      Query query = GetAllLocations(name);

      Location.LocationAlias locationAlias = Location.GetAlias("LagerteilLocation");
      query.InnerJoin(locationAlias, Expression.IsEqual(locationAlias.L, SgmLocation.Properties.L));
      query.Where(locationAlias.Name.Filter(lagerteilName));

      // Nur Pl�tze aus denen die Tu auch entnommen werden darf.
      query.Where(SgmLocation.Properties.MarkLockRetrieve.Filter(0));

      return query;
    }

    /// <summary>
    /// Gibt die Location zur�ck, die dem MplIpunkt entspricht.
    /// </summary>
    /// <returns>
    /// Die MplIpunkt-Location
    /// </returns>
    public SgmLocation ErmittleMplIpunkt()
    {
      return (SgmLocation)GetLocationByName("MPL_IPUNKT");
    }

    /// <summary>
    /// Erzeugt eine neue Gitterbox (mit den entsprecchenden Daten) auf der angegebenen Locaation
    /// </summary>
    public SgmTu ErzeugeGitterbox(string tuName, SgmTu.HoeheValue hoehe, string locName)
    {
      if (tuName == null) throw new ArgumentNullException("tuName");
      if (locName == null) throw new ArgumentException("locName");

      TuType type = GetTuTypeByName(ConstDirectory.TUT_NAME_GITTERBOX);
      Location location = GetLocationByName(locName);

      SgmTu sgmTu = (SgmTu)base.CreateTuOnLocation(location, type, tuName);

      sgmTu.Hoehe = hoehe;
      sgmTu.MarkRahmen = false;

      DataAccess.Update(sgmTu);

      _log.InfoFormat("Gitterbox [{0}] auf [{1}]angelegt", sgmTu.Name, location.Name);

      return sgmTu;
    }

    /// <summary>
    /// L�scht den Konturenfehler f�r die �bergebene Tu
    /// </summary>
    /// <param name="tu">die Tu</param>
    public void DeleteKonturenfehler(SgmTu tu)
    {
      if (tu == null) throw new ArgumentNullException("tu");

      tu.Fehlerkennung = null;
      DataAccess.Update(tu);

      _log.InfoFormat("Konturenfehler von Tu [{0}] gel�scht.", tu.Name);
    }

    /// <summary>
    /// Ermittelt den TuType aus dem Nummernkreis des Tu-Namen
    /// </summary>
    /// <param name="tuName"></param>
    /// <returns></returns>
    public TuType GetTuTypeAusTuName(string tuName)
    {
      //if (tuName.Length != ConstDirectory.LAENGE_TU_NAME)
      //{
      //  string text = string.Format("Der Tu-Name [{0}] ist nicht [{1}] Stellen lang und kann deshalb nicht verarbeitet werden.",
      //                  tuName, ConstDirectory.LAENGE_TU_NAME);

      //  throw new ArgumentException(text);
      //}

      decimal tuNummer;
      if (!decimal.TryParse(tuName, out tuNummer))
      {
        string text = string.Format("Der Tu-Name ist keine Nummer");
        throw new ArgumentException(text);
      }

      //Gr�ne Kiste 00001-02000
      if (tuNummer >= 1 && tuNummer <= 2000)
      {
        if (tuName.Length != ConstDirectory.LAENGE_TU_NAME_GRUENE_BOX)
        {
          string text = string.Format("Der Tu-Name [{0}] ist nicht [{1}] Stellen lang und kann deshalb nicht verarbeitet werden.",
                          tuName, ConstDirectory.LAENGE_TU_NAME_GRUENE_BOX);

          throw new ArgumentException(text);
        }
        
        return GetTuTypeByName(ConstDirectory.TUT_NAME_KISTE);
      }

      //Tablar 900000-909999
      if (tuNummer >= 900000 && tuNummer <= 909999)
      {
        return GetTuTypeByName(ConstDirectory.TUT_NAME_TABLAR);
      }
      //Gr�ne Gitterbox halbhoch 10000-14000
      if (tuNummer >= 10000 && tuNummer <= 14000)
      {
        return GetTuTypeByName(ConstDirectory.TUT_NAME_GITTERBOX_HALBHOCH);
      }
      //Gitterbox20000-23500
      if (tuNummer >= 20000 && tuNummer <= 23500)
      {
        return GetTuTypeByName(ConstDirectory.TUT_NAME_GITTERBOX);
      }
      //Holzpalette 30000-33500
      if (tuNummer >= 30000 && tuNummer <= 33500)
      {
        return GetTuTypeByName(ConstDirectory.TUT_NAME_PALETTE);
      }

      throw new TuTypeUnknownException(string.Format("Es wurde kein TuType f�r Tu [{0}] gefunden", tuName));
    }

    ///<summary>
    ///
    ///</summary>
    ///<param name="gasse"></param>
    ///<returns></returns>
    public SgmLocation GetEinlagerstichFuerGasse(decimal gasse)
    {
      SgmLocation einlagerstich = null;
      if (gasse == 1)
      {
        einlagerstich = (SgmLocation)GetLocationByName(ConstDirectory.LOC_NAME_EINLAGERSTICH_GASSE_01);
        return einlagerstich;
      }
      if (gasse == 2)
      {
        einlagerstich = (SgmLocation)GetLocationByName(ConstDirectory.LOC_NAME_EINLAGERSTICH_GASSE_02);
        return einlagerstich;
      }
      if (gasse == 3)
      {
        einlagerstich = (SgmLocation)GetLocationByName(ConstDirectory.LOC_NAME_EINLAGERSTICH_GASSE_03);
        return einlagerstich;
      }
      throw new LocationNotFoundException(string.Format("Es wurde kein Einlagerstich zu Gasse [{0}] gefunden", gasse));
    }

    public Query GetAllTusOnAuslagersticheAkl()
    {
      // ToDo: sind das die richtigen Locations?
      Query query = GetAllTus("AlleTusOnAuslagerstichLocation");
      query.Where(Expression.Or(
      Location.Properties.Name.Filter("1265"),
      Location.Properties.Name.Filter("1276"),
      Location.Properties.Name.Filter("1275"),
      Location.Properties.Name.Filter("1266"),
      Location.Properties.Name.Filter("1255"),
      Location.Properties.Name.Filter("1256")
//        Location.Properties.Name.Filter("1256"),
//        Location.Properties.Name.Filter("1266"),
//        Location.Properties.Name.Filter("1276"),
//        Location.Properties.Name.Filter("1255"),
//        Location.Properties.Name.Filter("1265"),
//        Location.Properties.Name.Filter("1275")
		));
      return query;
    }

    #endregion

    #region Hilfsfunktion

    private SgmTu CreateTuOnLocation(SgmLocation location, string name, bool rahmen)
    {
      TuType type = GetTuTypeAusTuName(name);

      SgmTu sgmTu = (SgmTu)base.CreateTuOnLocation(location, type, name);

      sgmTu.Hoehe = GetTuHoeheAusTuType(type);
      sgmTu.MarkRahmen = rahmen;

      DataAccess.Update(sgmTu);

      _log.InfoFormat("SgmTu [{0}] angelegt", sgmTu.Name);

      return sgmTu;
    }



    private SgmTu.HoeheValue? GetTuHoeheAusTuType(TuType tuType)
    {
      switch (tuType.Name)
      {
        case "GIB":
          {
            return SgmTu.HoeheValue.Hoch;
          }
        case "KOM":
          {
            return SgmTu.HoeheValue.Unbekannt;
          }
        default:
          {
            return SgmTu.HoeheValue.Unbekannt;
          }
      }
    }

    private string GetLagerteilNameFromLagerteil(Lagerteil lagerteil)
    {
      string lagerteilName;
      switch (lagerteil)
      {
        case Lagerteil.Akl:
          lagerteilName = ConstDirectory.LAGERTEIL_AKL;
          break;
        case Lagerteil.Bks:
          lagerteilName = ConstDirectory.LAGERTEIL_BKS;
          break;
        case Lagerteil.Mpl:
          lagerteilName = ConstDirectory.LAGERTEIL_MPL;
          break;
        default:
          throw new ArgumentException("Lagerteil nicht bekannt", "lagerteil");
      }
      return lagerteilName;
    }

    #endregion
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************


