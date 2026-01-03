using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.Core.DSMNotify
{
  public class Strings : ICommand
  {
    public string APIName => "SYNO.Core.DSMNotify.Strings";
    public int APIVersion => 1;
    public string APIMethod => "get";

    [QueryParameter]
    public string lang { get; set; }
  }

  public class Entry
  {
    public string entry;
    public string msg;
    public string title;
  }

  public class StringsResult : Dictionary<string, Entry>, IResult
  {
  }
}
