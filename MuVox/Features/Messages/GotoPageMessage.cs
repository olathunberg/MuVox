using System;

namespace TTech.MuVox.Features.Messages
{
    public class GotoPageMessage
    {
       public GotoPageMessage(Pages gotoPage)
       {
           GotoPage = gotoPage;
       }

        public Pages GotoPage { get; private set; }
    }
}
