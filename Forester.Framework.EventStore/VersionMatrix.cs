using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public VersionMatrix Update(string node, VersionVector version)
        {
            var copy = Copy();

            copy._versions[node] = version;

            return copy;
        }

        public VersionMatrix Ceil(VersionMatrix other)
        {
            var results = new Dictionary<string, VersionVector>();

            var first = Copy()._versions;
            var second = other.Copy()._versions;

            var nodes = new HashSet<string>(first.Keys.Union(second.Keys));

            foreach (var node in nodes)
            {
                var firstVersion = first.ContainsKey(node) ? first[node] : new VersionVector();
                var secondVersion = second.ContainsKey(node) ? second[node] : new VersionVector();

                var ceilVersion = firstVersion.Ceil(secondVersion);
                results.Add(node, ceilVersion);
            }

            return new VersionMatrix(results);
        }

        public VersionMatrix Floor(VersionMatrix other)
        {
            var results = new Dictionary<string, VersionVector>();

            var first = _versions;
            var second = other._versions;

            var nodes = new HashSet<string>(first.Keys.Union(second.Keys));

            foreach (var node in nodes)
            {
                var firstVersion = first.ContainsKey(node) ? first[node] : new VersionVector();
                var secondVersion = second.ContainsKey(node) ? second[node] : new VersionVector();

                var ceilVersion = firstVersion.Floor(secondVersion);
                results.Add(node, ceilVersion);
            }

            return new VersionMatrix(results);
        }

        /// <summary>
        /// Returns stable timestamp among given nodes. As clock itself allows multiple nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public VersionVector Stable(List<string> nodes)
        {
            VersionVector? stableStamp = null;

            foreach (var node in nodes)
            {
                var version = _versions.ContainsKey(node) ? _versions[node] : new VersionVector();
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
    }
}
