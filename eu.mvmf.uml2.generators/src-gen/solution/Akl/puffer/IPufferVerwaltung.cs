//****************************************************************************
//  NAME: IPufferVerwaltung.cs
//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siemens.WarehouseManagement.DataAccess;

namespace Sgm.Akl.Puffer
{
  /// <summary>
  /// 
  /// </summary>
  public interface IPufferVerwaltung
  {
    /// <summary>
    /// Ruft alle Tus zum übergebenen Teilauftrag zum Zusammenführplatz ab.
    /// </summary>
    /// <param name="kommissionierBox">die AuftragsId des Teilauftrags</param>
    void TeilauftragAbrufen(SgmTu kommissionierBox);
   
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//****************************************************************************
