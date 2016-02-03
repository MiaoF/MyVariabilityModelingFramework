//****************************************************************************
//  NAME: TmaFactory.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.TransportManagement.TransportMediumAdapters;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  /// <summary>
  /// Die TmaFactory hält Referenzen auf die Tmas 
  /// </summary>
  public class TmaFactory : ITmaFactory
  {
    /// <summary>
    /// Define a static logger variable
    /// </summary>
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    #region RequiredProperties

    [RequiredProperty]
    public ITransportMediumAdapter FotTma { get; set; }

    [RequiredProperty]
    public ITransportMediumAdapter RbgTma { get; set; }

    #endregion

    #region ITmaFactory Members

    /// <summary>
    /// Gibt den Tma zur übergebenen TmaId zurück
    /// </summary>
    /// <param name="tmaId">Die Id des Tmas</param>
    /// <returns>der Tma</returns>
    public ITransportMediumAdapter GetTma(string tmaId)
    {
      log.DebugFormat("Tma [{0}] wird angefordert",tmaId);

      switch(tmaId)
      {
        case "FOT":
          return FotTma;
        case "RBG":
          return RbgTma;
        default:
          log.ErrorFormat("unbekanntes Tma [{0}] angefordert", tmaId);
          throw new ArgumentException(string.Format("tma [{0}] unknown", tmaId), "tmaId");
      }
    }

    public void SetTmStatus(ITransportMediumStatus tmStatus)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
