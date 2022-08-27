using Forester.Framework.EventStore;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Khala.App.Test.Framework.Time
{
    public class VersionMatrixTest
    {
        private ITestOutputHelper _output;

        public VersionMatrixTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MatrixUpdateShouldWorkCorrectly()
        {
            var AversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 3 },
                { "B", 1 }, // A does not know B has been updated
                { "C", 1 },
            });

            var BversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // A has updated itself without yet telling to A
                { "B", 1 },
                { "C", 1 },
            });

            var CversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 1 },
            });

            // [ 3, 1, 1
            //   1, 1, 1
            //   1, 1, 1 ]
            var matrixA = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByA },
                { "B", BversionByA },
                { "C", CversionByA },
            });

            var updateVersionA = new VersionVector(new Dictionary<string, int> {
                { "A", 4 },
                { "B", 2 }, // A does not know B has been updated
                { "C", 1 },
            });

            matrixA = matrixA.Update("A", updateVersionA);

            Assert.Equal(4, matrixA["A"]["A"]);
            Assert.Equal(2, matrixA["A"]["B"]);
            Assert.Equal(1, matrixA["B"]["A"]);
            Assert.Equal(1, matrixA["C"]["A"]);
        }

        [Fact]
        public void MatricesShouldSynchronizeCorrectly()
        {
            var AversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 3 },
                { "B", 1 }, // A does not know B has been updated
                { "C", 1 },
            });

            var BversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // A has updated itself without yet telling to A
                { "B", 1 },
                { "C", 1 },
            });

            var CversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 1 },
            });

            // [ 3, 1, 1
            //   1, 1, 1
            //   1, 1, 1 ]
            var matrixA = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByA },
                { "B", BversionByA },
                { "C", CversionByA },
            });

            var AversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 }, // B has updated itself without yet telling to A
                { "C", 1 },
            });

            var BversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // B does not know A has been updated
                { "B", 3 },
                { "C", 1 },
            });

            var CversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 1 },
            });

            // [ 1, 1, 1
            //   1, 3, 1
            //   1, 1, 1 ]
            var matrixB = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByB },
                { "B", BversionByB },
                { "C", CversionByB },
            });

            // [ 3, 3, 1
            //   3, 3, 1
            //   1, 1, 1 ]
            var synchronized = matrixA.Sync(matrixB, "A", "B");

            // [ 3, 1, 1
            //   1, 3, 1
            //   1, 1, 1 ]
            var ceiled = matrixA.Ceil(matrixB);

            Assert.True(synchronized["A", "B"] == 3);
            Assert.True(synchronized["B", "A"] == 3);
            Assert.True(synchronized["A", "A"] == 3);
            Assert.True(synchronized["B", "B"] == 3);

            Assert.True(ceiled["A", "A"] == 3);
            Assert.True(ceiled["B", "B"] == 3);
            Assert.True(ceiled["A", "B"] == 1);
            Assert.True(ceiled["B", "A"] == 1);
        }

        [Fact]
        public void MatricesShouldProvideCorrectStableTimestampAfterSynchronization()
        {
            var AversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 3 },
                { "B", 1 }, // A does not know B has been updated
            });

            var BversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // A has updated itself without yet telling to A
                { "B", 1 },
            });

            // [ 3, 1
            //   1, 1 ]
            var matrixA = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByA },
                { "B", BversionByA },
            });

            var AversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 }, // B has updated itself without yet telling to A
            });

            var BversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // B does not know A has been updated
                { "B", 3 },
            });

            // [ 1, 1
            //   1, 3 ]
            var matrixB = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByB },
                { "B", BversionByB },
            });

            // [ 3, 3
            //   3, 3 ]
            var synchronized = matrixA.Sync(matrixB, "A", "B");
            var stable = synchronized.Stable("A", "B");

            Assert.Equal(3, stable["A"]);
            Assert.Equal(3, stable["B"]);
        }

        [Fact]
        public void MatrixShouldProvideCorrectStableTimestamp()
        {
            var AversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 3 },
                { "B", 1 }, // A does not know B has been updated
                { "C", 2 },
            });

            var BversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 }, // A has updated itself without yet telling to A
                { "B", 1 },
                { "C", 4 },
            });

            var CversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 4 },
            });

            // [ 3, 1, 2
            //   1, 1, 4
            //   1, 1, 4 ]
            var matrixA = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByA },
                { "B", BversionByA },
                { "C", CversionByA },
            });

            // [ 1, 1, 2 ]
            var stableA = matrixA.Stable(new List<string> { "A", "B", "C" });

            Assert.Equal(1, stableA["A"]);
            Assert.Equal(1, stableA["B"]);
            Assert.Equal(2, stableA["C"]);
        }

        [Fact]
        public void MatrixShouldProvideCorrectStableTimestampAfterUpdate()
        {
            var AversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 3 },
                { "B", 1 },
                { "C", 2 },
            });

            var BversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 2 },
            });

            var CversionByA = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 2 },
            });

            // [ 3, 1, 2
            //   1, 1, 2
            //   1, 1, 2 ]
            var matrixA = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByA },
                { "B", BversionByA },
                { "C", CversionByA },
            });

            var AversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 2 },
            });

            var BversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 3 },
                { "C", 2 },
            });

            var CversionByB = new VersionVector(new Dictionary<string, int> {
                { "A", 1 },
                { "B", 1 },
                { "C", 2 },
            });

            // [ 3, 1, 2
            //   1, 3, 2
            //   1, 1, 2 ]
            var matrixB = new VersionMatrix(new Dictionary<string, VersionVector> {
                { "A", AversionByB },
                { "B", BversionByB },
                { "C", CversionByB },
            });

            matrixB = matrixB.Update("A", AversionByA);
            matrixB = matrixB.Update("B", AversionByA.Ceil(BversionByB));
            matrixB = matrixB.Update("C", CversionByA.Ceil(BversionByB));

            var stableA = matrixA.Stable(new List<string> { "A", "B", "C" });
            var stableB = matrixB.Stable(new List<string> { "A", "B", "C" });

            Assert.Equal(1, stableA["A"]);
            Assert.Equal(1, stableA["B"]);
            Assert.Equal(2, stableA["C"]);
            Assert.Equal(3, stableB["A"]);
            Assert.Equal(1, stableB["B"]);
            Assert.Equal(2, stableB["C"]);
        }
    }
}