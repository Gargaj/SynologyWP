using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class CoverGetCover : ICommand
  {
    public string APIName => "SYNO.AudioStation.Cover";
    public int APIVersion => 3;
    public string APIMethod => "getcover";

    [QueryParameter]
    public bool output_default { get; set; }
    [QueryParameter]
    public bool is_hr { get; set; }
    [QueryParameter]
    public string library { get; set; }
    [QueryParameter]
    public string view { get; set; }
    [QueryParameter]
    public string artist_name { get; set; }
    [QueryParameter]
    public string album_artist_name { get; set; }
    [QueryParameter]
    public string album_name { get; set; }
  }

  public class CoverGetSongCover : ICommand
  {
    public string APIName => "SYNO.AudioStation.Cover";
    public int APIVersion => 3;
    public string APIMethod => "getsongcover";

    [QueryParameter]
    public bool output_default { get; set; }
    [QueryParameter]
    public bool is_hr { get; set; }
    [QueryParameter]
    public string library { get; set; }
    [QueryParameter]
    public string view { get; set; }
    [QueryParameter]
    public string id { get; set; }
  }
}
