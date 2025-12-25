using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynologyWP.Inlays
{
  public partial class AudioInlay : UserControl, INotifyPropertyChanged, IInlay
  {
    private App _app;
    private Pages.MainPage _mainPage;

    public AudioInlay()
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

    public IEnumerable<Album> RecentlyAdded { get; set; }

    private async void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
    {
      _mainPage = _app?.GetCurrentFrame<Pages.MainPage>();

      switch (args.Item.Name)
      {
        case "Artists":
          await RefreshArtists();
          break;
        case "Albums":
          await RefreshAlbums();
          break;
        case "Recent":
          await RefreshRecent();
          break;
      }
    }

    public async Task Refresh()
    {
      //await RefreshArtists();
    }

    public async Task RefreshArtists()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.ArtistListResult>(new API.Commands.SYNO.AudioStation.ArtistList());
      groupedArtists.Source = result.artists
        .Select(s => new Artist(_app.Client) { Name = s.name })
        .GroupBy(s => {
          var name = s.Name;
          if (name.Length == 0)
          {
            return '#';
          }
          if (name.StartsWith("The "))
          {
            name = name.Substring(4);
          }
          return char.IsLetter(name[0]) ? char.ToUpper(name[0]) : '#';
        })
        .OrderBy(s=>s.Key);
      ArtistsKeys.ItemsSource = groupedArtists.View.CollectionGroups;

      _mainPage?.EndLoading();
    }

    public async Task RefreshAlbums()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.AlbumListResult>(new API.Commands.SYNO.AudioStation.AlbumList());
      groupedAlbums.Source = result.albums
        .Select(s => new Album(_app.Client) { ArtistName = s.album_artist, Name = s.name })
        .GroupBy(s =>
        {
          var name = s.Name;
          if (name.Length == 0)
          {
            return '#';
          }
          return char.IsLetter(name[0]) ? char.ToUpper(name[0]) : '#';
        })
        .OrderBy(s => s.Key);
      AlbumKeys.ItemsSource = groupedAlbums.View.CollectionGroups;

      _mainPage?.EndLoading();
    }

    public async Task RefreshRecent()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.AlbumListResult>(new API.Commands.SYNO.AudioStation.AlbumList()
      {
        sort_by = "time",
        sort_direction = "desc",
        limit = 50,
      });
      RecentlyAdded = result.albums.Select(s => new Album(_app.Client) { ArtistName = s.album_artist, Name = s.name });
      OnPropertyChanged(nameof(RecentlyAdded));

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

    public class Artist
    {
      API.Client _client;

      public Artist(API.Client client) { _client = client; }
      public string Name { get; set; }
      public string CoverImageURL
      {
        get
        {
          return _client.RequestToGETQuery(new API.Commands.SYNO.AudioStation.CoverGetCover() {
            artist_name = Name,
          });
        }
      }
    }

    public class Album
    {
      API.Client _client;

      public Album(API.Client client) { _client = client; }
      public string ArtistName { get; set; }
      public string Name { get; set; }
      public string CoverImageURL
      {
        get
        {
          return _client.RequestToGETQuery(new API.Commands.SYNO.AudioStation.CoverGetCover()
          {
            album_artist_name = ArtistName,
            album_name = Name,
          });
        }
      }
    }
  }
}
