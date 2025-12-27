using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.Core
{
  public class SystemPoll : ICommand
  {
    public string APIName => "SYNO.Core.System";
    public int APIVersion => 1;
    public string APIMethod => "poll";

    [QueryParameter]
    public string type { get; set; }
  }

  public class VolumeInfo
  {
    public string desc;
    public ulong inode_free;
    public ulong inode_total;
    public bool is_encrypted;
    public string name;
    public string status;
    public ulong total_size;
    public ulong used_size;
    public string vol_desc;
    public string volume;
  }

  public class SystemPollResult : IResult
  {
    public List<VolumeInfo> vol_info;
  }

}
