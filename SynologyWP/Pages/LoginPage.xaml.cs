using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SynologyWP.Pages
{
  public partial class LoginPage : Page, INotifyPropertyChanged
  {
    private App _app;
    private string _host = "";
    private string _handle = string.Empty;
    private string _password = string.Empty;
    private string _params = string.Empty;

    public LoginPage()
    {
      InitializeComponent();
      _app = (App)Windows.UI.Xaml.Application.Current;
      DataContext = this;

      OnPropertyChanged(nameof(Host));
      OnPropertyChanged(nameof(Handle));
      OnPropertyChanged(nameof(Password));
    }

    public string Host { get { return _host; } set { _host = value; OnPropertyChanged(nameof(Host)); } }
    public string Handle { get { return _handle; } set { _handle = value; OnPropertyChanged(nameof(Handle)); } }
    public string Password { get { return passwordBox.Password; } }

    public event PropertyChangedEventHandler PropertyChanged;

    private async void Login_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      if (await _app.Client.AuthenticateWithPassword(Host, Handle, Password))
      {
        _app.NavigateToMainScreen(_params);
      }
      else
      {
        var dialog = new ContentDialog
        {
          Content = new TextBlock { Text = $"Login failed!" },
          Title = $"Login failed!",
          PrimaryButtonText = "Ok :(",
        };
        await dialog.ShowAsync();
      }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      _params = e.Parameter as string;
      base.OnNavigatedTo(e);
    }

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
