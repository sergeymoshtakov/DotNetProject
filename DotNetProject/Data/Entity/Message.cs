using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Data.Entity
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
        public Guid SenderId { get; set; }
        public DateTime SendDT { get; set; }
        public User Sender { get; set; }
    }
}
