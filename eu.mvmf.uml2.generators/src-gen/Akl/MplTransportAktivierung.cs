

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

namespace Sgm.Akl.MplTransportAktivierung
{
	public class MplTransportAktivierung : MplTransportAktivierungBase
    {
        [RequiredProperty]
          

        protected override Lagerteil PartName
        {
            get { return Lagerteil.MPL_ToDo; }
        }

        protected override void NextStep()
        {
        }
    }
    
  public class MplTransportAktivierungBase : Zyklisch
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
      log.Info("Start MplTransportAktivierung.ZyklischeAktion()");

      //TODO: add the code to implement local tasks:MplTransportAktivierung
      NextStep();

      log.Info("Ende MplTransportAktivierung.ZyklischeAktion()");
    }

    #endregion

  }
}
   

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************


