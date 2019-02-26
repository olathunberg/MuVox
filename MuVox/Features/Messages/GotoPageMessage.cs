using System;

namespace TTech.Muvox.Features.Messages
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
