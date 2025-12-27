using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.Core.System
{
  public class SystemHealth : ICommand
  {
    public string APIName => "SYNO.Core.System.SystemHealth";
    public int APIVersion => 1;
    public string APIMethod => "poll";
  }

  public class Interface
  {
    public string id;
    public string ip;
    public string type;
  }

  public class Description
  {
    public string description_format;
    public List<string> description_params;
    public string description_use_formatted;
  }

  public class Rule
  {
    public string id;
    public int priority;
    public int type;
    public Description description;
  }

  public class SystemHealthResult : IResult
  {
    public string hostname;
    public string uptime;
    public List<Interface> interfaces;
    public Rule rule;
  }

}
