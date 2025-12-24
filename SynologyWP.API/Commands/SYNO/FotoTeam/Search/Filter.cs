using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FotoTeam.Search
{
  public class FilterList : ICommand
  {
    public string APIName => "SYNO.FotoTeam.Search.Filter";
    public int APIVersion => 3;
    public string APIMethod => "list";

    [QueryParameter]
    public string additional { get; set; }
    [QueryParameter]
    public string setting { get; set; }
  }

  public class ItemType
  {
    public int id;
    public string name;
  }

  public class Geocoding
  {
    public int id;
    public int level;
    public string name;
    public List<Geocoding> children;
  }

  public class Time
  {
    public int year;
    public int month;
    public ulong start_time;
    public ulong end_time;
  }

  public class FilterListResult : IResult
  {
    public List<object> aperture;
    public List<object> camera;
    public List<object> exposure_time_group;
    public List<object> flash;
    public List<object> focal_length_group;
    public List<object> folder_filter;
    public List<object> general_tag;
    public List<Geocoding> geocoding;
    public List<object> iso;
    public List<ItemType> item_type;
    public List<object> lens;
    public List<object> person;
    public List<int> rating;
    public List<Time> time;
  }
}
