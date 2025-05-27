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
        public async Task SerializerTEstAsync()
        {
            var shape = new EditorShape("TestShape");
            shape.AddLod(new EditorLod(2000));
            shape.Lods[0].AddPart(new EditorPart(
                "TestPart",
                new ReplicationAtFixedPos(),
                true));
            shape.Lods[0].Parts[0].AddPolygon(new EditorPolygon(0, 
                new List<EditorVertex> { 
                    new EditorVertex(0.1f, 0.2f, 0.3f, 0.4f, 0.5f),
                    new EditorVertex(1.1f, 1.2f, 1.3f, 1.4f, 1.5f),
                    new EditorVertex(2.1f, 2.2f, 2.3f, 2.4f, 2.5f)
                }));

            var serializer = new EditorShapeSerializers();

            var csv = serializer.MakeCsvFromEditorShape(shape);

            await SaveDebugFile("test.csv", csv);

            Assert.AreEqual(csv, "a");
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
