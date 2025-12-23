namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class ArtistList : ICommand
  {
    public string APIName => "SYNO.AudioStation.Artist";
    public int APIVersion => 4;
    public string APIMethod => "list";
  }

  public class ArtistListResult : IResult
  {
  }
}
