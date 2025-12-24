using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FileStation
{
  public class Download : ICommand
  {
    public string APIName => "SYNO.FileStation.Download";
    public int APIVersion => 2;
    public string APIMethod => "download";

    [QueryParameter]
    public string path { get; set; }
    [QueryParameter]
    public string mode { get; set; }
  }
}
