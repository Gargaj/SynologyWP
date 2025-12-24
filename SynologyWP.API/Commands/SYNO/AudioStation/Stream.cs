using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class Stream : ICommand
  {
    public string APIName => "SYNO.AudioStation.Stream";
    public int APIVersion => 2;
    public string APIMethod => "stream";

    [QueryParameter]
    public string id { get; set; }
  }
}
