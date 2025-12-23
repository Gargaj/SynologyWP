using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynologyWP.Pages
{
  public partial class MainPage : Page, INotifyPropertyChanged
  {
    private App _app;
    private uint _isLoading = 0;

    public MainPage()
    {
      InitializeComponent();
      _app = (App)Application.Current;
      DataContext = this;
    }

    public void StartLoading() { _isLoading++; OnPropertyChanged(nameof(IsLoading)); }
    public void EndLoading() { _isLoading--; OnPropertyChanged(nameof(IsLoading)); }
    public bool IsLoading { get { return _isLoading > 0; } }

    protected async override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
    {
      var result = await _app.Client.GetAsync<API.Commands.SYNO.API.InfoResult>(new API.Commands.SYNO.API.Info());
      if (result != null)
      {
        _app.Client.SetEndpoints(result);
      }
    }

    private async void Main_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
    {
      var filesInlay = args.Item.ContentTemplateRoot as Inlays.FilesInlay;
      if (filesInlay != null)
      {
        await filesInlay.Refresh();
      }
    }

    private void Main_PivotItemUnloading(Pivot sender, PivotItemEventArgs args)
    {
      var filesInlay = args.Item.ContentTemplateRoot as Inlays.FilesInlay;
      if (filesInlay != null)
      {
        filesInlay.Flush();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
