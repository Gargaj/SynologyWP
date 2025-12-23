using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace SynologyWP.API
{
  public class Client
  {
    private Settings _settings = new Settings();
    private string _hostOverride;
    private Dictionary<string, Commands.SYNO.API.APIDescription> _endpoints = null;

    private Newtonsoft.Json.JsonSerializerSettings _deserializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
      {
        MetadataPropertyHandling = Newtonsoft.Json.MetadataPropertyHandling.ReadAhead,
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        //TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects,
      };

    public Client()
    {
    }

    public Settings Settings => _settings;

    public async Task<bool> Authenticate()
    {
      return await _settings.ReadSettings();
    }

    public async Task<bool> AuthenticateWithPassword(string host, string username, string password)
    {
      if (string.IsNullOrEmpty(host)
          || string.IsNullOrEmpty(username)
          || string.IsNullOrEmpty(password))
      {
        throw new ArgumentException();
      }

      host = host.TrimEnd('/');
      _hostOverride = host;

      Commands.SYNO.API.AuthResult response = null;
      try
      {
        response = await GetAsync<Commands.SYNO.API.AuthResult>(new Commands.SYNO.API.Auth()
        {
          account = username,
          passwd = password,
          format = "sid",
        });
      }
      catch (WebException)
      {
        return false;
      }
      if (response == null || string.IsNullOrEmpty(response.sid))
      {
        return false;
      }

      _hostOverride = null;

      _settings.CurrentCredential.URL = host;
      _settings.CurrentCredential.SID = response.sid;

      return await _settings.WriteSettings();
    }

    public void SetEndpoints(Dictionary<string, Commands.SYNO.API.APIDescription> endpoints)
    {
      _endpoints = endpoints;
    }

    public bool HasEndpoint(string endpoint)
    {
      return _endpoints.ContainsKey(endpoint);
    }

    public async Task<T> GetAsync<T>(ICommand input) where T : IResult
    {
      return await RequestAsync<T>("GET", input);
    }

    public async Task<T> PostAsync<T>(ICommand input) where T : IResult
    {
      return await RequestAsync<T>("POST", input);
    }

    protected async Task<T> RequestAsync<T>(string method, ICommand input) where T : IResult
    {
      var http = new HTTP();

      var endpoint = (_endpoints != null && _endpoints.ContainsKey(input.APIName)) ? _endpoints[input.APIName].path : "entry.cgi";
      var url = (_hostOverride ?? Settings.CurrentCredential.URL) + "/webapi/" + endpoint;
      string responseJson = null;
      var headers = new NameValueCollection();
      headers.Add("Content-Type", "application/json; charset=utf-8");
      switch (method)
      {
        case "GET":
          {
            var queryParams = new NameValueCollection();
            queryParams.Add("api", input.APIName);
            queryParams.Add("version", input.APIVersion.ToString());
            queryParams.Add("method", input.APIMethod);
            foreach (PropertyInfo prop in input.GetType().GetProperties())
            {
              if (prop.GetCustomAttribute<QueryParameter>() != null)
              {
                queryParams.Add(prop.Name, prop.GetValue(input).ToString());
              }
            }
            if (!string.IsNullOrEmpty(Settings.CurrentCredential.SID))
            {
              queryParams.Add("_sid", Settings.CurrentCredential.SID);
            }

            responseJson = await http.DoGETRequestAsync(url, queryParams, headers);
          }
          break;
      }

      if (responseJson == null)
      {
        return default(T);
      }
      var result = Newtonsoft.Json.JsonConvert.DeserializeObject<CommandResult<T>>(responseJson, _deserializerSettings);
      return result.data;
    }
  }
}
