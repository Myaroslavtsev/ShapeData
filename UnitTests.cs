using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework; // requires NUnit 3.14.0, will upgrade tests later
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
            var newLod = shape.AddLod(new EditorLod(200));
            newLod.AddPart(new EditorPart(
                "TestPart",
                new ReplicationAtFixedPos(),
                true));
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
                new ReplicationStretchedByDeflection(0.15f),
                false));
            shape.AddLod(new EditorLod(2000));

            var serializer = new EditorShapeSerializer();
            var deserializer = new EditorShapeDeserializer();

            var csv = serializer.MakeCsvFromEditorShape(shape);

            //await SaveDebugFile("test.csv", csv);

            var deserializedShape = deserializer.MakeShapeFromCsv(csv);

            Assert.AreEqual(shape.ShapeName, deserializedShape.ShapeName);
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
                    Assert.AreEqual(shape.Lods[lod].Parts[part].ReplicationParams.ReplicationMetod, 
                        deserializedShape.Lods[lod].Parts[part].ReplicationParams.ReplicationMetod);
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
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].X,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].X, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Y,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Y, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Z,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].Z, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].U,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].U, 0.00001);
                            Assert.AreEqual(shape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].V,
                                deserializedShape.Lods[lod].Parts[part].Polygons[poly].Vertices[vertice].V, 0.00001);
                        }
                    }
                }
            }
        }

        private async Task SaveDebugFile(string filename, string data)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true))
            {
                await writer.WriteAsync(data);
            }
        }
    }
}
