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
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
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

        // Test case parameter list:
        // shape name, part replication method,
        // part scaling method, part stretch method,
        // scale texture, bend part, leave at least one;  subdivision count;  expected replicas count;
        //      originalLength, intervalLength, maxDeflection, initialShift

        // no replication case
        [TestCase("A4t10mStrt.s", PartReplicationMethod.NoReplication,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 1, TestName = "No replication")]

        // cases for replication at fixed pos
        [TestCase("A1t10mStrt.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 1, TestName = "Fixed pos 1 track straight")]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 4, TestName = "Fixed pos 4 track straight")]
        [TestCase("SR_1tStr_c_005_6m.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 1, TestName = "Fixed pos 1 track complex straight")]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtFixedPos,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 2, TestName = "Fixed pos 2 track complex curve")]

        // cases for replication at end pos
        [TestCase("A1t10mStrt.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 1, TestName = "End pos 1 track")]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 4, TestName = "End pos 4 track")]
        [TestCase("SR_1tStr_c_005_6m.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 1, TestName = "End pos 1 track complex straight")]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.AtTheEnd,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            true, true, true, 1, 2, TestName = "End pos 2 track complex curve")]

        // cases for replication by fixed intervals
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 0, 1.05f, 10.5f, 0f, TestName = "FixedInt 1 track long part")]
        [TestCase("A1t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 9, 1.05f, 1.05f, 0f, TestName = "FixedInt 1 track short part")]
        [TestCase("A4t10mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 36, 1.05f, 1.05f, 0f, TestName = "FixedInt 4 track")]
        [TestCase("A1t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 8, 10.0f, 10.0f, 0f, TestName = "FixedInt 1 track curve")] 
        [TestCase("A2t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 173, 1.0f, 1.0f, 0f, TestName = "FixedInt 2 track curve")]
        [TestCase("SR_2tCrv_c_00150r20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 104, 1.0f, 1.0f, 0f, TestName = "FixedInt 2 track complex curve")]

        // leave at last one part option test
        [TestCase("A1t0_8mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 0, 1.05f, 1.05f, 0f, TestName = "LeaveAtLeastOne False straight")]
        [TestCase("A1t0_8mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, true, 1, 1, 1.05f, 1.05f, 0f, TestName = "LeaveAtLeastOne True straight")]
        [TestCase("A1t500r5d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 0, 50f, 50f, 0f, TestName = "LeaveAtLeastOne False curved")]
        [TestCase("A1t500r5d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, true, 1, 1, 50f, 50f, 0f, TestName = "LeaveAtLeastOne True curved")]
        
        // Subdivision trim cases
        [TestCase("A1t35mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 1, 3, 10.0f, 10.0f, 0f, TestName = "Subdiv - 1 divisions")]
        [TestCase("A1t35mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 35, 10.0f, 10.0f, 0f, TestName = "Subdiv - 10 divisions")]
        [TestCase("A1t3mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 3, 10.0f, 10.0f, 0f, TestName = "Subdiv - 3 of 10 divisions")]
        [TestCase("A1t2_5mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 2, 10.0f, 10.0f, 0f, TestName = "Subdiv - 2,5 of 10 no trim")]
        [TestCase("A1t2_5mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 3, 10.0f, 10.0f, 0f, TestName = "Subdiv - 2,5 of 10 last trimmed")]
        [TestCase("A1t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 88, 10.0f, 10.0f, 0f, TestName = "Subdiv - 1 track curve trimmed")]
        [TestCase("A1t0_8mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 1, 10.0f, 10.0f, 0f, TestName = "Subdiv - 0,8 of 10 last trimmed")]

        // segment bending
        [TestCase("A1t120r20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 20, 42, 20.0f, 20.0f, 0f, TestName = "Bending - not bent")]
        [TestCase("A1t120r20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, true, false, 20, 42, 20.0f, 20.0f, 0f, TestName = "Bending - bent")]

        // stretching parts (at fixed interval replication)
        [TestCase("A1t120r20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 22, 10.0f, 20.0f, 0f, TestName = "Stretching - no stretching")]
        [TestCase("A1t120r20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 20, 10.0f, 20.0f, 0f, TestName = "Stretching - stretch")]

        // initial shift
        [TestCase("A1t2_5mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 3, 10.0f, 10.0f, 0f, 0.25f, TestName = "Initial shift positive")]
        [TestCase("A1t2_5mStrt.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 3, 10.0f, 10.0f, 0f, -0.25f, TestName = "Initial shift negative")]
        [TestCase("A1t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 88, 10.0f, 10.0f, 0f, 1.5f, TestName = "Initial shift curved positive")]
        [TestCase("A1t500r10d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 88, 10.0f, 10.0f, 0f, -15f, TestName = "Initial shift curved negative")]

        // Non zero start point
        [TestCase("A1tXover20d.s", PartReplicationMethod.ByFixedIntervals,
            PartScalingMethod.FixLengthAndTrim, PartStretchInWidthMethod.ReplicateAlongAllTracks,
            false, false, false, 10, 30, 10.0f, 10.0f, 0f, TestName = "Non-zero start pos and angle")]

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
            int subdivisionCount,
            int replicaCount,
            float originalLength = 1,
            float intervalLength = 0,
            float maxDeflection = 0,
            float initialShift = 0)
        {
            var td = await GetTsectionDat();

            var shape = new EditorShape(TestContext.CurrentContext.Test.Name);

            var repParams = new Dictionary<string, float>
            {
                { "OriginalLength", originalLength },
                { "IntervalLength", intervalLength },
                { "MaxDeflection", maxDeflection },
                { "SubdivisionCount", subdivisionCount },
                { "InitialShift", initialShift }
            };

            var part = shape.Lods[0].AddPart(new EditorPart("Plane",
                new PartReplication(repMethod, scaleMethod, stretchMethod, scaleTexture, bendPart, leaveOne, repParams)));

            for (int i = 0; i < Math.Max(1, subdivisionCount); i++)
                part.AddPolygon(new EditorPolygon(
                    new List<EditorVertex> {
                        new EditorVertex(-1.2f, 0, i*1.0f, 0, 0),
                        new EditorVertex(-1.2f, 0, (i + 1)*1.0f, 0, 1),
                        new EditorVertex(+1.2f, 0, (i + 1)*1.0f, 1, 1),
                        new EditorVertex(+1.2f, 0, i*1.0f, 1, 0)
                    }));

            var replica = await ShapeReplicator.ReplicatePartsInShape(shape, td.TrackShapes[shapeName], td);

            int ActualreplicasCount = 0;
            foreach (var rPart in replica.Parts())
                ActualreplicasCount += rPart.Polygons.Count;

            DrawShape(replica, TestContext.CurrentContext.Test.Name, 25);

            Assert.AreEqual(replicaCount, ActualreplicasCount);
        }

        /*
         * Cases are commented as they are being checked visually using DrawShape method
         *
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
        */
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

        private static void DrawShape(EditorShape shape, string name, int pixelsPerMeter)
        {
            var bb = GetBoundingRect(shape);

            var width = (int)((5 + bb.Item2.X - bb.Item1.X) * pixelsPerMeter);
            var height = (int)((5 + bb.Item2.Y - bb.Item1.Y) * pixelsPerMeter);

            var originX = (int)((2.5 - bb.Item1.X) * pixelsPerMeter);
            var originY = height + (int)((bb.Item1.Y - 2.5) * pixelsPerMeter);

            if (bb.Item2.X < bb.Item1.X || bb.Item2.Y < bb.Item1.Y)
            {
                width = 5 * pixelsPerMeter;
                height = 5 * pixelsPerMeter;
                originX = (int)(2.5 * pixelsPerMeter);
                originY = (int)(2.5 * pixelsPerMeter);
            }

            using var bmp = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bmp);

            // prepare cancas
            graphics.Clear(Color.White);

            // draw coordinate senter
            using var navyPen = new Pen(Color.Navy, 2.0f);
            graphics.DrawEllipse(navyPen, originX - pixelsPerMeter, originY - pixelsPerMeter, 
                2 * pixelsPerMeter, 2 * pixelsPerMeter);
            graphics.DrawLine(navyPen, originX, originY, originX, originY - pixelsPerMeter);

            // draw shape
            using var redPen = new Pen(Color.Red, 1.0f);
            foreach (var poly in shape.Polygons())
            {
                for (int n = 1; n < poly.Vertices.Count; n++)
                {
                    var start = new PointF(originX + poly.Vertices[n].Position.X * pixelsPerMeter,
                        originY - poly.Vertices[n].Position.Z * pixelsPerMeter);
                    var end = new PointF(originX + poly.Vertices[n - 1].Position.X * pixelsPerMeter,
                        originY - poly.Vertices[n - 1].Position.Z * pixelsPerMeter);
                    graphics.DrawLine(redPen, start, end);
                }

                var start2 = new PointF(originX + poly.Vertices[0].Position.X * pixelsPerMeter,
                        originY - poly.Vertices[0].Position.Z * pixelsPerMeter);
                var end2 = new PointF(originX + poly.Vertices[^1].Position.X * pixelsPerMeter,
                        originY - poly.Vertices[^1].Position.Z * pixelsPerMeter);
                graphics.DrawLine(redPen, start2, end2);
            }

            // write comments
            var font = new Font("Arial", 10);
            var brush = new SolidBrush(Color.Black);

            graphics.DrawString(name, font, brush, 5, 5);
            graphics.DrawString(shape.ShapeName, font, brush, 5, 20);
            graphics.DrawString("Parts: " + shape.Parts().Count().ToString(), font, brush, 5, 35);
            graphics.DrawString("Polygons: " + shape.Polygons().Count().ToString(), font, brush, 5, 50);

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Tests\\");
            bmp.Save(Directory.GetCurrentDirectory() + "\\Tests\\" + name + ".png", ImageFormat.Png);
        }

        private static (Vector2, Vector2) GetBoundingRect(EditorShape shape)
        {
            var minX = float.MaxValue;
            var minZ = float.MaxValue;
            var maxX = float.MinValue;
            var maxZ = float.MinValue;

            foreach (var vertex in shape.Vertices())
            {
                if (vertex.Position.X > maxX) maxX = vertex.Position.X;
                if (vertex.Position.X < minX) minX = vertex.Position.X;
                if (vertex.Position.Z > maxZ) maxZ = vertex.Position.Z;
                if (vertex.Position.Z < minZ) minZ = vertex.Position.Z;
            }

            if (maxX < 0) maxX = 0;
            if (minX > 0) minX = 0;
            if (maxZ < 0) maxZ = 0;
            if (minZ > 0) minZ = 0;

            return (new Vector2(minX, minZ), new Vector2(maxX, maxZ));
        }
    }
}
