using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Comms.EMail
{
    public interface IEmailSenderService
    {
        Task Send(string to, string subject, string body);
    }
}