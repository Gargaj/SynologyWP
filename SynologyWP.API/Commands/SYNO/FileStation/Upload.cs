using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.FileStation
{
  public class Upload : ICommand
  {
    public string APIName => "SYNO.FileStation.Upload";
    public int APIVersion => 2;
    public string APIMethod => "upload";
  }

  public class UploadResult : IResult
  {
  }
}
