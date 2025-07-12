using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Editor_shapes;
using ShapeData.Geometry;
using ShapeData.Kuju_tsection.dat;
using ShapeData.Kuju_shape;
using NUnit.Framework; // requires NUnit 3.14.0, will upgrade tests later
//using NUnit.Framework.Legacy; // for NUnit 4.0 and newer

namespace ShapeData
{
    class PartSampleCreationTests
    {
        [Test]
        public async Task CreatePartSamples()
        {
            var shape = new EditorShape("SampleShape");

            var oldLod = shape.AddLod(new EditorLod(200));

            // Rails
            var railRepParams = new Dictionary<string, float> {
                { "OriginalLength".ToLower(), 10f },
                { "MaxDeflection".ToLower(), 0.01f },
                { "SubdivisionCount".ToLower(), 1f }
            };

            var method = PartReplicationMethod.ByDeflection;
            var preserveTextureDim = true;

            var part = oldLod.AddPart(new EditorPart("RailHead",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.173f, 0.326f, 0f, 244f/ 2048, 0),
                    new EditorVertex(0.173f, 0.326f, 10f, 244f / 2048, 2),
                    new EditorVertex(0.243f, 0.323f, 10f, 311f / 2048, 2),
                    new EditorVertex(0.243f, 0.323f, 0f, 311f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.170f, 0.291f, 0f, 196f/ 2048, 0),
                    new EditorVertex(0.170f, 0.291f, 10f, 196f / 2048, 1),
                    new EditorVertex(0.173f, 0.326f, 10f, 243f / 2048, 1),
                    new EditorVertex(0.173f, 0.326f, 0f, 243f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.243f, 0.323f, 0f, 312f/ 2048, 0),
                    new EditorVertex(0.243f, 0.323f, 10f, 312f / 2048, 1),
                    new EditorVertex(0.243f, 0.288f, 10f, 359f / 2048, 1),
                    new EditorVertex(0.243f, 0.288f, 0f, 359f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = oldLod.AddPart(new EditorPart("RailSideOut",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.125f, 0.148f, 0f, 0f/ 2048, 0),
                    new EditorVertex(0.125f, 0.148f, 10f, 0f / 2048, 1),
                    new EditorVertex(0.125f, 0.159f, 10f, 16f / 2048, 1),
                    new EditorVertex(0.125f, 0.159f, 0f, 16f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.125f, 0.159f, 0f, 17f/ 2048, 0),
                    new EditorVertex(0.125f, 0.159f, 10f, 17f / 2048, 1),
                    new EditorVertex(0.188f, 0.172f, 10f, 95f / 2048, 1),
                    new EditorVertex(0.188f, 0.172f, 0f, 95f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.188f, 0.172f, 0f, 96f/ 2048, 0),
                    new EditorVertex(0.188f, 0.172f, 10f, 96f / 2048, 1),
                    new EditorVertex(0.197f, 0.300f, 10f, 195f / 2048, 1),
                    new EditorVertex(0.197f, 0.300f, 0f, 195f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = oldLod.AddPart(new EditorPart("RailSideIn",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.217f, 0.299f, 0f, 360f/ 2048, 0),
                    new EditorVertex(0.217f, 0.299f, 10f, 360f / 2048, 1),
                    new EditorVertex(0.216f, 0.170f, 10f, 462f / 2048, 1),
                    new EditorVertex(0.216f, 0.170f, 0f, 462f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.216f, 0.170f, 0f, 463f/ 2048, 0),
                    new EditorVertex(0.216f, 0.170f, 10f, 463f / 2048, 1),
                    new EditorVertex(0.276f, 0.153f, 10f, 541f / 2048, 1),
                    new EditorVertex(0.276f, 0.153f, 0f, 541f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.276f, 0.153f, 0f, 542f/ 2048, 0),
                    new EditorVertex(0.276f, 0.153f, 10f, 542f / 2048, 1),
                    new EditorVertex(0.276f, 0.142f, 10f, 558f / 2048, 1),
                    new EditorVertex(0.276f, 0.142f, 0f, 558f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            // Sleepers and plates
            var repParams = new Dictionary<string, float> {
                { "OriginalLength".ToLower(), 5.5f },
                { "MaxDeflection".ToLower(), 0.02f },
                { "SubdivisionCount".ToLower(), 10f }
            };
            var sleepersMethod = PartReplicationMethod.ByDeflection;
            preserveTextureDim = false;

            part = oldLod.AddPart(new EditorPart("PlateSideOut",
                new PartReplication(sleepersMethod, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, repParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.020f, 0.125f, 0f, 836f/ 2048, 0),
                    new EditorVertex(0.020f, 0.125f, 2.75f, 836f / 2048, 0.5f),
                    new EditorVertex(0.020f, 0.134f, 2.75f, 851f / 2048, 0.5f),
                    new EditorVertex(0.020f, 0.134f, 0f, 851f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.020f, 0.134f, 0f, 852f/ 2048, 0),
                    new EditorVertex(0.020f, 0.134f, 2.75f, 852f / 2048, 0.5f),
                    new EditorVertex(0.135f, 0.170f, 2.75f, 895f / 2048, 0.5f),
                    new EditorVertex(0.135f, 0.170f, 0f, 895f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.020f, 0.125f, 2.75f, 836f/ 2048, 0.5f),
                    new EditorVertex(0.020f, 0.125f, 5.5f, 836f / 2048, 1),
                    new EditorVertex(0.020f, 0.134f, 5.5f, 851f / 2048, 1),
                    new EditorVertex(0.020f, 0.134f, 2.75f, 851f / 2048, 0.5f)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.020f, 0.134f, 2.75f, 852f/ 2048, 0.5f),
                    new EditorVertex(0.020f, 0.134f, 5.5f, 852f / 2048, 1),
                    new EditorVertex(0.135f, 0.170f, 5.5f, 895f / 2048, 1),
                    new EditorVertex(0.135f, 0.170f, 2.75f, 895f / 2048, 0.5f)
                }, Material.TransNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -0.988f, 0, 0);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = oldLod.AddPart(new EditorPart("PlateSideIn",
                new PartReplication(sleepersMethod, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, repParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.265f, 0.165f, 0f, 896f/ 2048, 0),
                    new EditorVertex(0.265f, 0.165f, 2.75f, 896f / 2048, 0.5f),
                    new EditorVertex(0.380f, 0.134f, 2.75f, 939f / 2048, 0.5f),
                    new EditorVertex(0.380f, 0.134f, 0f, 939f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.380f, 0.134f, 0f, 940f/ 2048, 0),
                    new EditorVertex(0.380f, 0.134f, 2.75f, 940f / 2048, 0.5f),
                    new EditorVertex(0.380f, 0.125f, 2.75f, 955f / 2048, 0.5f),
                    new EditorVertex(0.380f, 0.125f, 0f, 955f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.265f, 0.165f, 2.75f, 896f/ 2048, 0.5f),
                    new EditorVertex(0.265f, 0.165f, 5.5f, 896f / 2048, 1),
                    new EditorVertex(0.380f, 0.134f, 5.5f, 939f / 2048, 1),
                    new EditorVertex(0.380f, 0.134f, 2.75f, 939f / 2048, 0.5f)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.380f, 0.134f, 2.75f, 940f/ 2048, 0.5f),
                    new EditorVertex(0.380f, 0.134f, 5.5f, 940f / 2048, 1),
                    new EditorVertex(0.380f, 0.125f, 5.5f, 955f / 2048, 1),
                    new EditorVertex(0.380f, 0.125f, 2.75f, 955f / 2048, 0.5f)
                }, Material.TransNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.018f, 0, 0);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = oldLod.AddPart(new EditorPart("Sleepers",
                new PartReplication(sleepersMethod, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, repParams)));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.375f, 0.090f, 0.0f, 964f/ 2048, 0),
                    new EditorVertex(-1.375f, 0.090f, 5.5f, 964f / 2048, 1),
                    new EditorVertex(-1.375f, 0.125f, 5.5f, 999f / 2048, 1),
                    new EditorVertex(-1.375f, 0.125f, 0.0f, 999f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.375f, 0.125f, 0.0f, 1004f/ 2048, 0),
                    new EditorVertex(-1.375f, 0.125f, 5.5f, 1004f / 2048, 1),
                    new EditorVertex(1.375f, 0.125f, 5.5f, 2044f / 2048, 1),
                    new EditorVertex(1.375f, 0.125f, 0.0f, 2044f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(1.375f, 0.125f, 0.0f, 999f/ 2048, 0),
                    new EditorVertex(1.375f, 0.125f, 5.5f, 999f/ 2048, 1),
                    new EditorVertex(1.375f, 0.090f, 5.5f, 964f / 2048, 1),
                    new EditorVertex(1.375f, 0.090f, 0.0f, 964f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));

            part = oldLod.AddPart(new EditorPart("Ballast",
                new PartReplication(sleepersMethod, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, repParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-3.0f, -0.5f, 0.0f, 0f / 1024, 0),
                    new EditorVertex(-3.0f, -0.5f, 5.5f, 0f / 1024, 1),
                    new EditorVertex(-1.8f, 0.09f, 5.5f, 214f / 1024, 1),
                    new EditorVertex(-1.8f, 0.09f, 0.0f, 214f / 1024, 0)
                }, Material.SolidNorm, "Trackbed1", true));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.8f, 0.09f, 0.0f, 215f / 1024, 0),
                    new EditorVertex(-1.8f, 0.09f, 5.5f, 215f / 1024, 1),
                    new EditorVertex(1.8f, 0.09f, 5.5f, 809f / 1024, 1),
                    new EditorVertex(1.8f, 0.09f, 0.0f, 809f / 1024, 0)
                }, Material.SolidNorm, "Trackbed1", true));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(1.8f, 0.09f, 0.0f, 810f / 1024, 0),
                    new EditorVertex(1.8f, 0.09f, 5.5f, 810f / 1024, 1),
                    new EditorVertex(3.0f, -0.5f, 5.5f, 1024f / 1024, 1),
                    new EditorVertex(3.0f, -0.5f, 0.0f, 1024f / 1024, 0)
                }, Material.SolidNorm, "Trackbed1", true));

            part = oldLod.AddPart(new EditorPart("Ballast start",
                new PartReplication(PartReplicationMethod.AtFixedPos, PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                false, true, false, repParams)));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-3.0f, -0.5f, 0.0f, 0f / 1024, 128f / 1024),
                    new EditorVertex(-1.8f, 0.09f, 0.0f, 214f / 1024, 78f / 1024),
                    new EditorVertex(1.8f, 0.09f, 0.0f, 809f / 1024, 78f / 1024),
                    new EditorVertex(3.0f, -0.5f, 0.0f, 1024f / 1024, 128f / 1024)
                }, Material.SolidNorm, "Trackbed1", true));

            part = oldLod.AddPart(new EditorPart("Ballast end",
                new PartReplication(PartReplicationMethod.AtTheEnd, PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                false, true, false, repParams)));
            var poly = part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-3.0f, -0.5f, 0.0f, 0f / 1024, 128f / 1024),
                    new EditorVertex(-1.8f, 0.09f, 0.0f, 214f / 1024, 78f / 1024),
                    new EditorVertex(1.8f, 0.09f, 0.0f, 809f / 1024, 78f / 1024),
                    new EditorVertex(3.0f, -0.5f, 0.0f, 1024f / 1024, 128f / 1024)
                }, Material.SolidNorm, "Trackbed1", true));
            poly.Flip();

            // Joints
            var jointRepParams = new Dictionary<string, float> {
                { "OriginalLength".ToLower(), 25.0f },
                { "IntervalLength".ToLower(), 25.0f },
                { "SubdivisionCount".ToLower(), 1f },
                { "InitialShift".ToLower(), 0f }
            };

            part = oldLod.AddPart(new EditorPart("Joint",
                new PartReplication(PartReplicationMethod.ByFixedIntervals, PartScalingMethod.FixLength, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                false, true, false, jointRepParams)));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.140f, 0.163f, -0.5f, 683f/ 2048, 24f/ 2048),
                    new EditorVertex(0.140f, 0.163f, 0.5f, 683f / 2048, 830f/ 2048),
                    new EditorVertex(0.150f, 0.280f, 0.5f, 585f / 2048, 830f/ 2048),
                    new EditorVertex(0.150f, 0.280f, -0.5f, 585f / 2048, 24f/ 2048)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.260f, 0.275f, -0.5f, 585f/ 2048, 24f/ 2048),
                    new EditorVertex(0.260f, 0.275f, 0.5f, 585f / 2048, 830f/ 2048),
                    new EditorVertex(0.255f, 0.160f, 0.5f, 683f / 2048, 830f/ 2048),
                    new EditorVertex(0.255f, 0.160f, -0.5f, 683f / 2048, 24f/ 2048)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.150f, 0.280f, -0.5f, 470f/ 2048, 24f/ 2048),
                    new EditorVertex(0.150f, 0.280f, 0.5f, 470f / 2048, 830f/ 2048),
                    new EditorVertex(0.260f, 0.275f, 0.5f, 530f / 2048, 830f/ 2048),
                    new EditorVertex(0.260f, 0.275f, -0.5f, 530f / 2048, 24f/ 2048)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 12.5f);
            oldLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);


            var newLod = shape.AddLod(new EditorLod(600));
            CopyParts(oldLod, newLod, new List<string> { "Ballast" });

            part = newLod.AddPart(new EditorPart("RailHead",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.173f, 0.326f, 0f, 244f/ 2048, 0),
                    new EditorVertex(0.173f, 0.326f, 10f, 244f / 2048, 2),
                    new EditorVertex(0.243f, 0.323f, 10f, 311f / 2048, 2),
                    new EditorVertex(0.243f, 0.323f, 0f, 311f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            newLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = newLod.AddPart(new EditorPart("RailSideOut",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.125f, 0.159f, 0f, 17f/ 2048, 0),
                    new EditorVertex(0.125f, 0.159f, 10f, 17f / 2048, 1),
                    new EditorVertex(0.188f, 0.172f, 10f, 95f / 2048, 1),
                    new EditorVertex(0.188f, 0.172f, 0f, 95f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.188f, 0.172f, 0f, 96f/ 2048, 0),
                    new EditorVertex(0.188f, 0.172f, 10f, 96f / 2048, 1),
                    new EditorVertex(0.197f, 0.300f, 10f, 195f / 2048, 1),
                    new EditorVertex(0.197f, 0.300f, 0f, 195f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            newLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = newLod.AddPart(new EditorPart("RailSideIn",
                new PartReplication(method, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, railRepParams)));
            part.Smoothed = true;
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.217f, 0.299f, 0f, 360f/ 2048, 0),
                    new EditorVertex(0.217f, 0.299f, 10f, 360f / 2048, 1),
                    new EditorVertex(0.216f, 0.170f, 10f, 462f / 2048, 1),
                    new EditorVertex(0.216f, 0.170f, 0f, 462f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.216f, 0.170f, 0f, 463f/ 2048, 0),
                    new EditorVertex(0.216f, 0.170f, 10f, 463f / 2048, 1),
                    new EditorVertex(0.276f, 0.153f, 10f, 541f / 2048, 1),
                    new EditorVertex(0.276f, 0.153f, 0f, 541f / 2048, 0)
                }, Material.SolidNorm, "Sleepers1", false));

            PartTransformer.ShiftPart(part, -1.003f, 0, 0);
            newLod.AddPart(part.Copy(false));
            PartTransformer.FlipPart(part, true, false, false);

            part = newLod.AddPart(new EditorPart("Sleepers",
                new PartReplication(sleepersMethod, PartScalingMethod.Stretch, PartStretchInWidthMethod.ReplicateAlongAllTracks,
                preserveTextureDim, true, false, repParams)));
            part.AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(-1.375f, 0.125f, 0.0f, 1004f/ 2048, 0),
                    new EditorVertex(-1.375f, 0.125f, 5.5f, 1004f / 2048, 1),
                    new EditorVertex(1.375f, 0.125f, 5.5f, 2044f / 2048, 1),
                    new EditorVertex(1.375f, 0.125f, 0.0f, 2044f / 2048, 0)
                }, Material.TransNorm, "Sleepers1", false));


            oldLod = newLod;
            newLod = shape.AddLod(new EditorLod(2000));
            CopyParts(oldLod, newLod, new List<string> { "BallastC", "Sleepers" });


            await GeneralMethods.SaveStringToFile(shape.ShapeName + ".csv", EditorShapeSerializer.MakeCsvFromEditorShape(shape));

            await BatchConverter.ConvertShape(shape.ShapeName + ".csv", "tsection.dat", "A*.s", "", 20);
        }

        private static void CopyParts(EditorLod oldLod, EditorLod newLod, List<string> partNames)
        {
            foreach (var name in partNames)
            {
                var part = oldLod.Parts.Find(p => p.PartName == name);

                if (part is not null)
                    newLod.Parts.Add(part.Copy(false));
            }
        }
    }
}

// A1t120r20d.s
// A1t10mStrt.s
// A1t3WayPnt10dMnl.s
// A1t45dYardRgt.s