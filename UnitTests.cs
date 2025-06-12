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
    [TestFixture]

    public class UnitTests
    {
        const string tsectionPath = "tsection.dat"; // local copy of build 00038 is used // D:\\Train\\GLOBAL\\

        private static Task<KujuTsectionDat> _td;

        private static Task<KujuTsectionDat> GetTsectionDat()
        {
            return _td ??= KujuTsectionParser.LoadTsection(tsectionPath);
        }

        [Test]
        public void TwoWayConversionTest() // public async Task TwoWayConversionTest() // for writing to .csv file
        {
            var shape = new EditorShape("TestShape");
            shape.ShapeComment = "Test; comment";
            var newLod = shape.AddLod(new EditorLod(200));
            newLod.AddPart(new EditorPart(
                "TestPart",
                PartReplication.NoReplication(),
                true));
            newLod.Parts[0].AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0.1001f, 0.2f, 0.3f, 0.4f, 0.5f),
                    new EditorVertex(1.1f, 1.2f, 1.3f, 1.4f, 1.5f),
                    new EditorVertex(2.1f, 2.2f, 2.3f, 2.4f, 2.5f)
                }));
            newLod.Parts[0].AddPolygon(new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(5.1f, 5.2f, 5.3f, 5.4f, 5.5f)
                }));
            newLod.AddPart(new EditorPart(
                "ReplicatedPart",
                new PartReplication(
                    PartReplicationMethod.AtFixedPos, 
                    PartScalingMethod.FixLengthOnly, 
                    PartStretchInWidthMethod.ReplicateAlongAllTracks, 
                    false, false, true),
                false));
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
                    Assert.AreEqual(shape.Lods[lod].Parts[part].Replication.ReplicationMethod,
                        deserializedShape.Lods[lod].Parts[part].Replication.ReplicationMethod);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].Replication.LeaveAtLeastOne,
                        deserializedShape.Lods[lod].Parts[part].Replication.LeaveAtLeastOne);
                    Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons.Count,
                        deserializedShape.Lods[lod].Parts[part].Polygons.Count);

                    for (var poly = 0; poly < shape.Lods[lod].Parts[part].Polygons.Count; poly++)
                    {
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
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].UvPosition.X,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].UvPosition.X, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].UvPosition.Y,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].UvPosition.Y, 0.00001);
                        }
                    }
                }
            }
        }

        // for Okrasa Ghia's tsection.dat build 00038
        // without 32 shapes having duplicated file names
        [TestCase(tsectionPath, true, 6050, 8709, TestName = "load tsection no roads")]
        [TestCase(tsectionPath, false, 6050, 11827, TestName = "load tsection with roads")]
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
        public void PlaneProjectionTestSimple()
        {
            var Polygon = new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(0, 0, 0, 0, 0),
                    new EditorVertex(0.2f, 0.2f, 0, 0, 0),
                    new EditorVertex(-0.2f, 0.4f, 0, 0, 0)
                });

            var points = Polygon.Vertices.Select(v => v.Position).ToList();

            var dots = Transfigurations.MakeSomeUVcoords(points);

            Assert.AreEqual(0, dots[0].U, 1e-5); Assert.AreEqual(1d, dots[0].V, 1e-5);
            Assert.AreEqual(2d / 3d, dots[1].U, 1e-5); Assert.AreEqual(1d, dots[1].V, 1e-5);
            Assert.AreEqual(1d / 3d, dots[2].U, 1e-5); Assert.AreEqual(0, dots[2].V, 1e-5);
        }

        [Test]
        public void PlaneProjectionTestDiagonal()
        {
            var Polygon = new EditorPolygon(
                new List<EditorVertex> {
                    new EditorVertex(10, 0, 0, 0, 0),
                    new EditorVertex(0, 10, 0, 0, 0),
                    new EditorVertex(-5, 15, 7, 0, 0),
                    new EditorVertex(5, 5, 12, 0, 0),
                    new EditorVertex(10, 0, 12, 0, 0)
                });

            var points = Polygon.Vertices.Select(v => v.Position).ToList();

            var dots = Transfigurations.MakeSomeUVcoords(points);

            Assert.AreEqual(0, dots[0].U, 1e-5); Assert.AreEqual(12d / 15d / Math.Sqrt(2), dots[0].V, 1e-5);
            Assert.AreEqual(2d / 3d, dots[1].U, 1e-5); Assert.AreEqual(12d / 15d / Math.Sqrt(2), dots[1].V, 1e-5);
            Assert.AreEqual(1d, dots[2].U, 1e-5); Assert.AreEqual(5d / 15d / Math.Sqrt(2), dots[2].V, 1e-5);
            Assert.AreEqual(1d / 3d, dots[3].U, 1e-5); Assert.AreEqual(0, dots[3].V, 1e-5);
            Assert.AreEqual(0, dots[4].U, 1e-5); Assert.AreEqual(0, dots[4].V, 1e-5);
        }

        [Test]
        public void DataBlockSimplePrintingTest()
        {
            var db = new DataBlock(
                "Blockname", 
                new List<string> { "1", "2" }, 
                null, 
                new List<string> { "3", "4" } );

            var sb = new StringBuilder();

            db.PrintBlock(sb);

            var s = sb.ToString();

            Assert.AreEqual("Blockname ( 1 2 ) 3 4", s.Substring(0, 21));
        }

        [Test]
        public void DataBlockComplexPrintingTest()
        {
            var db = new DataBlock(
                "Blockname",
                new List<string> { "1", "2" },
                new List<DataBlock> {
                    new DataBlock( "Subblock", new List<string>{ "7", "8" } )
                },
                new List<string> { "3", "4" }) ;

            var sb = new StringBuilder();

            db.PrintBlock(sb);

            var s = sb.ToString().Split("\r\n");

            Assert.AreEqual("Blockname ( 1 2", s[0].Substring(0, 15));
            Assert.AreEqual("Subblock ( 7 8 )", s[1].Substring(1, 16));
            Assert.AreEqual(") 3 4", s[2].Substring(0, 5));
        }

        [Test]
        public async Task ShapeCreationTest()
        {
            var editorShape = new EditorShape("ThreePolyShapeSmooth.s");

            var part = editorShape.Lods[0].AddPart(new EditorPart("Angle", PartReplication.NoReplication(), true));

            AddTheePolygons(part, 0.6f);

            var (s, sd) = KujuShapeBuilder.BuildShapeFile(editorShape);

            await GeneralMethods.SaveStringToFile(editorShape.ShapeName, s, DataFileFormat.UTF16LE);
            await GeneralMethods.SaveStringToFile(editorShape.ShapeName + 'd', sd, DataFileFormat.UTF16LE);

            // Assertion by program is impossible. Try opening shape in MSTS.
        }

        [Test]
        public async Task MultiLodShapeCreationTest()
        {
            var editorShape = new EditorShape("ThreePolyShapeWithLods.s");

            var part = editorShape.Lods[0].AddPart(new EditorPart("Angle", PartReplication.NoReplication(), false));

            AddTheePolygons(part, 2.4f);

            var lod = editorShape.AddLod(new EditorLod(100));

            part = lod.AddPart(new EditorPart("Angle", PartReplication.NoReplication(), false));

            AddTheePolygons(part, 1.2f);

            lod = editorShape.AddLod(new EditorLod(20));

            part = lod.AddPart(new EditorPart("Angle", PartReplication.NoReplication(), true));

            AddTheePolygons(part, 0.6f);

            var (s, sd) = KujuShapeBuilder.BuildShapeFile(editorShape);

            await GeneralMethods.SaveStringToFile(editorShape.ShapeName, s, DataFileFormat.UTF16LE);
            await GeneralMethods.SaveStringToFile(editorShape.ShapeName + 'd', sd, DataFileFormat.UTF16LE);

            // Assertion by program is impossible. Try opening shape in MSTS.
        }

        private static void AddTheePolygons(EditorPart part, float size)
        {
            part.AddPolygon(new EditorPolygon(new List<EditorVertex> {
                new EditorVertex(size, size, -size, 1, 0),
                new EditorVertex(size, size, size, 0, 1),
                new EditorVertex(size, -size, size, 0, 0)
            }));

            part.AddPolygon(new EditorPolygon(new List<EditorVertex> {
                new EditorVertex(size, -size, size, 0, 0),
                new EditorVertex(size, size, size, 0, 1),
                new EditorVertex(-size, size, size, 1, 0)
            }));

            part.AddPolygon(new EditorPolygon(new List<EditorVertex> {
                new EditorVertex(-size, size, size, 1, 0),
                new EditorVertex(size, size, size, 0, 1),
                new EditorVertex(size, size, -size, 0, 0)
            }));
        }
    }
}
