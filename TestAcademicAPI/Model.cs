using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAcademicAPI
{
    public class InterpretModel
    {
        public class Rootobject
        {
            public string query { get; set; }
            public Interpretation[] interpretations { get; set; }
        }

        public class Interpretation
        {
            public float logprob { get; set; }
            public string parse { get; set; }
            public Rule[] rules { get; set; }
        }

        public class Rule
        {
            public string name { get; set; }
            public Output output { get; set; }
        }

        public class Output
        {
            public string type { get; set; }
            public string value { get; set; }
        }
    }

    public class EvaluateModel
    {
        public class Rootobject
        {
            public string expr { get; set; }
            public Entity[] entities { get; set; }
        }

        public class Entity
        {
            public float logprob { get; set; }
            public string Ti { get; set; }
            public int Y { get; set; }
            public int CC { get; set; }
            public AA[] AA { get; set; }
        }

        public class AA
        {
            public string AuN { get; set; }
            public long AuId { get; set; }
        }
    }
}
