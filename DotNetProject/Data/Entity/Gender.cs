using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Data.Entity
{
    public class Gender
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public List<User>? Users { get; set; }
    }
}
