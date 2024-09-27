using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Rules
{
    public class BaseRule
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public BaseRule(string name, string description)
        {

            Name = name;
            Description = description;
        }
    }
}
