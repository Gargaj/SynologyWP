namespace SynologyWP.API.Commands.SYNO.FotoTeam.Browse
{
  public class TimelineGet : ICommand
  {
    public string APIName => "SYNO.FotoTeam.Browse.Timeline";
    public int APIVersion => 5;
    public string APIMethod => "get";

    [QueryParameter]
    public string timeline_group_unit { get; set; }
  }

  public class TimelineGetResult : IResult
  {
  }
}
