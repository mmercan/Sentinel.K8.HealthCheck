using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Comms.EMail.SendGrid
{
    public class SendGridMailServiceSettings
    {
        public string Key { get; set; } = default!;
        public string From { get; set; } = default!;
    }
}