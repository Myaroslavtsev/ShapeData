using NUnit.Framework; // requires NUnit 3.14.0, will upgrade tests later
using ShapeData.Editor_shapes;
using ShapeData.Geometry;
using ShapeData.Kuju_tsection.dat;
using ShapeData.Kuju_shape;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
//using NUnit.Framework.Legacy; // for NUnit 4.0 and newer

namespace ShapeData
{
    class ReplicationTests
    {
        const string tsectionPath = "tsection.dat"; // local copy of build 00038 is used // D:\\Train\\GLOBAL\\

        private static Task<KujuTsectionDat> _td;

        private static Task<KujuTsectionDat> GetTsectionDat()
        {
            return _td ??= KujuTsectionParser.LoadTsection(tsectionPath);
        }

        // Possible test cases to try:
        // "A1t10mStrt.s"
        // "A2t10mStrt.s"
        // "A1t500r10d.s" // 87,2664626 m length. End point @ X = -7,596123494, Z = 86.8240888
        // "A2t500r10d.s" // multi-track and curved
        // "SR_1tStr_c_005_6m.s" = 5.0m + 0.3m + 0.3m
        // "SR_2tCrv_c_00150r20d.s"  = 2 x (5d + 5d + 5d + 5d)

        // no replication case
        [TestCase("A4t10mStrt.s", PartReplicationMethod.NoReplication,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1)]

        // cases for replication at fixed pos
        [TestCase("A1t10mStrt.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1)]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 4)]
        [TestCase("SR_1tStr_c_005_6m.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1)]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 2)]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLengthAndCut, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 2)]

        // cases for replication at end pos
        [TestCase("A1t10mStrt.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1)]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 4)]
        [TestCase("SR_1tStr_c_005_6m.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1)]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 2)]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLengthAndCut, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 2)]

        // cases for replication by fixed intervals
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 0, 1.05f, 10.5f, 0f)]
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 9, 1.05f, 1.05f, 0f)]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 36, 1.05f, 1.05f, 0f)]
        [TestCase("A1t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 87, 1.0f, 1.0f, 0f)]

        // leave at last one part option test
        [TestCase("A1t0_8mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 0, 1.05f, 1.05f, 0f)]
        [TestCase("A1t0_8mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, true, 1, 1.05f, 1.05f, 0f)]
        [TestCase("A1t500r5d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 0, 50f, 50f, 0f)]
        [TestCase("A1t500r5d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthOnly, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, true, 1, 50f, 50f, 0f)]

        // Fix length + trim
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndCut, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 1.05f, 10.5f, 0f)]
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndCut, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 1.05f, 1.05f, 0f)]

        // cases for replication by even intervals
        /*
        [TestCase("A1t10mStrt.s", "ByEvenIntervals", 1.05f, 0f, false, 9)] // inaccurate division
        [TestCase("A4t10mStrt.s", "ByEvenIntervals", 1.05f, 0f, false, 36)]
        [TestCase("A1t500r10d.s", "ByEvenIntervals", 1.0f, 0f, false, 87)] // accurate division
        [TestCase("A1t100mStrt.s", "ByEvenIntervals", 1.0f, 0f, false, 100)]
        [TestCase("A1t10mStrt.s", "ByEvenIntervals", 0f, 0f, false, 1)] // protection from incorrect data

        // cases for replication stretched by arc
        [TestCase("A1t10mStrt.s", "StretchedByArc", 1.05f, 1.05f, false, 9)] // inaccurate division
        [TestCase("A4t10mStrt.s", "StretchedByArc", 1.05f, 1.05f, false, 36)]
        [TestCase("A1t500r10d.s", "StretchedByArc", 1.0f, 1.0f, false, 87)] // accurate division
        [TestCase("A1t100mStrt.s", "StretchedByArc", 1.0f, 1.0f, false, 100)]
        [TestCase("A1t0_8mStrt.s", "StretchedByArc", 1.05f, 1.05f, false, 0)] // short sections
        [TestCase("A1t0_8mStrt.s", "StretchedByArc", 1.05f, 1.05f, true, 1)]

        // cases for different original length values
        [TestCase("A1t10mStrt.s", "StretchedByArc", 2.05f, 1.05f, false, 9)]
        [TestCase("A1t10mStrt.s", "StretchedByArc", 0.05f, 1.05f, false, 9)]
        [TestCase("A1t10mStrt.s", "StretchedByArc", 0f, 1.05f, false, 9)]

        // cases for replication stretched by deflection
        [TestCase("A1t10mStrt.s", "StretchedByDeflection", 1.05f, 1.05f, false, 1)]
        [TestCase("A4t10mStrt.s", "StretchedByDeflection", 1.05f, 1.05f, false, 4)]
        [TestCase("A1t500r10d.s", "StretchedByDeflection", 1.0f, 0.000762f, false, 50)]*/

        public async Task ShapeReplicationPartQuantityTest(
            string shapeName,
            PartReplicationMethod repMethod,
            PartScalingMethod scaleMethod,
            PartStretchInWidthMethod stretchMethod,
            bool scaleTexture, bool bendPart, bool leaveOne,
            int replicaCount,
            float originalLength = 0,
            float minLength = 0,
            float maxDeflection = 0)
        {
            var td = await GetTsectionDat();

            var shape = new EditorShape("Simple shape");

            var repParams = new Dictionary<string, float>
            {
                { "OriginalLength", originalLength },
                { "MinLength", minLength },
                { "MaxDeflection", maxDeflection }
            };

            var part = shape.Lods[0].AddPart(new EditorPart("Plane",
                new PartReplication(repMethod, scaleMethod, stretchMethod, scaleTexture, bendPart, leaveOne, repParams)));

            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.2f, 0, 0, 0, 0),
                    new EditorVertex(1.2f, 0, 0, 1, 0),
                    new EditorVertex(0, 1.7f, 0, 0.5f, 1)
                }));

            var replica = await ShapeReplicator.ReplicatePartsInShape(shape, td.TrackShapes[shapeName], td);

            int ActualreplicasCount = 0;
            foreach (var rPart in replica.Parts())
                ActualreplicasCount += rPart.Polygons.Count;

            Assert.AreEqual(replicaCount, ActualreplicasCount);
        }

        // cases for replication at fixed pos
        [TestCase("A1t10mStrt.s", "AtFixedPos", 0f, 0f, false, 0f, 0f)]
        [TestCase("A2t10mStrt.s", "AtFixedPos", 0f, 0f, false, 2.4925f, 0f)]

        // cases for replication at the end
        [TestCase("A1t10mStrt.s", "AtTheEnd", 0f, 0f, false, 0f, 10f)]
        [TestCase("A2t10mStrt.s", "AtTheEnd", 0f, 0f, true, 2.4925f, 10f)]
        [TestCase("A1t500r10d.s", "AtTheEnd", 0f, 0f, true, 7.596123494f, 86.8240888f)]

        // cases for replication by fixed intervals
        [TestCase("A1t10mStrt.s", "ByFixedIntervals", 1.05f, 0f, false, 0f, 8.4f)]
        [TestCase("A1t500r10d.s", "ByFixedIntervals", 1.0f, 0f, true, 7.377784366f, 85.57658946f)] // stops in 1 m before end

        // cases for replication by even intervals
        [TestCase("A1t10mStrt.s", "ByEvenIntervals", 1.05f, 0f, false, 0f, 8.88888888f)]
        [TestCase("A2t10mStrt.s", "ByEvenIntervals", 1.05f, 0f, true, 2.4925f, 8.88888888f)]
        [TestCase("A1t500r10d.s", "ByEvenIntervals", 2.0f, 0f, true, 7.247769813f, 84.824758448f)] // stops in 2.1 m before end

        // cases for replication stretched by arc
        [TestCase("A1t10mStrt.s", "StretchedByArc", 1.0f, 1.05f, false, 0f, 8.88888888f)]
        [TestCase("A1t500r10d.s", "StretchedByArc", 1.0f, 2.0f, false, 7.247769813f, 84.824758448f)]

        // cases for replication stretched by deflection
        [TestCase("A1t10mStrt.s", "StretchedByDeflection", 1.0f, 1.05f, true, 0f, 0f)]
        [TestCase("A1t500r10d.s", "StretchedByDeflection", 1.0f, 0.000762f, false, 7.2960507583f, 85.104749583f)] // last part at 9.8 degree

        public async Task ShapeReplicationPartPositionTest(
           string shapeName,
           PartReplicationMethod repMethod,
           PartScalingMethod scaleMethod,
           PartStretchInWidthMethod stretchMethod,
           bool scaleTexture, bool bendPart, bool leaveOne,
           float X, float Z)
        {
            var td = await GetTsectionDat();

            var shape = new EditorShape("Simple shape");

            var part = shape.Lods[0].AddPart(
                new EditorPart("Plane", new PartReplication(repMethod, scaleMethod, stretchMethod, scaleTexture, bendPart, leaveOne)));

            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.2f, 0, 0, 0, 0),
                    new EditorVertex(1.2f, 0, 0, 1, 0),
                    new EditorVertex(0, 1.7f, 0, 0.5f, 1)
                }));

            var replica = await ShapeReplicator.ReplicatePartsInShape(shape, td.TrackShapes[shapeName], td);

            var lastPartPoly = replica.Lods[0].Parts.Last().Polygons[0];
            Assert.AreEqual(X, lastPartPoly.Vertices[2].Position.X, 1e-5f);
            Assert.AreEqual(Z, lastPartPoly.Vertices[2].Position.Z, 1e-5f);
        }
    }
}
