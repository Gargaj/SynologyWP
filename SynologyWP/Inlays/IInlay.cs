using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynologyWP.Inlays
{
  public interface IInlay
  {
    void Flush();
    Task Refresh();
  }
}