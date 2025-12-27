using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.Entry
{
  public class Request : ICommand
  {
    public string APIName => "SYNO.Entry.Request";
    public int APIVersion => 1;
    public string APIMethod => "request";

    [QueryParameter]
    public bool stop_when_error { get; set; }
    [QueryParameter]
    public string mode { get; set; }
    [QueryParameter]
    public string compound { get; set; }
  }

  public class RequestResult<T> : IResult where T : IResult
  {
    public bool has_fail;
    public List<CommandResult<T>> result;
  }
}
