namespace SynologyWP.API
{
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class QueryParameter : System.Attribute
  {
  }

  public interface ICommand
  {
    string APIName { get; }
    int APIVersion { get; }
    string APIMethod { get; }
  }
}
