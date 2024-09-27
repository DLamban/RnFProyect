using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Rules
{
    public class Modifiers
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Attribute_Affected AttributeAffected { get; set; }
        public int Value { get; set; }
        public Modifiers(string name, string description, Attribute_Affected attributeAffected, int value)
        {
            Name = name;
            Description = description;
            AttributeAffected = attributeAffected;
            Value = value;
        }
    }
}
