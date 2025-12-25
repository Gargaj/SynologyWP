using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class AlbumList : ICommand
  {
    public string APIName => "SYNO.AudioStation.Album";
    public int APIVersion => 3;
    public string APIMethod => "list";

    [QueryParameter]
    public string artist { get; set; }
    [QueryParameter]
    public string sort_by { get; set; }
    [QueryParameter]
    public string sort_direction { get; set; }
    [QueryParameter]
    public int? limit { get; set; }
  }

  public class Album
  {
    public string album_artist;
    public string artist;
    public string display_artist;
    public string name;
    public int year;
  }

  public class AlbumListResult : IResult
  {
    public List<Album> albums;
    public int offset;
    public int total;
  }
}
