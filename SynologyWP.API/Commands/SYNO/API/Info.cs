using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.API
{
  public struct APIDescription
  {
    public string key;
    public string path;
    public int? minVersion;
    public int? maxVersion;
    public string requestFormat;
  }

  public class Info : ICommand
  {
    public string APIName => "SYNO.API.Info";
    public int APIVersion => 1;
    public string APIMethod => "query";
  }

  public class InfoResult : Dictionary<string, APIDescription>, IResult
  {
  }
}
