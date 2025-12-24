using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FotoTeam.Browse
{
  public class ItemList : ICommand
  {
    public string APIName => "SYNO.FotoTeam.Browse.Item";
    public int APIVersion => 4;
    public string APIMethod => "list";

    [QueryParameter]
    public int offset { get; set; }
    [QueryParameter]
    public int limit { get; set; }
    [QueryParameter]
    public ulong start_time { get; set; }
    [QueryParameter]
    public ulong end_time { get; set; }
    [QueryParameter]
    public string additional { get; set; }
  }

  public class Resolution
  {
    public int width;
    public int height;
  }

  public class Thumbnail
  {
    public string sm;
    public string m;
    public string xl;
    public string preview;
    public string cache_key;
    public string unit_id;
  }

  public class Addditional
  {
    public Resolution resolution;
    public int orientation;
    public int orientation_original;
    public Thumbnail thumbnail;
    public object address;
    public object video_meta;
    public object video_convert;
    public object video_convert_status;
  };

  public class Entry
  {
    public int id;
    public string filename;
    public ulong filesize;
    public ulong time;
    public ulong indexed_time;
    public int owner_user_id;
    public int folder_id;
    public string type;
    public Addditional additional;
  };

  public class ItemListResult : IResult
  {
    public List<Entry> list;
  }
}
