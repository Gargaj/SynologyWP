using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynologyWP.Pages
{
  public partial class MainPage : Page, INotifyPropertyChanged
  {
    private App _app;
    private uint _isLoading = 0;
    private string _zoomedImageURL;

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
        AudioPivot.Visibility = _app.Client.HasEndpoint("SYNO.AudioStation.Artist") ? Visibility.Visible : Visibility.Collapsed;
        PhotosPivot.Visibility = _app.Client.HasEndpoint("SYNO.FotoTeam.Browse.Timeline") ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public string ZoomedImageURL
    {
      get => _zoomedImageURL;
      set
      {
        _zoomedImageURL = value;
        OnPropertyChanged(nameof(ZoomedImageURL));
        OnPropertyChanged(nameof(IsZoomedImageValid));
      }
    }
    public bool IsZoomedImageValid => !string.IsNullOrEmpty(ZoomedImageURL);

    private void CloseZoomedImage_Click(object sender, RoutedEventArgs e)
    {
      ZoomedImageURL = null;
    }

    private async void Main_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
    {
      var inlay = args.Item.ContentTemplateRoot as Inlays.IInlay;
      if (inlay != null)
      {
        await inlay.Refresh();
      }
    }

    private void Main_PivotItemUnloading(Pivot sender, PivotItemEventArgs args)
    {
      var inlay = args.Item.ContentTemplateRoot as Inlays.IInlay;
      if (inlay != null)
      {
        inlay.Flush();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
