using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework; // requires NUnit 3.14.0, will upgrade tests later
using ShapeData.Kuju_tsection.dat;
using ShapeData.Geometry;
using ShapeData.Editor_shapes;
//using NUnit.Framework.Legacy; // for NUnit 4.0 and newer

namespace ShapeData
{
    [TestFixture]
 
    public class UnitTests
    {
        [Test]
        public void TwoWayConversionTest() // public async Task TwoWayConversionTest() // for writing .csv file
        {
            var shape = new EditorShape("TestShape");
            shape.ShapeComment = "Test; comment";
            var newLod = shape.AddLod(new EditorLod(200));
            newLod.AddPart(new EditorPart(
                "TestPart",
                new ReplicationAtFixedPos(),
                true, true));
            newLod.Parts[0].AddPolygon(new EditorPolygon(0, 
                new List<EditorVertex> { 
                    new EditorVertex(0.1001f, 0.2f, 0.3f, 0.4f, 0.5f),
                    new EditorVertex(1.1f, 1.2f, 1.3f, 1.4f, 1.5f),
                    new EditorVertex(2.1f, 2.2f, 2.3f, 2.4f, 2.5f)
                }));
            newLod.Parts[0].AddPolygon(new EditorPolygon(1,
                new List<EditorVertex> {
                    new EditorVertex(5.1f, 5.2f, 5.3f, 5.4f, 5.5f)
                }));
            newLod.AddPart(new EditorPart(
                "ReplicatedPart",
                new ReplicationStretchedByDeflection(0.2f, 0.15f),
                false, false));
            shape.AddLod(new EditorLod(2000));

            var csv = EditorShapeSerializer.MakeCsvFromEditorShape(shape);

            //await GeneralMethods.SaveStringToFile("test.csv", csv);

            var deserializedShape = EditorShapeDeserializer.MakeShapeFromCsv(csv);

            Assert.AreEqual(shape.ShapeName, deserializedShape.ShapeName);
            Assert.AreEqual("Test: comment", deserializedShape.ShapeComment);
            Assert.AreEqual(shape.Lods.Count, deserializedShape.Lods.Count);

            for (var lod = 0; lod < shape.Lods.Count; lod++)
            {
                Assert.AreEqual(shape.Lods[lod].Distance, 
                    deserializedShape.Lods[lod].Distance);
                Assert.AreEqual(shape.Lods[lod].Parts.Count, 
                    deserializedShape.Lods[lod].Parts.Count);

                for (var part = 0; part < shape.Lods[lod].Parts.Count; part++)
                {
                    Assert.AreEqual(shape.Lods[lod].Parts[part].PartName, 
                        deserializedShape.Lods[lod].Parts[part].PartName);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].Smoothed, 
                        deserializedShape.Lods[lod].Parts[part].Smoothed);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].ReplicationParams.ReplicationMethod, 
                        deserializedShape.Lods[lod].Parts[part].ReplicationParams.ReplicationMethod);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].ReplicationParams.LeaveAtLeastOnePart,
                        deserializedShape.Lods[lod].Parts[part].ReplicationParams.LeaveAtLeastOnePart);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons.Count, 
                        deserializedShape.Lods[lod].Parts[part].Polygons.Count);

                    for (var poly = 0; poly < shape.Lods[lod].Parts[part].Polygons.Count; poly++)
                    {
                        Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].PolygonId,
                            deserializedShape.Lods[lod].Parts[part].Polygons[poly].PolygonId);
                        Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].MaterialType,
                            deserializedShape.Lods[lod].Parts[part].Polygons[poly].MaterialType);
                        Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].TextureFilename,
                            deserializedShape.Lods[lod].Parts[part].Polygons[poly].TextureFilename);
                        Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices.Count,
                            deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices.Count);

                        for (var vertice = 0; vertice < shape.Lods[lod].Parts[part].Polygons[poly].Vertices.Count; vertice++)
                        {                            
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.X,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.X, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.Y,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.Y, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.Z,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Position.Z, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].U,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].U, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].V,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].V, 0.00001);
                        }
                    }
                }
            }
        }

        // for Okrasa Ghia's tsection.dat build 00038
        // without 32 shapes having duplicated file names
        [TestCase("D:\\Train\\GLOBAL\\tsection.dat", true, 6050, 8709, TestName = "load tsection no roads")] 
        [TestCase("D:\\Train\\GLOBAL\\tsection.dat", false, 6050, 11827, TestName = "load tsection with roads")] 
        public async Task LoadTsectionDat(string tsectionPath, bool skipRoadShapes, int trackSectionCount, int trackShapeCount)
        {
            var td = await KujuTsectionParser.LoadTsection(tsectionPath, skipRoadShapes);

            Assert.AreEqual(trackSectionCount, td.TrackSections.Count); 
            Assert.AreEqual(trackShapeCount, td.TrackShapes.Count);
        }

        [TestCase(0, 0, 0, 10, 0, 0, 0, 10, 0, TestName = "traj: forward 10m")]
        [TestCase(-5, -7, 0, 10, 0, 0, -5, 3, 0, TestName = "traj: forward 10m from non-zero start")]
        [TestCase(0, 0, 90, 10, 0, 0, -10, 0, 90, TestName = "traj: left 10m")]
        [TestCase(0, 0, 45, 14.14213562373095, 0, 0, -10, 10, 45, TestName = "traj: diagonal -14,142m")]
        [TestCase(0, 0, 0, 0, 10, 90, -10, 10, 90, TestName = "traj: turn left r10 90d")]
        [TestCase(0, 0, 0, 0, 10, -90, 10, 10, 270, TestName = "traj: turn right r10 90d")]
        [TestCase(0, 0, 0, 0, 14.14213562373095, 45, -4.14213562373, 10, 45, TestName = "traj: turn left r14,142 45d")]        
        [TestCase(0, 0, 0, 10, 10, 90, -10, 20, 90, TestName = "traj: forward 10m, then turn left r10 90d")]
        [TestCase(0, 0, 90, 10, 10, -90, -20, 10, 0, TestName = "traj: left 10m, then turn right r10 90d")]
        [TestCase(0, 0, -90, 5, 14.14213562373095, -45, 15, -4.14213562373, 225, TestName = "traj: turn right r14,142 45d")]
        public void TrajectoryEndDirectionTest(
            double X0,
            double Z0,
            double A0,
            double straight,
            double radius,
            double angle,
            double X1,
            double Z1,
            double A1)
        {
            var startDirection = new Direction(X0, 0, Z0, A0);

            var trajectory = new Trajectory(straight, radius, angle);

            var editorTrackSection = new EditorTrackSection(startDirection, trajectory);

            Assert.AreEqual(X1, editorTrackSection.EndDirection.X, 1e-5);
            Assert.AreEqual(Z1, editorTrackSection.EndDirection.Z, 1e-5);
            Assert.AreEqual(A1, editorTrackSection.EndDirection.A, 1e-5);
        }

        [Test]
        public async Task ReplicationTest()
        {
            var td = await KujuTsectionParser.LoadTsection("D:\\Train\\GLOBAL\\tsection.dat");

            var section = ShapeReplication.GetSectionsFromShape(td.TrackShapes["SR_2tCrv_c_00150r20d.s"], td);

            Assert.AreEqual(section, section);
        }
    }
}
