using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Messages
{
   public class GotoPageMessage
    {
       public GotoPageMessage(Pages gotoPage)
       {
           GotoPage = gotoPage;
       }

        public Pages GotoPage { get; set; }
    }
}
