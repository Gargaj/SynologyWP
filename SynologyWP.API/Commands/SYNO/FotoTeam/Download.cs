using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FotoTeam
{
  public class Download : ICommand
  {
    public string APIName => "SYNO.FotoTeam.Download";
    public int APIVersion => 2;
    public string APIMethod => "download";

    [QueryParameter]
    public string item_id { get; set; }
  }
}
