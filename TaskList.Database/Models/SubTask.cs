using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Database.Models
{
    public class SubTask
    {
        public string Name { get; set; }

        public SubTask(string name)
        {
            Name = name;
        }
    }
}
