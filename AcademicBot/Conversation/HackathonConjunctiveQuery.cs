namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;

    public sealed class HackathonConjunctiveQuery : IConjunctiveQuery
    {
        private readonly Dictionary<string, HashSet<Predicate>> valueMap;
        private readonly Dictionary<PredicateType, HashSet<Predicate>> typeMap;

        private int numAmbiguousPredicates;

        public HackathonConjunctiveQuery()
        {
            this.valueMap = new Dictionary<string, HashSet<Predicate>>();
            this.typeMap = new Dictionary<PredicateType, HashSet<Predicate>>();
        }

        public HackathonConjunctiveQuery(List<Predicate> predicates)
        {
            //  The key is the user supplied query like "albert einstein"
            this.valueMap = new Dictionary<string, HashSet<Predicate>>();
            this.typeMap = new Dictionary<PredicateType, HashSet<Predicate>>();

            foreach (Predicate p in predicates)
            {
                this.AddPredicate(p);
            }
        }

        public void AddPredicate(Predicate predicate)
        {
            if (valueMap.ContainsKey(predicate.Value))
            {
                HashSet<Predicate> set;
                valueMap.TryGetValue(predicate.Value, out set);

                if (set.Contains(predicate))
                {
                    // do nothing
                }
                else
                {
                    set.Add(predicate);
                    this.numAmbiguousPredicates++;
                }
            }
            else
            {
                HashSet<Predicate> set = new HashSet<Predicate>();
                set.Add(predicate);
                valueMap.Add(predicate.Value, set);
            }

            if (typeMap.ContainsKey(predicate.Type))
            {
                HashSet<Predicate> set;
                typeMap.TryGetValue(predicate.Type, out set);

                if (set.Contains(predicate))
                {
                    // do nothing
                }
                else
                {
                    set.Add(predicate);
                    this.numAmbiguousPredicates++;
                }
            }
            else
            {
                HashSet<Predicate> set = new HashSet<Predicate>();
                set.Add(predicate);
                typeMap.Add(predicate.Type, set);
            }
        }

        public Dictionary<string, HashSet<Predicate>> GetAmbiguousPredicates()
        {
            //TODO: A very naive implementation. Needs to be made more efficient. 

            Dictionary<string, HashSet<Predicate>> ambMap = new Dictionary<string, HashSet<Predicate>>();

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.valueMap)
            {
                if (entry.Value.Count > 1)
                {
                    ambMap.Add(entry.Key, entry.Value);
                }
            }

            return ambMap;
        }

        public List<Predicate> GetAllPredicates()
        {
            Dictionary<string, HashSet<Predicate>> ambMap = new Dictionary<string, HashSet<Predicate>>();
            List<Predicate> list = new List<Predicate>();

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.valueMap)
            {
                foreach (Predicate p in entry.Value)
                {
                    list.Add(p);
                }
            }

            return list;
        }

        public bool IsAmbiguous()
        {
            return this.numAmbiguousPredicates > 0;
        }

        public void RemovePredicate(Predicate predicate)
        {
            if (valueMap.ContainsKey(predicate.Value))
            {
                HashSet<Predicate> set;
                valueMap.TryGetValue(predicate.Value, out set);
                set.Remove(predicate);

                if (set.Count == 0)
                {
                    this.numAmbiguousPredicates--;
                    valueMap.Remove(predicate.Value);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format("Can't remove a non-existent predicate {0}", predicate.ToString()));
            }
        }
    }
}