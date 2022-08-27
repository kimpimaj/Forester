using System.Text;

namespace Forester.Framework.EventStore
{
    public class VersionMatrix
    {
        private Dictionary<string, VersionVector> _versions = new Dictionary<string, VersionVector>();

        public VersionMatrix()
        {
            _versions = new Dictionary<string, VersionVector>(); ;
        }

        public VersionMatrix(Dictionary<string, VersionVector> versions)
        {
            _versions = versions;
        }

        /// <summary>
        /// Returns a version vector that describes observed 
        /// state for a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public VersionVector this[string node]
        {
            get => _versions.ContainsKey(node) 
                ? _versions[node] 
                : new VersionVector();
        }

        /// <summary>
        /// Returns a version that describes state of 
        /// a target node observed by an observing node
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int this[string observingNode, string targetNode]
        {
            get
            {
                if (!_versions.ContainsKey(observingNode))
                {
                    return 0;
                }

                return _versions[observingNode][targetNode];
            }
        }

        public VersionMatrix Copy()
        {
            return new VersionMatrix(_versions.ToDictionary(
                    x => x.Key, 
                    x => x.Value.Copy()));
        }

        public Dictionary<string, VersionVector> GetVectors()
        {
            return _versions.ToDictionary(
                    x => x.Key,
                    x => x.Value.Copy());
        }

        /// <summary>
        /// Replaces version vector for given node.
        /// </summary>
        /// <param name="node">Node to be updated</param>
        /// <param name="version">Updated version vector</param>
        /// <returns>Updated version matrix</returns>
        public VersionMatrix Update(string node, VersionVector version)
        {
            var copy = Copy();

            copy._versions[node] = version;

            return copy;
        }

        /// <summary>
        /// Cell wise maximum between two version matrices, 
        /// and takes cell-wise maximum between versions of
        /// the two given nodes putting them into same state.
        /// </summary>
        /// <param name="other">
        /// Version matrix to be compared.
        /// </param>
        /// <returns>
        /// Synchronized version matrix.
        /// </returns>
        public VersionMatrix Sync(VersionMatrix other, string node1, string node2)
        {
            var ceiled = Ceil(other);

            var first = ceiled.GetVectors()[node1];
            var second = ceiled.GetVectors()[node2];

            var synced = first.Ceil(second);

            ceiled = ceiled.Update(node1, synced);
            ceiled = ceiled.Update(node2, synced);

            return ceiled;
        }

        /// <summary>
        /// Cell wise maximum between two version matrices.
        /// </summary>
        /// <param name="other">
        /// Version matrix to be compared.
        /// </param>
        /// <returns>
        /// Version matrix with maximum cell values.
        /// </returns>
        public VersionMatrix Ceil(VersionMatrix other)
        {
            var results = new Dictionary<string, VersionVector>();

            var first = Copy()._versions;
            var second = other.Copy()._versions;

            var nodes = new HashSet<string>(first.Keys.Union(second.Keys));

            foreach (var node in nodes)
            {
                var firstVersion = first.ContainsKey(node) 
                    ? first[node] 
                    : new VersionVector(new Dictionary<string, int> { { node, 0 } });
                var secondVersion = second.ContainsKey(node) 
                    ? second[node] 
                    : new VersionVector(new Dictionary<string, int> { { node, 0 } });

                var ceilVersion = firstVersion.Ceil(secondVersion);
                results.Add(node, ceilVersion);
            }

            return new VersionMatrix(results);
        }

        /// <summary>
        /// Cell wise minimum between two version matrices.
        /// </summary>
        /// <param name="other">
        /// Version matrix to be compared.
        /// </param>
        /// <returns>
        /// Version matrix with minimum cell values.
        /// </returns>
        public VersionMatrix Floor(VersionMatrix other)
        {
            var results = new Dictionary<string, VersionVector>();

            var first = _versions;
            var second = other._versions;

            var nodes = new HashSet<string>(first.Keys.Union(second.Keys));

            foreach (var node in nodes)
            {
                var firstVersion = first.ContainsKey(node) 
                    ? first[node] 
                    : new VersionVector(new Dictionary<string, int> { { node, 0 } });

                var secondVersion = second.ContainsKey(node) 
                    ? second[node] 
                    : new VersionVector(new Dictionary<string, int> { { node, 0 } });

                var ceilVersion = firstVersion.Floor(secondVersion);
                results.Add(node, ceilVersion);
            }

            return new VersionMatrix(results);
        }

        public VersionVector Stable(params string[] nodes)
        {
            return Stable(nodes.ToList());
        }

        /// <summary>
        /// Returns stable timestamp among given nodes.
        /// If any of the nodes is not known, an empty
        /// version vector with all zero values will be
        /// returned.
        /// </summary>
        /// <param name="nodes">
        /// Nodes the stability is checked among.
        /// </param>
        /// <returns>Stable timestamp</returns>
        public VersionVector Stable(List<string> nodes)
        {
            VersionVector? stableStamp = null;

            foreach (var node in nodes)
            {
                var version = _versions.ContainsKey(node) 
                    ?  _versions[node]
                    : new VersionVector(new Dictionary<string, int> { { node, 0 } });

                if (ReferenceEquals(stableStamp, null))
                {
                    stableStamp = version;
                }
                else
                {
                    stableStamp = version.Floor(stableStamp);
                }
            }

            return stableStamp ?? new VersionVector();
        }

        public override string ToString()
        {
            var resultBuilder = new StringBuilder();

            foreach (var version in _versions)
            {
                resultBuilder.AppendLine($"{version.Key}: ${version.Value.ToString()}");
            }

            return resultBuilder.ToString();
        }
    }
}
