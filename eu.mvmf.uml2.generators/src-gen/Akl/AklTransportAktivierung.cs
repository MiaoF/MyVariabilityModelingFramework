

//******************************************************************************
// NAME: AklMaterialErmittlung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sgm.Base;
using Sgm.Base.Auftraege;
using Sgm.Base.SgmMaterialVerwaltung;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.ErrorHandling;
using Siemens.WarehouseManagement.Sgm.Base;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AklTransportAktivierung
{
	public class AklTransportAktivierung : AklTransportAktivierungBase
    {
        [RequiredProperty]
          

        protected override Lagerteil PartName
        {
            get { return Lagerteil.AKL_ToDo; }
        }

        protected override void NextStep()
        {
        }
    }
    
  public class AklTransportAktivierungBase : Zyklisch
  {
    //Logger
    private static log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Properties

    [RequiredProperty]
    //public ISgmMaterialErmittlung SgmMaterialErmittlung { get; set; }


    /// <summary>
    /// Zugriff auf die Datenbank
    /// </summary>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }
    private IDataAccess DataAccess
    {
      get
      {
        return DataAccessRegistry.GetDataAccess();
      }
    }
    #endregion Properties

    #region Overrides of Zyklisch

    protected abstract Sgm.Base.Lagerteil PartName
    {
        get;
    }

    protected abstract void NextStep();

    public override void ZyklischeAktion()
    {
      log.Info("Start AklTransportAktivierung.ZyklischeAktion()");

      //TODO: add the code to implement local tasks:AklTransportAktivierung
      NextStep();

      log.Info("Ende AklTransportAktivierung.ZyklischeAktion()");
    }

    #endregion

  }
}
   

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************


