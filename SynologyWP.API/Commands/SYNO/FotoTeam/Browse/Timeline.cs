using System.Collections.Generic;

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

  public class Item
  {
    public int day;
    public int item_count;
    public int month;
    public int year;
  }

  public class Section
  {
    public List<Item> list;
    public int limit;
    public int offset;
  }

  public class TimelineGetResult : IResult
  {
    public List<Section> section;
  }
}
