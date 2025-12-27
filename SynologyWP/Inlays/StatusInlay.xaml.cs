using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynologyWP.Inlays
{
  public partial class StatusInlay : UserControl, INotifyPropertyChanged, IInlay
  {
    private App _app;
    private Pages.MainPage _mainPage;

    public StatusInlay()
    {
      InitializeComponent();
      _app = (App)Application.Current;
      Loaded += NotificationsInlay_Loaded;
      DataContext = this;
    }

    public string MachineName { get; set; }
    public string Uptime { get; set; }
    public IEnumerable<Volume> Volumes { get; set; }

    private void NotificationsInlay_Loaded(object sender, RoutedEventArgs e)
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
    }

    public void Flush()
    {
    }

    public async Task Refresh()
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
      _mainPage?.StartLoading();

      var health = await _app.Client.GetAsync<API.Commands.SYNO.Entry.RequestResult<API.Commands.SYNO.Core.System.SystemHealthResult>>(new API.Commands.SYNO.Entry.Request()
      {
        stop_when_error = false,
        mode = "parallel",
        compound = "[{\"api\":\"SYNO.Core.System.SystemHealth\",\"method\":\"get\",\"version\":1}]",
      });

      MachineName = health?.result?.First()?.data?.hostname;
      OnPropertyChanged(nameof(MachineName));
      Uptime = health?.result?.First()?.data?.uptime;
      OnPropertyChanged(nameof(Uptime));

      var system = await _app.Client.GetAsync<API.Commands.SYNO.Core.SystemPollResult>(new API.Commands.SYNO.Core.SystemPoll()
      {
        type = "storage",
      });

      Volumes = system.vol_info.Select(s => new Volume()
      {
        Name = s.name,
        TotalSize = s.total_size,
        UsedSize = s.used_size,
      }).OrderBy(s => s.Name);
      OnPropertyChanged(nameof(Volumes));

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

    public class Volume
    {
      public string Name { get; set; }
      public string UsedSizeString => API.Helpers.HumanReadableSize(UsedSize);
      public string TotalSizeString => API.Helpers.HumanReadableSize(TotalSize);
      public double Percentage => (UsedSize / (double)TotalSize);
      public string PercentageString => Percentage.ToString("P");
      public ulong UsedSize { get; set; }
      public ulong TotalSize { get; set; }
    }
  }
}