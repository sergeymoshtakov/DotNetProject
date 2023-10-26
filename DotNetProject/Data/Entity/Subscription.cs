using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Data.Entity
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid Subscriber { get; set; }
        public Guid Owner { get; set; }
    }
}
