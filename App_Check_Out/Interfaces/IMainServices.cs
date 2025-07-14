using APP_CHECKOUT.Models.Models.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Interfaces
{
    public interface IMainServices
    {
        public Task Excute(CheckoutQueueModel request);

    }
}
