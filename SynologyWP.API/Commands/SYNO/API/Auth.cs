namespace SynologyWP.API.Commands.SYNO.API
{
  public class Auth : ICommand
  {
    public string APIName => "SYNO.API.Auth";
    public int APIVersion => 6;
    public string APIMethod => "login";

    [QueryParameter]
    public string account { get; set; }
    [QueryParameter]
    public string passwd { get; set; }
    [QueryParameter]
    public string format { get; set; }
  }

  public class AuthResult : IResult
  {
    public string did;
    public bool is_portal_port;
    public string sid;
    public string synotoken;
  }
}
