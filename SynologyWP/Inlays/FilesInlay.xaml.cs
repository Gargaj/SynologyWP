using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace SynologyWP.Inlays
{
  public partial class FilesInlay : UserControl, INotifyPropertyChanged, IInlay
  {
    private App _app;
    private Pages.MainPage _mainPage;

    public FilesInlay()
    {
      InitializeComponent();
      _app = (App)Application.Current;
      Loaded += NotificationsInlay_Loaded;
      DataContext = this;
    }

    public List<Entry> Entries { get; set; }
    public string CurrentPath { get; set; }

    private void NotificationsInlay_Loaded(object sender, RoutedEventArgs e)
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
    }

    public void Flush()
    {
    }

    public async Task Refresh()
    {
      await ListDirectory("/");
    }

    async Task ListDirectory(string directory)
    {
      _mainPage = _app.GetCurrentFrame<Pages.MainPage>();
      _mainPage?.StartLoading();

      CurrentPath = directory;
      OnPropertyChanged(nameof(CurrentPath));

      if (directory == string.Empty || directory == "/")
      {
        var result = await _app.Client.GetAsync<API.Commands.SYNO.FileStation.ListShareResult>(new API.Commands.SYNO.FileStation.ListShare());

        Entries = result.shares.Select(s => new Entry()
        {
          Name = s.name,
          Path = s.path,
          Type = "dir"
        }).ToList();
      }
      else
      {
        var result = await _app.Client.GetAsync<API.Commands.SYNO.FileStation.ListResult>(new API.Commands.SYNO.FileStation.List() {
          folder_path = directory,
          additional = "[\"size\",\"time\",\"type\",\"real_path\"]"
        });

        Entries = new List<Entry>();

        var parent = directory.Substring(0, directory.LastIndexOf('/'));
        Entries.Add(new Entry() {
          Name = "..",
          Path = parent,
          Type = "parent",
        });
       
        Entries.AddRange(result.files.Select(s => new Entry()
        {
          Name = s.name,
          Path = s.path,
          Type = s.isdir ? "dir" : (s.additional?.type ?? ""),
          Info = InfoToString(s.additional),
        }));
      }
      OnPropertyChanged(nameof(Entries));

      _mainPage?.EndLoading();
    }

    public static string InfoToString(API.Commands.SYNO.FileStation.FileAdditional info)
    {
      if (info == null)
      {
        return "";
      }

      var s = string.Empty;
      s += API.Helpers.UnixTimeStampToDateTime(info.time.mtime).ToLocalTime().ToString();
      s += ", ";
      s += API.Helpers.HumanReadableSize(info.size);
      return s;
    }

    private async void Entry_Click(object sender, RoutedEventArgs e)
    {
      var button = e.OriginalSource as Button;
      if (button == null)
      {
        return;
      }
      var dataContext = button.DataContext as Entry;
      if (dataContext == null)
      {
        return;
      }

      if (dataContext.IsDirectory)
      {
        await ListDirectory(dataContext.Path);
      }
    }

    private async void Download_Click(object sender, RoutedEventArgs e)
    {
      var item = e.OriginalSource as MenuFlyoutItem;
      if (item == null)
      {
        return;
      }
      var dataContext = item.DataContext as Entry;
      if (dataContext == null)
      {
        return;
      }

      var picker = new Windows.Storage.Pickers.FileSavePicker
      {
        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
      };
      var ext = System.IO.Path.GetExtension(dataContext.Path);
      picker.DefaultFileExtension = ext;
      picker.SuggestedFileName = dataContext.Name;
      picker.FileTypeChoices.Add(ext.TrimStart('.').ToUpper() + " files", new List<string>() { ext });

      var file = await picker.PickSaveFileAsync();
      if (file != null)
      {
        _mainPage?.StartLoading();

        var result = await _app.Client.Download(new API.Commands.SYNO.FileStation.Download()
        {
          path = dataContext.Path,
          mode = "download"
        });
        
        byte[] buffer = new byte[result.Length];
        result.Position = 0;
        result.Read(buffer, 0, (int)result.Length);
        await Windows.Storage.FileIO.WriteBytesAsync(file, buffer);

        _mainPage?.EndLoading();

        var dialog = new ContentDialog
        {
          Content = new TextBlock { Text = $"Successfully downloaded to {file.Path}", TextWrapping = TextWrapping.WrapWholeWords },
          Title = $"Download successful!",
          PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync();
      }
    }

    private void FileMenu_Click(object sender, RoutedEventArgs e)
    {
      var element = sender as FrameworkElement;
      if (element != null)
      {
        FlyoutBase.ShowAttachedFlyout(element);
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

    public class Entry
    {
      public string Icon
      {
        get
        {
          switch (Type)
          {
            case "parent": return "\xE752";
            case "dir": return "\xF12B";
            case "text":
            default: return "\xF000";
          }
        }
      }
      public string Type { get; set; }
      public bool IsDirectory => Type == "dir" || Type == "parent";
      public string Name { get; set; }
      public string Info { get; set; }
      public string Path { get; set; }
    };
  }
}
