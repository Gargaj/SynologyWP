using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.Core.DSMNotify
{
  public class Notify : ICommand
  {
    public string APIName => "SYNO.Core.DSMNotify";
    public int APIVersion => 1;
    public string APIMethod => "notify";

    [QueryParameter]
    public string action { get; set; }
    [QueryParameter]
    public string level { get; set; }
    [QueryParameter]
    public int? limit { get; set; }
    [QueryParameter]
    public int? offset { get; set; }
  }

  public class Notification
  {
    public bool bindEvt;
    public string className;
    public List<object> fn;
    public bool hasMail;
    public bool isEncoded;
    public string level;
    public string mailType;
    public List<string> msg;
    public int notifyId;
    public string tag;
    public ulong time;
    public string title;
  }

  public class NotifyResult : IResult
  {
    public int total;
    public ulong newestMsgTime;
    public List<Notification> items;
  }

}
