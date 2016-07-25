﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using AcademicBot.Controllers;

namespace AcademicBot.Conversation
{
    using System;

    public class Predicate
    {
        public PredicateType type { get; private set; }
        public string value { get; private set; }
        public double Confidence { get; private set; } // between 0 and 1
        public OperationType OperationType { get; private set; }

        public Predicate(PredicateType type, string value, double confidence = 1.0, OperationType opType = OperationType.EQ)
        {
            if (type < PredicateType.Affiliation || type > PredicateType.PublicationYear)
            {
                throw new ArgumentOutOfRangeException(String.Format("Predicate Type {0} not understood", type));
            }

            if (value == null)
            {
                throw new ArgumentNullException(String.Format("Value of a predicate can't be null"));
            }

            this.type = type;
            this.value = value;
            this.Confidence = confidence;
            this.OperationType = opType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Predicate target = obj as Predicate;

            return (this.OperationType == target.OperationType) && (this.value.Equals(target.value));
        }

        public override int GetHashCode()
        {
            return this.OperationType.GetHashCode() + this.value.GetHashCode();
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
                                                                // FIXIT - add more mappings

                                                             };

        private static 
        Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>()
                                                             {
                                                                { ">", OperationType.GT },
                                                                { "<", OperationType.LT },
                                                                // FIXIT - add "=="

                                                             };

        private static Regex PredicateRegex = new Regex(@"(Composite\([^\)]*\))|(Y[\>\<\=]\d{4})", RegexOptions.Compiled);

        #region Get the array of predicates from the interpretations
        public static List<Conversation.Predicate> GetPredicateList(InterpretModel.Rootobject obj)
        {
            List<Conversation.Predicate> predicateList = new List<Conversation.Predicate>();
            foreach (var interpretation in obj.interpretations)
            {
                foreach (var r in interpretation.rules)
                {
                    string structuredQuery = r.output.value;
                    // Note: Ignore structured queries where there are more than one Composite 
                    // for now. Think about it later.
                    if(PredicateRegex.Matches(structuredQuery).Count > 1)
                    {
                        continue;
                    }
                    foreach (Match match in PredicateRegex.Matches(structuredQuery))
                    {
                        PredicateType predType = PredicateType.Unknown;
                        OperationType opType = OperationType.NOP;
                        Predicate pred = null;

                        string leafPredicate = match.Value;
                        #region Deal with predicates like "Composite(AA.AuN == 'albert einstein')"
                        if (leafPredicate.Contains("Composite"))
                        {
                            int prefixLength = "Composite(".Length;
                            string attributeValueStr = leafPredicate.Substring(prefixLength, leafPredicate.Length - prefixLength - 2);
                            string[] attributeValue = attributeValueStr.Split(new string[] { "=='" }, StringSplitOptions.RemoveEmptyEntries);

                            if (PredicateTypeMap.ContainsKey(attributeValue[0]))
                            {
                                predType = PredicateTypeMap[attributeValue[0]];
                            }
                            pred = new Predicate(predType,                // PredicateType
                                                 attributeValue[1],       // Value
                                                 interpretation.logprob,  // Confidence 
                                                 OperationType.EQ         // OperationType 
                                                );
                            
                        }
                        #endregion
                        #region Deal with predicates like "Y>2007"
                        else
                        {
                            predType = PredicateType.PublicationYear;
                            string operatorStr = leafPredicate.Substring(1, 1);
                            if (OperationTypeMap.ContainsKey(operatorStr))
                            {
                                opType = OperationTypeMap[operatorStr];
                            }
                            pred = new Predicate(predType,                        // PredicateType
                                                 leafPredicate.Substring(2),      // Value
                                                 interpretation.logprob,          // Confidence 
                                                 opType                          // OperationType 
                                                );
                        }
                        #endregion
                        predicateList.Add(pred);
                    }
                }
            }
            return predicateList;
        }
        #endregion
    }
}