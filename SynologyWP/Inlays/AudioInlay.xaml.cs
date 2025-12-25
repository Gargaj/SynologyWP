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
    public IEnumerable<Album> SelectedArtistAlbums { get; set; }
    public List<Song> SelectedAlbumSongs { get; set; }
    public Artist SelectedArtist { get; set; }
    public Album SelectedAlbum { get; set; }

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
        case "ArtistView":
          await RefreshArtistView();
          break;
        case "AlbumView":
          await RefreshAlbumView();
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
        .Select(s => new Album(_app.Client) { ArtistName = s.album_artist, Name = s.name, Year = s.year })
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
      RecentlyAdded = result.albums.Select(s => new Album(_app.Client) { ArtistName = s.album_artist, Name = s.name, Year = s.year });
      OnPropertyChanged(nameof(RecentlyAdded));

      _mainPage?.EndLoading();
    }

    public async Task RefreshArtistView()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.AlbumListResult>(new API.Commands.SYNO.AudioStation.AlbumList()
      {
        artist = SelectedArtist.Name
      });
      SelectedArtistAlbums = result.albums.Select(s => new Album(_app.Client) { ArtistName = s.album_artist, Name = s.name, Year = s.year });
      OnPropertyChanged(nameof(SelectedArtistAlbums));

      _mainPage?.EndLoading();
    }

    public async Task RefreshAlbumView()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.SongListResult>(new API.Commands.SYNO.AudioStation.SongList()
      {
        album_artist = SelectedAlbum.ArtistName,
        album = SelectedAlbum.Name,
        sort_by = "disc",
        sort_direction = "asc",
        additional = "song_tag,song_audio",
      });
      SelectedAlbumSongs = result.songs.Select(s => new Song(_app.Client) {
        ID = s.id,
        Title = s.title,
        Artist = s.additional.song_tag.artist,
        Length = s.additional.song_audio.duration,
        TrackNumber = s.additional.song_tag.track,
      }).ToList();
      OnPropertyChanged(nameof(SelectedAlbumSongs));

      var playlist = new Windows.Media.Playback.MediaPlaybackList();
      foreach (var song in SelectedAlbumSongs)
      {
        var url = Windows.Media.Core.MediaSource.CreateFromUri(new System.Uri(song.URL));
        playlist.Items.Add(new Windows.Media.Playback.MediaPlaybackItem(url));
      }
      audioPlayer.Source = playlist;

      _mainPage?.EndLoading();
    }

    private void Artist_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
      var fe = e.OriginalSource as FrameworkElement;
      var artist = fe?.DataContext as Artist;
      if (artist == null)
      {
        return;
      }

      SelectedArtist = artist;
      OnPropertyChanged(nameof(SelectedArtist));
      MainPivot.SelectedItem = MainPivot.Items.First(s => (s as PivotItem).Name == "ArtistView");
    }

    private void Album_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
      var fe = e.OriginalSource as FrameworkElement;
      var album = fe?.DataContext as Album;
      if (album == null)
      {
        return;
      }

      SelectedAlbum = album;
      OnPropertyChanged(nameof(SelectedAlbum));
      MainPivot.SelectedItem = MainPivot.Items.First(s => (s as PivotItem).Name == "AlbumView");
    }

    private void Song_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
      var fe = e.OriginalSource as FrameworkElement;
      var song = fe?.DataContext as Song;
      if (song == null)
      {
        return;
      }

      var playlist = (audioPlayer.Source as Windows.Media.Playback.MediaPlaybackList);
      playlist.MoveTo((uint)SelectedAlbumSongs.IndexOf(song));
      audioPlayer.MediaPlayer.Play();
    }

    private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
    {
      audioPlayer.SetMediaPlayer(new Windows.Media.Playback.MediaPlayer());
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
      public int Year { get; set; }
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

    public class Song
    {
      API.Client _client;

      public Song(API.Client client) { _client = client; }
      public string ID { get; set; }
      public int TrackNumber { get; set; }
      public string Artist { get; set; }
      public string Title { get; set; }
      public int Length { get; set; }
      public string LengthStr => $"{Length / 60}:{Length % 60:D2}";

      public string URL
      {
        get
        {
          return _client.RequestToGETQuery(new API.Commands.SYNO.AudioStation.Stream()
          {
            id = ID
          });
        }
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
  }
}
