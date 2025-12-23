namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class AlbumList : ICommand
  {
    public string APIName => "SYNO.AudioStation.Album";
    public int APIVersion => 3;
    public string APIMethod => "list";
  }

  public class AlbumListResult : IResult
  {
  }
}
