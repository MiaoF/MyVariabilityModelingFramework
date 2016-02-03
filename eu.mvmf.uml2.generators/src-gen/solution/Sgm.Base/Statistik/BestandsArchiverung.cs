    
//******************************************************************************
// NAME: BestandsArchivierung.cs
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Sgm.Base.SgmMaterialVerwaltung;
using Sgm.Base.Zyklus;
using Siemens.WarehouseManagement.Configuration;
using Siemens.WarehouseManagement.ContractInfrastructure.Expressions;
using Siemens.WarehouseManagement.DataAccess;
using Siemens.WarehouseManagement.Infrastructure.SystemParameters;
using Siemens.WarehouseManagement.Validation;

namespace Sgm.Base.Statistik
{
  /// <summary>
  /// 
  /// </summary>
  public class BestandsArchivierung : Zyklisch
  {
    private static log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    #region Required Properties

    /// <summary>
    /// Gets or sets the data access registry.
    /// </summary>
    /// <value>The data access registry.</value>
    [RequiredProperty]
    public IDataAccessRegistry DataAccessRegistry { get; set; }

    /// <summary>
    /// Zugriff auf die SystemParamaters
    /// </summary>
    [RequiredProperty]
    public SystemParameters SystemParameters { get; set; }

    /// <summary>
    /// Gets or sets the MaterialManagement with a proxy
    /// </summary>
    [RequiredProperty]
    public ISgmMaterialManagement SgmMaterialManagement { get; set; }

    #endregion

    ///<summary>Returns the default instance for the current thread.</summary>
    protected IDataAccess DataAccess
    {
      get { return DataAccessRegistry.GetDataAccess(); }
    }

    private TimeSpan LaengePeriodeH
    {
      get
      {
        int periode = SystemParameters.GetInt32("LAENGE_PERIODE_BESTANDS_ARCHIVIERUNG_H", 24);
        return new TimeSpan(periode, 0, 0);
      }
    }

    private DateTime LetztePeriode
    {
      get
      {
        //CultureInfo germanCulture = CultureInfo.CreateSpecificCulture("de-DE");

        //// Lade das Datum der letzten KostenRechnung und lege es an, falls es noch nicht existiert.
        //string sDatum = SystemParameters.GetString("DATI_LETZTE_BESTANDSARCHIVIERUNG", null);
        //if (string.IsNullOrEmpty(sDatum))
        //{
        //  return DateTime.MinValue;
        //}

        //return DateTime.Parse(sDatum, germanCulture).Date;
        DateTime? letztePeriode = DataAccess.SelectMax<StatBestand, DateTime?>(StatBestand.Properties.DatiCreated, Expression.True);
        if (!letztePeriode.HasValue)
        {
          return DateTime.MinValue;
        }
        return letztePeriode.Value;
      }
    }


    private bool _isRunning = false;
    private bool _ersterLaufNachNeustart = true;

    public BestandsArchivierung()
    {
      _ersterLaufNachNeustart = true;
    }

    #region Overrides of Zyklisch

    /// <summary>
    /// Wird Zyklisch aufgerufen
    /// </summary>
    public override void ZyklischeAktion()
    {
      try
      {
        log.Debug("Prüfe, ob Bestandsarchivierung erforderlich");
        DateTime? letztePeriode = LetztePeriode;

        // Falls noch keine Archivierung stattgefunden oder sie älter ist als die Länge der Periode wird eine neue Archivierung durchgeführt
        if (!_isRunning && (letztePeriode + LaengePeriodeH < DateTime.Now))
        {
          StarteBestandsArchivierung();
        }
        // Nur nach dem Neustart muss auf unvollständige Archivierung geprüft werden
        else if (!_isRunning && _ersterLaufNachNeustart)
        {
          // Fügt fehlende Einträge ein. Kann bei neustart des Servers währen der Archivierung entstehen
          VervollstaendigeBestandsArchivierung();
        }
        _ersterLaufNachNeustart = false;
      }
      catch (Exception e)
      {
        log.ErrorFormat("Fehler bei BestandsArchivierung: {0} ", e);
      }

    }

    #endregion

    /// <summary>
    /// Diese Funktion startet die Bestandsarchivierung. Sie kann z.B auch für einen Button verwendet werden
    /// </summary>
    public void StarteBestandsArchivierung()
    {
      log.Info("Starte Bestandsarchivierung()");
      Thread thread = new Thread(new ThreadStart(AsyncStarteBestandsArchivierung));
      thread.Start();
    }
    /// <summary>
    /// Diese Funktion vervollständigt eine nicht fertige Bestandsarchivierung. Sie kann z.B auch für einen Button verwendet werden.
    /// Der Fall einer nicht vollständigen Bestandsarchivierung entsteht wenn der Server während einer Archivierung beendet wird.
    /// </summary>
    public void VervollstaendigeBestandsArchivierung()
    {
      log.Info("Starte VervollstaendigeBestandsArchivierung()");
      Thread thread = new Thread(new ThreadStart(AsyncVervollstaendigeBestandsArchivierung));
      thread.Start();
    }

    private void AsyncVervollstaendigeBestandsArchivierung()
    {
      try
      {
        _isRunning = true;

        // Liefert die Query für die Bestandsberechnung
        Query query = GetBestandsQuery();

        query.LeftOuterJoin<StatBestand>(
          Expression.And(
            Expression.IsEqual(StatBestand.Properties.SapMaterialNo, SgmMaterial.Properties.SapMaterialNo),
            StatBestand.Properties.DatiCreated.Filter(LetztePeriode)));

        query.Where(StatBestand.Properties.SapMaterialNo.Filter(null));

        var materialDaten = DataAccess.ExecuteQuery(query, row => new
        {
          Material = (SgmMaterial)row[SgmMaterial.Properties.AliasName],
          Menge = (decimal)row["MaterialMenge"]
        });
        DateTime datiCreated = LetztePeriode;
        foreach (var materialData in materialDaten)
        {
          // Legt einen Eintrag in StatBestand an
          CreateStatistikBestand(materialData.Material, materialData.Menge, datiCreated);
        }
        log.InfoFormat("Vervollständigung der Bestandsarchivierung von [{0}] Materialien abgeschlossen", materialDaten.Count);
      }
      catch (Exception e)
      {
        log.ErrorFormat("Fehler bei BestandsArchivierung: {0} ", e);
      }
      _isRunning = false;
    }

    private void AsyncStarteBestandsArchivierung()
    {
      try
      {
        _isRunning = true;

        // Liefert die Query für die Bestandsberechnung
        Query query = GetBestandsQuery();

        var materialDaten = DataAccess.ExecuteQuery(query, row => new
                                                             {
                                                               Material = (SgmMaterial)row[SgmMaterial.Properties.AliasName],
                                                               Menge = (decimal)row["MaterialMenge"]
                                                             });
        DateTime datiCreated = DateTime.Now;
        foreach (var materialData in materialDaten)
        {
          // Legt einen Eintrag in StatBestand an
          CreateStatistikBestand(materialData.Material, materialData.Menge, datiCreated);
        }

        log.InfoFormat("Bestandsarchivierung von [{0}] Materialien abgeschlossen", materialDaten.Count);
      }
      catch (Exception e)
      {
        log.ErrorFormat("Fehler bei BestandsArchivierung: {0} ", e);
      }
      _isRunning = false;
    }

    private void CreateStatistikBestand(SgmMaterial material, decimal menge, DateTime datiCreated)
    {
      decimal verbrauch = GetVerbrauch(material);

      StatBestand bestand = DataAccess.Create<StatBestand>();
      bestand.SapMaterialNo = material.SapMaterialNo;
      bestand.Menge = menge;
      bestand.Verbrauch = verbrauch;
      bestand.DatiCreated = datiCreated;
      DataAccess.Insert(bestand);

      material.DatiStatCreated = datiCreated;
      SgmMaterialManagement.UpdateMaterial(material);

      log.Debug(bestand);
    }

    private Query GetBestandsQuery()
    {
      Query query = new Query("Bestand");
      query.From<SgmMaterial>();
      // TuPos hinzufügen
      query.LeftOuterJoin<SgmTuPos>(Expression.IsEqual(SgmMaterial.Properties.Id, SgmTuPos.Properties.MatId));
      // Nur nicht gesperrte TuPositionen
      query.Where(Expression.Or(SgmTuPos.Properties.MarkLocked.Filter(false), SgmTuPos.Properties.Id.Filter(null)));

      // Berechnet die Menge
      CalculatedProperty cpMenge = new CalculatedProperty("MaterialMenge", Expression.Sum(Expression.Subtract(SgmTuPos.Properties.Amount, SgmTuPos.Properties.MengeBereitgestellt)));
      query.Select(cpMenge);

      query.GroupBy<SgmMaterial>();
      return query;
    }

    private decimal GetVerbrauch(SgmMaterial material)
    {

      DateTime letzePeriodeMat = GetLetztePeriodeMat(material);
      Query query;
      query = new Query("Verbrauch");
      query.From<SgmMaterial>();
      query.Where(SgmMaterial.Properties.Id.Filter(material.Id));
      // Nur Entnahmen die in der aktuellen Periode kommissioniert wurden
      query.LeftOuterJoin<VEntnahme>(Expression.And(Expression.IsEqual(VEntnahme.Properties.MatId, SgmMaterial.Properties.Id),
        VEntnahme.Properties.DatiStatusUpdatet.Filter(BinaryExpressionOperator.GreaterThan, letzePeriodeMat)));

      CalculatedProperty cpVerbrauch = new CalculatedProperty("Verbrauch", Expression.Sum(VEntnahme.Properties.Menge));
      query.Select(cpVerbrauch);

      query.GroupBy(SgmMaterial.Properties.Id);

      var t = DataAccess.ExecuteQuery(query, row => (decimal)row[cpVerbrauch.Name]).First();
      return t;
    }


    private DateTime GetLetztePeriodeMat(SgmMaterial material)
    {
      if(!material.DatiStatCreated.HasValue)
      {
        return DateTime.MinValue;
      }
      return material.DatiStatCreated.Value;

    }

  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2010. Confidential. All rights reserved.
//******************************************************************************



