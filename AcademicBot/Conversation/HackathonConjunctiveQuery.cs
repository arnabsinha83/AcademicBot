namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;

    public sealed class HackathonConjunctiveQuery : IConjunctiveQuery
    {
        Dictionary<string, HashSet<Predicate>> map;
        int numAmbiguousPredicates;

        public HackathonConjunctiveQuery()
        {
            this.map = new Dictionary<string, HashSet<Predicate>>();
        }

        public HackathonConjunctiveQuery(List<Predicate> predicates)
        {
            //  The key is the user supplied query like "albert einstein"
            this.map = new Dictionary<string, HashSet<Predicate>>();

            foreach (Predicate p in predicates)
            {
                this.AddPredicate(p);
            }
        }

        public void AddPredicate(Predicate predicate)
        {
            if (map.ContainsKey(predicate.value))
            {
                HashSet<Predicate> set;
                map.TryGetValue(predicate.value, out set);

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
                map.Add(predicate.value, set);
            }
        }

        public Dictionary<string, HashSet<Predicate>> GetAmbiguousPredicates()
        {
            //TODO: A very naive implementation. Needs to be made more efficient. 

            Dictionary<string, HashSet<Predicate>> ambMap = new Dictionary<string, HashSet<Predicate>>();

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.map)
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

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in this.map)
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
            if (map.ContainsKey(predicate.value))
            {
                HashSet<Predicate> set;
                map.TryGetValue(predicate.value, out set);
                set.Remove(predicate);

                if (set.Count == 0)
                {
                    this.numAmbiguousPredicates--;
                    map.Remove(predicate.value);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format("Can't remove a non-existent predicate {0}", predicate.ToString()));
            }
        }
    }
}