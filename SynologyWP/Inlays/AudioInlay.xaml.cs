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

    public async Task Refresh()
    {
      _mainPage?.StartLoading();

      var result = await _app.Client.GetAsync<API.Commands.SYNO.AudioStation.ArtistListResult>(new API.Commands.SYNO.AudioStation.ArtistList()
      {
      });

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
  }
}
