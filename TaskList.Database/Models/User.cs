using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Database.Models
{
    public class User
    {
        public User(string name,string password)
        {
            Name = name;
            Password = password;
        }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
