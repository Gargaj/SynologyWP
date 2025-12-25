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
    private List<API.Commands.SYNO.FotoTeam.Browse.Entry> _entryList;

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
      _entryList.Clear();
    }

    public async Task Refresh()
    {
      _mainPage?.StartLoading();

      _timeline = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Browse.TimelineGetResult>(new API.Commands.SYNO.FotoTeam.Browse.TimelineGet()
      {
        timeline_group_unit = "day"
      });
      _filter = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Search.FilterListResult>(new API.Commands.SYNO.FotoTeam.Search.FilterList()
      {
        additional = "[\"thumbnail\"]",
        setting = "{\"item_type\":true,\"time\":true,\"geocoding\":true}",
      });

      _mainPage?.EndLoading();

      _entryList = new List<API.Commands.SYNO.FotoTeam.Browse.Entry>();
      for (int i = 0; i < 3; i++)
      {
        foreach (var item in _timeline.section[i].list)
        {
          LoadSection(item);
        }
      }
    }

    private async void LoadSection(API.Commands.SYNO.FotoTeam.Browse.Item sectionItem)
    {
      _mainPage?.StartLoading();

      var time = _filter.time.First(s => s.year == sectionItem.year && s.month == sectionItem.month);

      var items = await _app.Client.GetAsync<API.Commands.SYNO.FotoTeam.Browse.ItemListResult>(new API.Commands.SYNO.FotoTeam.Browse.ItemList()
      {
        offset = 0,
        limit = sectionItem.item_count,
        start_time = time.start_time,
        end_time = time.end_time,
        additional = "[\"thumbnail\",\"video_meta\"]",
      });

      _entryList.AddRange(items.list.Where(s=>!_entryList.Any(t=>t.id==s.id)));

      groupedPhotosByMonth.Source = _entryList
        .Select(s => new Photo(_app.Client)
        {
          ID = s.id,
          Name = s.filename,
          IsVideo = s.type == "video",
          VideoLengthMS = s.additional?.video_meta?.duration ?? 0,
          Month = API.Helpers.UnixTimeStampToDateTime(s.time).ToLocalTime().ToString("yyyy-MM"),
          CacheKey = s.additional.thumbnail.cache_key,
        })
        .GroupBy(s => s.Month)
        .OrderByDescending(s => s.Key);
      Months.ItemsSource = groupedPhotosByMonth.View.CollectionGroups;

      _mainPage?.EndLoading();
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

    public class Photo
    {
      API.Client _client;

      public Photo(API.Client client) { _client = client; }
      public int ID { get; set; }
      public string Name { get; set; }
      public string Month { get; set; }
      public string CacheKey { get; set; }
      public bool IsVideo { get; set; }
      public string VideoLengthString => $"{VideoLengthMS/60000}:{VideoLengthMS/1000:D2}";
      public int VideoLengthMS { get; set; }
      public string ImageURLThumb => ImageURL("sm");
      public string ImageURL(string size)
      {
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
    }
  }
}
