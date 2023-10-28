using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Data.Entity
{
    public class User
    {
        public Guid Id { get; set; }
        public String Surname { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Email { get; set; } = null!;
        public String? Password { get; set; }
        public String? ConfirmationCode { get; set; }
        public String? Status { get; set; }
        public Guid? IdGender { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? DeleteDt { get; set; }
        public DateTime Birthday { get; set; }
        public String? Avatar { get; set; }

        public Gender? Gender { get; set; }
    }
}
