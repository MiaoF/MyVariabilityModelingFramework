
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

namespace Sgm.Akl.AuftraegeBearbeitung
{
	/// <summary>
  	/// TODO: Add summary here
 	/// </summary>
  	
  	public class AklMaterialErmittlung : Zyklisch
    {
    	 //Logger
    	private static log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

   		#region Properties

    	//[RequiredProperty]
   		// TODO: Local Required Property, e.g. public IMaterialErmittlung SgmMaterialErmittlung { get; set; }

    	[RequiredProperty]
        
		public IZyklisch AklTransportGenerierung { get; set; }  

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

        public override void NextStep()
        {
			AklTransportGenerierung.Aufwecken();
        }
        
        public override void ZyklischeAktion()
    	{
      		log.Info("Start AklMaterialErmittlung.ZyklischeAktion()");

      		//TODO: add the code to implement local tasks:AklMaterialErmittlung
     		 NextStep();

      		log.Info("Ende AklMaterialErmittlung.ZyklischeAktion()");
   		}

    	#endregion
    	
    }
}

//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************
