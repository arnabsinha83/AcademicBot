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
            string url = string.Format("https://api.projectoxford.ai/academic/v1.0/evaluate?expr={0}&model=latest&count={1}&offset={2}&attributes=Ti,Y,CC,AA.AuN,AA.AuId",
                                            expr, count, offset);

            // Get the Json response
            string result = GetJsonResponse(url);

            // Print the json in the command prompt
            //Console.Write(result);

            // Deserialize the json and get the EvaluateModel.Rootobject
            EvaluateModel.Rootobject obj = JsonConvert.DeserializeObject<EvaluateModel.Rootobject>(result);

            // Return the structured query expression required for evaluate method 
            //return obj.interpretations[0].rules[0].output.value; // "Composite(AA.AuN=='arnab sinha')"	
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