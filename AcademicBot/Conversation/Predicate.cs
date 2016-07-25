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
}