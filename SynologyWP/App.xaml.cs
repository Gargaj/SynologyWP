using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SynologyWP
{
  public partial class App : Application
  {
    private API.Client _client = new API.Client();
    private Frame _rootFrame;
    public App()
    {
      InitializeComponent();
      Suspending += OnSuspending;
    }

    public API.Client Client => _client;

    protected async override void OnLaunched(LaunchActivatedEventArgs e)
    {
      _rootFrame = Window.Current.Content as Frame;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (_rootFrame == null)
      {
        _rootFrame = new Frame();

        _rootFrame.NavigationFailed += OnNavigationFailed;

        Window.Current.Content = _rootFrame;
      }

      if (e.PrelaunchActivated == false)
      {
        if (_rootFrame.Content == null)
        {
          if (!await _client.Authenticate())
          {
            NavigateToLoginScreen(e.Arguments);
          }
          else
          {
            NavigateToMainScreen(e.Arguments);
          }
        }
        Window.Current.Activate();
      }
    }

    public void NavigateToMainScreen(string arguments)
    {
      if (_rootFrame == null)
      {
        return;
      }

      _rootFrame.Navigate(typeof(Pages.MainPage), arguments);
    }

    public void NavigateToLoginScreen(string arguments)
    {
      if (_rootFrame == null)
      {
        return;
      }

      _rootFrame.Navigate(typeof(Pages.LoginPage), arguments);
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
      throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    public T GetCurrentFrame<T>() where T : Page
    {
      return _rootFrame.Content as T;
    }

    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();
      deferral.Complete();
    }
  }
}
