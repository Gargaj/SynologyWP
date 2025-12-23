using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class ArtistList : ICommand
  {
    public string APIName => "SYNO.AudioStation.Artist";
    public int APIVersion => 4;
    public string APIMethod => "list";
  }

  public class Artist
  {
    public string name;
  }

  public class ArtistListResult : IResult
  {
    public List<Artist> artists;
    public int offset;
    public int total;
  }
}
