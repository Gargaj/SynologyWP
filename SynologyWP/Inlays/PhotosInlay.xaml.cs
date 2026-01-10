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
    private API.Commands.SYNO.FotoTeam.Browse.TimelineGetResult _timeline;
    private API.Commands.SYNO.FotoTeam.Search.FilterListResult _filter;
    private List<IGrouping<string, Photo>> _groupedItems;
    private List<API.Commands.SYNO.FotoTeam.Browse.Item> _loadedSections = new List<API.Commands.SYNO.FotoTeam.Browse.Item>();
    private List<API.Commands.SYNO.FotoTeam.Browse.Item> _enqueuedSections = new List<API.Commands.SYNO.FotoTeam.Browse.Item>();
    private DispatcherTimer _periodicUpdateTimer = new DispatcherTimer();
    private Task _task = null;

    public PhotosInlay()
    {
      InitializeComponent();
      _app = (App)Application.Current;
      Loaded += NotificationsInlay_Loaded;
      _periodicUpdateTimer.Interval = TimeSpan.FromSeconds(1.0f);
      _periodicUpdateTimer.Tick += PeriodicTick;
      DataContext = this;
    }

    private void NotificationsInlay_Loaded(object sender, RoutedEventArgs e)
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
    }

    public void Flush()
    {
      _periodicUpdateTimer.Stop();
    }

    public async Task Refresh()
    {
      _mainPage?.StartLoading();

      _timeline = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Browse.TimelineGetResult>(new API.Commands.SYNO.FotoTeam.Browse.TimelineGet()
      {
        timeline_group_unit = "day"
      });

      _groupedItems = _timeline.section.SelectMany(s => s.list)
        .SelectMany(s =>
        {
          var a = new List<Photo>();
          for (int i = 0; i < s.item_count; i++) a.Add(new Photo(this, _app.Client) { Section = s });
          return a;
        }
        )
      .GroupBy(s => s.Month)
      .OrderByDescending(s => s.Key)
      .ToList();

      groupedPhotosByMonth.Source = _groupedItems;
      Months.ItemsSource = groupedPhotosByMonth.View.CollectionGroups;

      _filter = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Search.FilterListResult>(new API.Commands.SYNO.FotoTeam.Search.FilterList()
      {
        additional = "[\"thumbnail\"]",
        setting = "{\"item_type\":true,\"time\":true,\"geocoding\":true}",
      });

      _periodicUpdateTimer.Start();

      _mainPage?.EndLoading();
    }

    private void EnqueueSectionLoad(API.Commands.SYNO.FotoTeam.Browse.Item section)
    {
      if (_enqueuedSections.Contains(section) || _loadedSections.Contains(section))
      {
        return;
      }
      _enqueuedSections.Add(section);
    }

    private void PeriodicTick(object sender, object e)
    {
      if (_task != null && !_task.IsCompleted)
      {
        return;
      }
      _task = Task.Run((Action)ProcessSectionLoadQueueAsync);
    }

    private async void ProcessSectionLoadQueueAsync()
    {
      await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
      {
        _mainPage?.StartLoading();

        var sections = _enqueuedSections.ToList();
        foreach (var section in sections)
        {
          await LoadSection(section);
          _loadedSections.Add(section);
        }
        _enqueuedSections.Clear();

        _mainPage?.EndLoading();
      });
    }

    private async Task LoadSection(API.Commands.SYNO.FotoTeam.Browse.Item sectionItem)
    {
      var times = _filter.time.Where(s => s.year == sectionItem.year && s.month == sectionItem.month);
      var startTime = API.Helpers.DateTimeToUnixTimeStamp(new DateTime(sectionItem.year, sectionItem.month, sectionItem.day, 0, 0, 0).ToLocalTime());
      var endTime = API.Helpers.DateTimeToUnixTimeStamp(new DateTime(sectionItem.year, sectionItem.month, sectionItem.day, 23, 59, 59).ToLocalTime());

      var items = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Browse.ItemListResult>(new API.Commands.SYNO.FotoTeam.Browse.ItemList()
      {
        offset = 0,
        limit = sectionItem.item_count,
        start_time = startTime,
        end_time = endTime,
        additional = "[\"thumbnail\",\"video_meta\"]",
      });

      foreach (var item in items.list)
      {
        var day = string.Format($"{sectionItem.year:D4}-{sectionItem.month:D2}-{sectionItem.day:D2}");
        var month = day.Substring(0, 7);
        var group = _groupedItems?.FirstOrDefault(s => s.Key == month);

        Photo photo = null;
        photo = group.FirstOrDefault(s => s.ID == item.id);
        if (photo == null)
        {
          photo = group?.FirstOrDefault(s => s.ID == 0 && s.DateString == day);
          if (photo == null)
          {
            continue;
          }
        }

        photo.ID = item.id;
        photo.Name = item.filename;
        photo.IsVideo = item.type == "video";
        photo.VideoLengthMS = item.additional?.video_meta?.duration ?? 0;
        photo.CacheKey = item.additional.thumbnail.cache_key;

        photo.OnPropertyChanged("IsVideo");
        photo.OnPropertyChanged("VideoLengthString");
        photo.OnPropertyChanged("ImageURLThumb");
      }
    }

    private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
      e.Handled = true;

      var image = sender as Image;
      var photo = image.DataContext as Photo;
      if (photo.IsVideo)
      {
        _mainPage.VideoPlayerURL = photo.VideoURL;
      }
      else
      {
        _mainPage.ZoomedImageURL = photo.ImageURL("xl");
      }
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

    public class Photo : INotifyPropertyChanged
    {
      API.Client _client;
      PhotosInlay _inlay;

      public Photo(PhotosInlay inlay, API.Client client) { _inlay = inlay; _client = client; }
      public int ID { get; set; }
      public string Name { get; set; }
      public string Month => string.Format($"{Section.year:D4}-{Section.month:D2}");
      public string DateString => string.Format($"{Section.year:D4}-{Section.month:D2}-{Section.day:D2}");
      public API.Commands.SYNO.FotoTeam.Browse.Item Section { get; set; }
      public string CacheKey { get; set; }
      public bool IsVideo { get; set; }
      public string VideoLengthString => $"{VideoLengthMS / 60000}:{VideoLengthMS / 1000:D2}";
      public int VideoLengthMS { get; set; }
      public string ImageURLThumb => ImageURL("sm");
      public string ImageURL(string size)
      {
        if (ID == 0)
        {
          _inlay.EnqueueSectionLoad(Section);
          return null;
        }

        var url = _client.Settings.CurrentCredential.URL;
        url += "/synofoto/api/v2/t/Thumbnail/get";

        var queryParams = new System.Collections.Specialized.NameValueCollection();
        queryParams.Add("type", "unit");
        queryParams.Add("size", size);
        queryParams.Add("id", ID.ToString());
        queryParams.Add("cache_key", CacheKey);
        queryParams.Add("_sid", _client.Settings.CurrentCredential.SID);

        url += "?" + string.Join("&", queryParams.AllKeys.Select(s => $"{s}={WebUtility.UrlEncode(queryParams.GetValues(s)[0])}"));
        url = url.Replace("+", "%20"); // Synology quirk
        return url;
      }
      public string VideoURL
      {
        get
        {
          return _client.RequestToGETQuery(new API.Commands.SYNO.FotoTeam.Download()
          {
            item_id = $"[{ID}]"
          });
        }
      }


      public event PropertyChangedEventHandler PropertyChanged;

      /// <summary>
      /// Raises this object's PropertyChanged event.
      /// </summary>
      /// <param name="propertyName">The property that has a new value.</param>
      public virtual void OnPropertyChanged(string propertyName)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}
