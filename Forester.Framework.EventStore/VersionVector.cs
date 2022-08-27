namespace Forester.Framework.EventStore
{
    public class VersionVector
    {
        public enum Comparison
        {
            AreSame,
            IsOlder,
            IsNewer,
            AreConcurrent
        }

        private readonly Dictionary<string, int> _value;

        public VersionVector()
        {
            _value = new Dictionary<string, int>();
        }

        public VersionVector(Dictionary<string, int> value)
        {
            _value = Copy(value);
        }

        public VersionVector Next(string node)
        {
            var copy = Copy(_value);

            if (!copy.ContainsKey(node))
            {
                copy[node] = 0;
            }

            copy[node]++;

            return new VersionVector(copy);
        }

        public Comparison ComparedTo(VersionVector another)
        {
            var a = Copy(_value);
            var b = Copy(another._value);

            var notInB = a.Where(kvp => !b.ContainsKey(kvp.Key)).ToList();
            var notInA = b.Where(kvp => !a.ContainsKey(kvp.Key)).ToList();

            // Empty value equals zero value
            notInA.ForEach(kvp => a.Add(kvp.Key, 0));
            notInB.ForEach(kvp => b.Add(kvp.Key, 0));

            // Check if all values are same.
            var areSame = a.All(kvp => b[kvp.Key] == kvp.Value);
            if (areSame)
            {
                return Comparison.AreSame;
            }

            var aIsAfterB = a.All(kvp => kvp.Value >= b[kvp.Key]);
            if (aIsAfterB)
            {
                return Comparison.IsNewer;
            }

            var bIsAfterA = b.All(kvp => kvp.Value >= a[kvp.Key]);
            if (bIsAfterA)
            {
                return Comparison.IsOlder;
            }

            return Comparison.AreConcurrent;
        }

        public VersionVector Copy()
        {
            return new VersionVector(_value);
        }

        public Dictionary<string, int> GetCells()
        {
            return Copy(_value);
        }

        public static bool operator >(VersionVector a, VersionVector b)
        {
            return a.ComparedTo(b) == Comparison.IsNewer;
        }

        public static bool operator <(VersionVector a, VersionVector b)
        {
            return a.ComparedTo(b) == Comparison.IsOlder;
        }

        public static bool operator ==(VersionVector a, VersionVector b)
        {
            return a.ComparedTo(b) == Comparison.AreSame;
        }

        public static bool operator !=(VersionVector a, VersionVector b)
        {
            return a.ComparedTo(b) != Comparison.AreSame;
        }

        public static bool operator <=(VersionVector a, VersionVector b)
        {
            var comparison = a.ComparedTo(b);
            return comparison == Comparison.AreSame || comparison == Comparison.IsOlder;
        }

        public static bool operator >=(VersionVector a, VersionVector b)
        {
            var comparison = a.ComparedTo(b);
            return comparison == Comparison.AreSame || comparison == Comparison.IsNewer;
        }

        public VersionVector Ceil(VersionVector another)
        {
            var a = Copy(_value);
            var b = Copy(another._value);

            var result = new Dictionary<string, int>();

            foreach (var kvp in a)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in b)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    result.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    result[kvp.Key] = Math.Max(result[kvp.Key], kvp.Value);
                }
            }

            return new VersionVector(result);
        }

        public VersionVector Floor(VersionVector another)
        {
            var a = Copy(_value);
            var b = Copy(another._value);

            var result = new Dictionary<string, int>();

            foreach (var kvp in a)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in b)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    result.Add(kvp.Key, 0);
                }
                else
                {
                    result[kvp.Key] = Math.Min(result[kvp.Key], kvp.Value);
                }
            }

            return new VersionVector(result);
        }

        private Dictionary<string, int> Copy(Dictionary<string, int> original)
        {
            return original.ToDictionary(k => k.Key, k => k.Value);
        }

        public override string ToString()
        {
            return $"[{String.Join(", ", _value.OrderBy(v => v.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}"))}]";
        }
    }
}
