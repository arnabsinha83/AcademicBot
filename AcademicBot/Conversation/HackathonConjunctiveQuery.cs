namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;

    public sealed class HackathonConjunctiveQuery : IConjunctiveQuery
    {
        public  Dictionary<string, HashSet<Predicate>> ValueMap { get; }
        public Dictionary<PredicateType, HashSet<Predicate>> TypeMap { get; }

        private int numAmbiguousPredicates;

        public HackathonConjunctiveQuery()
        {
            this.ValueMap = new Dictionary<string, HashSet<Predicate>>();
            this.TypeMap = new Dictionary<PredicateType, HashSet<Predicate>>();
        }

        public HackathonConjunctiveQuery(List<Predicate> predicates)
        {
            //  The key is the user supplied query like "albert einstein"
            this.ValueMap = new Dictionary<string, HashSet<Predicate>>();
            this.TypeMap = new Dictionary<PredicateType, HashSet<Predicate>>();

            foreach (Predicate p in predicates)
            {
                this.AddPredicate(p);
            }
        }

        public void AddPredicate(Predicate predicate)
        {
            if (ValueMap.ContainsKey(predicate.Value))
            {
                HashSet<Predicate> set;
                ValueMap.TryGetValue(predicate.Value, out set);

                if (set.Contains(predicate))
                {
                    // do nothing
                }
                else
                {
                    set.Add(predicate);
                    if(set.Count == 2)
                        this.numAmbiguousPredicates++;
                }
            }
            else
            {
                HashSet<Predicate> set = new HashSet<Predicate>();
                set.Add(predicate);
                ValueMap.Add(predicate.Value, set);
            }

            if (TypeMap.ContainsKey(predicate.Type))
            {
                HashSet<Predicate> set;
                TypeMap.TryGetValue(predicate.Type, out set);

                if (set.Contains(predicate))
                {
                    // do nothing
                }
                else
                {
                    set.Add(predicate);
                    if(set.Count == 2)
                        this.numAmbiguousPredicates++;
                }
            }
            else
            {
                HashSet<Predicate> set = new HashSet<Predicate>();
                set.Add(predicate);
                TypeMap.Add(predicate.Type, set);
            }
        }

        public Dictionary<string, HashSet<Predicate>> GetAmbiguousPredicates()
        {
            //TODO: A very naive implementation. Needs to be made more efficient. 

            Dictionary<string, HashSet<Predicate>> ambMap = new Dictionary<string, HashSet<Predicate>>();

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.ValueMap)
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

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.ValueMap)
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
            if (ValueMap.ContainsKey(predicate.Value))
            {
                HashSet<Predicate> set;
                this.ValueMap.TryGetValue(predicate.Value, out set);
                set.Remove(predicate);

                if (set.Count == 1)
                {
                    this.numAmbiguousPredicates--;
                }
                else if(set.Count == 0)
                {
                    this.ValueMap.Remove(predicate.Value);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format("Can't remove a non-existent predicate {0}", predicate.ToString()));
            }

            if (this.TypeMap.ContainsKey(predicate.Type))
            {
                HashSet<Predicate> set;
                TypeMap.TryGetValue(predicate.Type, out set);
                set.Remove(predicate);

                if (set.Count == 1)
                {
                    this.numAmbiguousPredicates--;
                }
                else if(set.Count == 0)
                {
                    this.TypeMap.Remove(predicate.Type);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format("Can't remove a non-existent predicate {0}", predicate.ToString()));
            }
        }
    }
}