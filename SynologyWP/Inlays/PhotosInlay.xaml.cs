using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynologyWP.Inlays
{
  public partial class PhotosInlay : UserControl, INotifyPropertyChanged, IInlay
  {
    private App _app;
    private Pages.MainPage _mainPage;

    public PhotosInlay()
    {
      InitializeComponent();
      _app = (App)Application.Current;
      Loaded += NotificationsInlay_Loaded;
      DataContext = this;
    }

    private void NotificationsInlay_Loaded(object sender, RoutedEventArgs e)
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
    }

    public void Flush()
    {
    }

    public async Task Refresh()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Browse.TimelineGetResult>(new API.Commands.SYNO.FotoTeam.Browse.TimelineGet()
      {
        timeline_group_unit = "day"
      });

      _mainPage?.EndLoading();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises this object's PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property that has a new value.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
