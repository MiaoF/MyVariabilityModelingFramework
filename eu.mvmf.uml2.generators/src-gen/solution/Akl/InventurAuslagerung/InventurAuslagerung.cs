//******************************************************************************
// NAME: InventurAuslagerung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sgm.Akl.Arbeitsplaetze;
using Sgm.Akl.AuftraegeBearbeitung;
using Sgm.Base;
using Sgm.Base.Inventur;
using Sgm.Base.SgmMaterialVerwaltung;
using Sgm.Base.SgmPhysik;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.ContractInfrastructure.SystemParametersPresenter;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.InventurAuftraege
{
  /// <summary>
  ///
  /// </summary>
  public class InventurAuslagerung : Zyklisch
  {
    //Logger
    private static log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Properties

    /// <summary>
    /// Zugriff auf die ArbeitsplatzVerwaltung
    /// </summary>
    [RequiredProperty]
    public IAklArbeitsplatzVerwaltung AklArbeitsplatzVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf die AklTransportGenerierung
    /// </summary>
    [RequiredProperty]
    public IAklTransportGenerierung AklTransportGenerierung { get; set; }

    /// <summary>
    /// Zugriff auf die SgmPhysik
    /// </summary>
    [RequiredProperty]
    public ISgmPhysik Physik { get; set; }

    /// <summary>
    /// Zugriff auf die SgmMaterialManagement
    /// </summary>
    [RequiredProperty]
    public ISgmMaterialManagement MaterialManagement { get; set; }

    /// <summary>
    /// Zugriff auf die SystemParameter
    /// </summary>
    [RequiredProperty]
    public ISystemParameters SystemParameters { get; set; }

    /// <summary>
    /// Gets or sets the inventur verwaltung.
    /// </summary>
    /// <value>The inventur verwaltung.</value>
    [RequiredProperty]
    public IInventurVerwaltung InventurVerwaltung { get; set; }

    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    #endregion Properties

    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }

    #region Overrides of Zyklisch

      public override void ZyklischeAktion()
    {
      _log.Info("Start InventurAuslagerung.ZyklischeAktion()");
      InventurAuslagerungenAnlegen();
      _log.Info("Ende InventurAuslagerung.ZyklischeAktion()");
    }

    private void InventurAuslagerungenAnlegen()
    {
     
    }

    #endregion

  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
