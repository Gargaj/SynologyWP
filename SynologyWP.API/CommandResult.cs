using Newtonsoft.Json;
using System.Collections.Generic;

namespace SynologyWP.API
{
  public interface IResult
  {
  }

  public class CommandResultError
  {
    public int code;
    public List<object> errors;
  };

  public class CommandResult<T> where T : IResult
  {
    public bool success;
    public CommandResultError error;
    public T data;
  }
}
