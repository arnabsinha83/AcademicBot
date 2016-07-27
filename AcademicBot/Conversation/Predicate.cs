namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using AcademicBot.Controllers;

    public class Predicate
    {
        public PredicateType Type { get; private set; }
        public string Value { get; private set; }
        public double Confidence { get; private set; } // between 0 and 1
        public OperationType OperationType { get; private set; }
        public string StructuredQuery { get; private set; }
        public Predicate(PredicateType type, string value, string structuredQuery, double confidence = 1.0, OperationType opType = OperationType.EQ)
        {
            if (type < PredicateType.Affiliation || type > PredicateType.PublicationYear)
            {
                throw new ArgumentOutOfRangeException(String.Format("Predicate Type {0} not understood", type));
            }

            if (value == null)
            {
                throw new ArgumentNullException(String.Format("Value of a predicate can't be null"));
            }

            this.Type = type;
            this.Value = value;
            this.Confidence = confidence;
            this.OperationType = opType;
            this.StructuredQuery = structuredQuery;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Predicate target = obj as Predicate;

            return (this.OperationType == target.OperationType) && (this.Value.Equals(target.Value));
        }

        public override int GetHashCode()
        {
            return this.OperationType.GetHashCode() + this.Value.GetHashCode();
        }
    }

    public static class Utilities
    {
        private static 
        Dictionary<string, PredicateType> PredicateTypeMap = new Dictionary<string, PredicateType>()
                                                             {
                                                                { "AA.AuN", PredicateType.AuthorName },
                                                                { "AA.AfN", PredicateType.Affiliation },
                                                                { "F.FN", PredicateType.FieldOfStudy },
                                                                { "C.CN", PredicateType.PaperVenue },
                                                                { "J.JN", PredicateType.PaperVenue },
                                                                { "Ti", PredicateType.PaperTitle },
                                                                // FIXIT - add more mappings

                                                             };

        private static 
        Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>()
                                                             {
                                                                { ">", OperationType.GT },
                                                                { "<", OperationType.LT },
                                                                { "=", OperationType.EQ },
                                                             };

        private static Regex PredicateCompositeRegex = new Regex(@"Composite\([^\)]*\)", RegexOptions.Compiled);
        private static Regex PredicateYearRegex = new Regex(@"Y[\>\<\=]\d{4}", RegexOptions.Compiled);

        #region Construct the conjunction of predicates from a list of predicates
        public static string GetPredicateConjunction(List<Predicate> predicateList)
        {
            string reply = predicateList[0].StructuredQuery;
            for(int i=1; i<predicateList.Count; i++)
            {
                reply = string.Format("AND({0},{1})", reply, predicateList[i].StructuredQuery);
            }
            return reply;
        }
        #endregion

        #region Get the array of predicates from the interpretations
        public static List<Conversation.Predicate> GetPredicateList(InterpretModel.Rootobject obj)
        {
            int interpretationCount = obj.interpretations.Length;
            List<Conversation.Predicate> predicateList = new List<Conversation.Predicate>();
            foreach (var interpretation in obj.interpretations)
            {
                foreach (var r in interpretation.rules)
                {
                    string structuredQuery = r.output.value;

                    #region Commented out
                    // Note: Ignore structured queries when there are multiple interpretations
                    // and there are more than one Composite for now. Think about it later. However, 
                    // we are dealing with queries like "sharad malik about boolean satisfiability" and
                    // "nips 2010". Note that we cannot serve queries like "princeton university laser"
                    // right now
                    //if ((PredicateCompositeRegex.Matches(structuredQuery).Count > 1) &&
                    //    (interpretationCount > 1))
                    //{
                    //    continue;
                    //}
                    #endregion

                    #region Deal with predicates like "Composite(AA.AuN == 'albert einstein')"
                    foreach (Match match in PredicateCompositeRegex.Matches(structuredQuery))
                    {
                        PredicateType predType = PredicateType.Unknown;
                        Predicate pred = null;

                        string leafPredicate = match.Value;
                        int prefixLength = "Composite(".Length;
                        string attributeValueStr = leafPredicate.Substring(prefixLength, leafPredicate.Length - prefixLength - 2);
                        string[] attributeValue = attributeValueStr.Split(new string[] { "=='" }, StringSplitOptions.RemoveEmptyEntries);

                        if (PredicateTypeMap.ContainsKey(attributeValue[0]))
                        {
                            predType = PredicateTypeMap[attributeValue[0]];
                        }
                        pred = new Predicate(predType,                // PredicateType
                                             attributeValue[1],       // Value
                                             leafPredicate,           // StructuredQuery  
                                             interpretation.logprob,  // Confidence 
                                             OperationType.EQ         // OperationType 
                                            );
                        predicateList.Add(pred);
                    }
                    #endregion

                    #region Deal with predicates like "Y>2007"
                    foreach (Match match in PredicateYearRegex.Matches(structuredQuery))
                    {
                        PredicateType predType = PredicateType.Unknown;
                        OperationType opType = OperationType.NOP;
                        Predicate pred = null;

                        string leafPredicate = match.Value;
                        predType = PredicateType.PublicationYear;
                        string operatorStr = leafPredicate.Substring(1, 1);
                        if (OperationTypeMap.ContainsKey(operatorStr))
                        {
                            opType = OperationTypeMap[operatorStr];
                        }
                        pred = new Predicate(predType,                        // PredicateType
                                             leafPredicate.Substring(2),      // Value
                                             leafPredicate,                   // StructuredQuery
                                             interpretation.logprob,          // Confidence 
                                             opType                           // OperationType 
                                            );
                        predicateList.Add(pred);
                    }
                    #endregion

                }
            }
            return predicateList;
        }
        #endregion
    }
}