using System;
using System.Text;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using AcademicBot.Conversation;
using System.Collections.Generic;

namespace AcademicBot.Controllers
{
    public static class AcademicApi
    {
        private static string GetJsonResponse(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";

            // Set the key
            webrequest.Headers.Add("Ocp-Apim-Subscription-Key", "55c6a7c69639485ca7f954b5214b53fc");

            // Return the response
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

            // Get the json response
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            string result = (new StreamReader(webresponse.GetResponseStream(), enc)).ReadToEnd();

            // Close the webresponse
            webresponse.Close();

            // Return the json string
            return result;
        }

        // Reference: https://dev.projectoxford.ai/docs/services/56332331778daf02acc0a50b/operations/56332331778daf06340c9666
        #region Interpret method
        public static List<Predicate> CallInterpretMethod(string query,
                                                          int complete = 1,    // autosuggestions are turned on by default
                                                          int count = 5,       // default is five interpretations 
                                                          int offset = 0)
        {
            string url = string.Format("https://api.projectoxford.ai/academic/v1.0/interpret?query={0}&complete={1}&count={2}&offset={3}",
                                            query, complete, count, offset);

            // Get the Json response
            string result = GetJsonResponse(url);

            // Deserialize the json and get the InterpretModel.Rootobject
            InterpretModel.Rootobject obj = JsonConvert.DeserializeObject<InterpretModel.Rootobject>(result);
            return Utilities.GetPredicateList(obj);
        }
        #endregion

        // Reference: https://dev.projectoxford.ai/docs/services/56332331778daf02acc0a50b/operations/565d753be597ed16ac3ffc03
        #region Evaluate method
        public static string CallEvaluateMethod(string expr,
                                                int count = 10,
                                                int offset = 0)
        {
            string entityAttributes = "Id,Ti,Y,CC,AA.AuN,AA.AuId,AA.AfN,AA.AfId,F.FN,F.FId,J.JId,J.JN,C.CN,C.CId,E";
            string url = string.Format("https://api.projectoxford.ai/academic/v1.0/evaluate?expr={0}&model=latest&count={1}&offset={2}&attributes={3}",
                                            expr, count, offset, entityAttributes);

            // Get the Json response
            string result = GetJsonResponse(url);

            return result;
        }
        #endregion
    }

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
            public long Id { get; set; }
            public string Ti { get; set; }
            public int Y { get; set; }
            public int CC { get; set; }
            public string E { get; set; }
            public AA[] AA { get; set; }
            public J J { get; set; }
            public C C { get; set; }
            public F[] F { get; set; } 
        }

        public class F
        {
            public string FN { get; set; }
            public long FId { get; set; }
        }

        public class J
        {
            public string JN { get; set; }
            public long JId { get; set; }
        }

        public class C
        {
            public string CN { get; set; }
            public long CId { get; set; }
        }

        public class AA
        {
            public string AuN { get; set; }
            public long AuId { get; set; }
            public string AfN { get; set; }
            public long AfId { get; set; }
        }

    }

    public class ExtendedAttributesModel
    {
        public class Rootobject
        {
            public string DN { get; set; }
            public S[] S { get; set; }
            public string VFN { get; set; }
            public string VSN { get; set; }
        }

        public class S
        {
            public int Ty { get; set; }
            public string U { get; set; }
        }
    }
}