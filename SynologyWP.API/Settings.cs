using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynologyWP.API
{
  public class Settings
  {
    private const string _settingsFilename = "settings.dat";
    private Credential _credential = new Credential();

    public Settings()
    {
    }

    public Credential CurrentCredential => _credential;

    public async Task<bool> ReadSettings()
    {
      try
      {
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        var provider = new Windows.Security.Cryptography.DataProtection.DataProtectionProvider();

        var file = await localFolder.GetFileAsync(_settingsFilename);
        var buffProtected = await Windows.Storage.FileIO.ReadBufferAsync(file);

        var buffUnprotected = await provider.UnprotectAsync(buffProtected);
        var strClearText = Windows.Security.Cryptography.CryptographicBuffer.ConvertBinaryToString(Windows.Security.Cryptography.BinaryStringEncoding.Utf8, buffUnprotected);

        Newtonsoft.Json.JsonConvert.PopulateObject(strClearText,this);

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public async Task<bool> WriteSettings()
    {
      try
      {
        var str = Newtonsoft.Json.JsonConvert.SerializeObject(this);

        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        var provider = new Windows.Security.Cryptography.DataProtection.DataProtectionProvider("LOCAL=user");

        var buffMsg = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(str, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
        var buffProtected = await provider.ProtectAsync(buffMsg);

        var file = await localFolder.CreateFileAsync(_settingsFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
        await Windows.Storage.FileIO.WriteBufferAsync(file, buffProtected);

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public class Credential
    {
      public string URL { get; set; }
      public string SID { get; set; }
    }
  }
}
