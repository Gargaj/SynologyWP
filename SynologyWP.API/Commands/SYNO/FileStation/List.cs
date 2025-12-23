using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FileStation
{
  // list_share

  public class ListShare : ICommand
  {
    public string APIName => "SYNO.FileStation.List";
    public int APIVersion => 2;
    public string APIMethod => "list_share";
  }

  public class SharedFolder
  {
    public string path;
    public string name;
  }

  public class ListShareResult : IResult
  {
    public int total;
    public int offset;
    public List<SharedFolder> shares;
  }

  // list

  public class List : ICommand
  {
    public string APIName => "SYNO.FileStation.List";
    public int APIVersion => 2;
    public string APIMethod => "list";

    [QueryParameter]
    public string folder_path { get; set; }
    [QueryParameter]
    public string additional { get; set; }
  }

  public class FileTime
  {
    public ulong atime;
    public ulong crtime;
    public ulong ctime;
    public ulong mtime;
  }

  public class FileAdditional
  {
    public string real_path;
    public ulong size;
    public FileTime time;
    public string type;
  }

  public class File
  {
    public string path;
    public string name;
    public bool isdir;
    public FileAdditional additional;
  }

  public class ListResult : IResult
  {
    public int total;
    public int offset;
    public List<File> files;
  }

}
