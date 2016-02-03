//******************************************************************************
// NAME: IRbgStatusObserver.cs
//******************************************************************************
//
// Description:
//
//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sgm.Akl.AutomatikTransporte.Tma
{
  public interface IRbgStatusObserver
  {
    void GassenDatenChanged(GassenDaten daten);
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2007. Confidential. All rights reserved.
//******************************************************************************
