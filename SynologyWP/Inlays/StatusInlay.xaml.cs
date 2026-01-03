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
    private API.Commands.SYNO.Core.DSMNotify.StringsResult _strings;

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
    public IEnumerable<Notification> Notifications { get; set; }

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

      await UpdateSystemHealth();
      await UpdateVolumeStatus();
      await UpdateNotifications();

      _mainPage?.EndLoading();
    }

    private async Task UpdateSystemHealth()
    {
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
    }

    private async Task UpdateVolumeStatus()
    {
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
    }

    private async Task UpdateNotifications()
    {
      if (_strings == null)
      {
        _strings = await _app.Client.GetAsync<API.Commands.SYNO.Core.DSMNotify.StringsResult>(new API.Commands.SYNO.Core.DSMNotify.Strings()
        {
          lang = "enu",
        });
      }

      var notifications = await _app.Client.GetAsync<API.Commands.SYNO.Core.DSMNotify.NotifyResult>(new API.Commands.SYNO.Core.DSMNotify.Notify()
      {
        action = "load",
      });

      Notifications = notifications.items.OrderByDescending(s => s.time).Select(s => new Notification()
      {
        Title = _strings[s.title].title,
        Time = s.time,
        FormatString = _strings[s.title].msg,
        Data = s.msg.Count > 0 ? s.msg[0] : string.Empty,
      });

      OnPropertyChanged(nameof(Notifications));
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

    public class Notification
    {
      public string Title { get; set; }
      public string Description
      {
        get
        {
          var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Data);
          var str = FormatString;
          foreach (var d in data)
          {
            str = str.Replace(d.Key, d.Value);
          }
          return str;
        }
      }
      public string TimeString
      {
        get
        {
          var date = API.Helpers.UnixTimeStampToDateTime(Time);
          return date.ToLocalTime().ToString();
        }
      }
      public ulong Time { get; set; }
      public string FormatString { get; set; }
      public string Data { get; set; }
    }
  }
}