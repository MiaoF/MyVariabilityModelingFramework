using Siemens.WarehouseManagement.DPS.Templates;
using System.Windows;

namespace Sgm.Client.Akl.DPSDialogs
{

  public partial class MaterialanfordernForm
  {
    public MaterialanfordernForm()
    {
      InitializeComponent();
      var gridDialog = (GridDialog)Content;
      #region Buttons anpassen
       // "Neu Anlegen" Button wird ausgeblendet
      gridDialog.ButtonPanel[3].Visibility = Visibility.Collapsed;
      // "Bearbeiten" Button wird ausgeblendet
      gridDialog.ButtonPanel[4].Visibility = Visibility.Collapsed;
      // "Löschen" Button wird ausgeblendet
      gridDialog.ButtonPanel[6].Visibility = Visibility.Collapsed;
      #endregion Buttons anpassen
    }
  }
}
