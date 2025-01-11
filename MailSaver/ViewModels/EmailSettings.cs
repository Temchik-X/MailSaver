using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailSaver.ViewModels
{
    public class EmailSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string SenderMask { get; set; }
        public string? TopicMask { get; set; }
        public string? MessageMask { get; set; }
    }
    public class PathConfiguration
    {
        public string Path { get; set; }
    }
}
