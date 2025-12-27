using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FileStation
{
  public class BackgroundTaskList : ICommand
  {
    public string APIName => "SYNO.FileStation.BackgroundTask";
    public int APIVersion => 3;
    public string APIMethod => "list";

    [QueryParameter]
    public bool is_list_sharemove { get; set; }
    [QueryParameter]
    public bool is_vfs { get; set; }
    [QueryParameter]
    public bool bkg_info { get; set; }
  }

  public class Task
  {
    public string id;
    public ulong size;
    public int status;
    public string title;
    public string type;
    public string username;
  }

  public class BackgroundTaskListResult : IResult
  {
    public int total;
    public int offset;
    public List<Task> Tasks;
  }

}
