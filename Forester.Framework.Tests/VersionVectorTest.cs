using Forester.Framework.EventStore;
using System.Collections.Generic;
using Xunit;

namespace Khala.App.Test.Framework.Time
{
    public class VersionVectorTest
    {
        [Fact]
        public void EnsureCeilingWorks()
        {
            var v1 = new Dictionary<string, int>() {
                { "node1", 5 },
                { "node2", 3 }
            };

            var v2 = new Dictionary<string, int>() {
                { "node1", 4 },
                { "node2", 5 },
                { "node3", 1 }
            };

            var version1 = new VersionVector(v1);
            var version2 = new VersionVector(v2);

            var combined = version1.Ceil(version2);

            Assert.Equal(VersionVector.Comparison.IsNewer, combined.ComparedTo(version1));
            Assert.Equal(VersionVector.Comparison.IsNewer, combined.ComparedTo(version2));
            Assert.Equal(5, combined.GetCells()["node1"]);
            Assert.Equal(5, combined.GetCells()["node2"]);
            Assert.Equal(1, combined.GetCells()["node3"]);
        }

        [Fact]
        public void EnsureFlooringWorks()
        {
            var v1 = new Dictionary<string, int>() {
                { "node1", 5 },
                { "node2", 3 }
            };

            var v2 = new Dictionary<string, int>() {
                { "node1", 4 },
                { "node2", 5 },
                { "node3", 1 }
            };

            var version1 = new VersionVector(v1);
            var version2 = new VersionVector(v2);

            var combined = version1.Floor(version2);

            Assert.Equal(VersionVector.Comparison.IsOlder, combined.ComparedTo(version1));
            Assert.Equal(VersionVector.Comparison.IsOlder, combined.ComparedTo(version2));
            Assert.Equal(4, combined.GetCells()["node1"]);
            Assert.Equal(3, combined.GetCells()["node2"]);
            Assert.Equal(0, combined.GetCells()["node3"]);
        }

        [Fact]
        public void EmptyAreSame()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            Assert.Equal(VersionVector.Comparison.AreSame, version1.ComparedTo(version2));
        }

        [Fact]
        public void IncrementedAreSame()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            version1 = version1.Next("node1");
            version1 = version1.Next("node1");
            version2 = version2.Next("node1");
            version2 = version2.Next("node1");

            Assert.Equal(VersionVector.Comparison.AreSame, version1.ComparedTo(version2));
        }

        [Fact]
        public void IncrementedDifferentNodesAreConcurrent()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            version1 = version1.Next("node1");
            version2 = version2.Next("node2");

            Assert.Equal(VersionVector.Comparison.AreConcurrent, version1.ComparedTo(version2));
        }

        [Fact]
        public void IncrementedDifferentNodesAreConcurrent2()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            version1 = version1.Next("node1");
            version2 = version2.Next("node1");
            version1 = version1.Next("node1");
            version2 = version2.Next("node2");

            Assert.Equal(VersionVector.Comparison.AreConcurrent, version1.ComparedTo(version2));
        }

        [Fact]
        public void OneIsBeforeAnother()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            version1 = version1.Next("node1");
            version2 = version2.Next("node1");
            version1 = version1.Next("node1");

            Assert.Equal(VersionVector.Comparison.IsNewer, version1.ComparedTo(version2));
        }

        [Fact]
        public void OneIsAfterAnother()
        {
            var version1 = new VersionVector();
            var version2 = new VersionVector();

            version1 = version1
                .Next("node1");

            version2 = version2
                .Next("node1")
                .Next("node1");

            Assert.Equal(VersionVector.Comparison.IsOlder, version1.ComparedTo(version2));
        }

        [Fact]
        public void CopiedVectorsAreSame()
        {
            var version1 = new VersionVector();

            version1 = version1
                .Next("node1")
                .Next("node1")
                .Next("node2")
                .Next("node5")
                .Next("node1")
                .Next("node2")
                .Next("node1")
                .Next("node6");

            var version2 = version1
                .Copy();

            Assert.Equal(VersionVector.Comparison.AreSame, version1.ComparedTo(version2));
        }

        [Fact]
        public void OneIsAfterAnotherAfterCopy()
        {
            var version1 = new VersionVector();

            version1 = version1
                .Next("node1")
                .Next("node1")
                .Next("node2")
                .Next("node5")
                .Next("node1")
                .Next("node2")
                .Next("node1")
                .Next("node6");

            var version2 = version1
                .Copy()
                .Next("node2");

            Assert.Equal(VersionVector.Comparison.IsOlder, version1.ComparedTo(version2));
        }
    }
}