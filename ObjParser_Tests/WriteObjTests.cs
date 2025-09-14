using System;
using System.IO;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using ObjParser;

namespace ObjParser_Tests
{
    [TestFixture]
    public class WriteObjTests
    {
        private ObjModel obj = null!;
        private MaterialLibrary mtl = null!;

        [SetUp]
        public void SetUp()
        {
            obj = new ObjModel();
            mtl = new MaterialLibrary();
        }

        #region Obj
        [Test]
        public void Obj_WriteObj_TwoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vt 5.0711 0.0003",
                "vt 5.4612 1.0000",
                "usemtl Material",
                "f 1//1 2//1 3//1",
                "usemtl Material.001",
                "f 1//1 2//1 3//1",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);
            obj.Save(tempfilepath, headers);
            obj = new ObjModel();
            obj.Load(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 3);
            Assert.AreEqual("Material", obj.Faces[0].MaterialName);
            Assert.AreEqual("Material.001", obj.Faces[1].MaterialName);
            Assert.AreEqual("Material.001", obj.Faces[2].MaterialName);
            Assert.AreEqual(5.0711d, obj.TextureVertices[0].X);
            Assert.AreEqual(0.0003d, obj.TextureVertices[0].Y);
            Assert.AreEqual(5.4612d, obj.TextureVertices[1].X);
            Assert.AreEqual(1.0000d, obj.TextureVertices[1].Y);
        }

        [Test]
        public void Obj_WriteObj_NoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "f 1//1 2//1 3//1",
                "f 1//1 2//1 3//1",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);
            obj.Save(tempfilepath, headers);
            obj = new ObjModel();
            obj.Load(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 3);
            Assert.IsNull(obj.Faces[0].MaterialName);
            Assert.IsNull(obj.Faces[1].MaterialName);
            Assert.IsNull(obj.Faces[2].MaterialName);
        }
        #endregion

        #region Groups/Smoothing/Object
        [Test]
        public void Obj_WriteObj_GroupsAndSmoothing_RoundTrip()
        {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange: build programmatically with groups/smoothing/object
            obj = new ObjModel();
            // vertices
            obj.Load(new[]
            {
                "v 0 0 0",
                "v 1 0 0",
                "v 0 1 0"
            });

            obj.SetGroups("G1");
            obj.SetSmoothingGroup("1");
            obj.SetObjectName("O1");

            var f1 = new ObjParser.Types.Face
            {
                VertexIndexList = new[] { 1, 2, 3 },
                TextureVertexIndexList = new int[] { 0, 0, 0 },
                NormalIndexList = new int[] { 0, 0, 0 }
            };
            obj.AddFace(f1);

            obj.SetGroups("G2", "G3");
            obj.ClearSmoothingGroup(); // should emit s off

            var f2 = new ObjParser.Types.Face
            {
                VertexIndexList = new[] { 1, 3, 2 },
                TextureVertexIndexList = new int[] { 0, 0, 0 },
                NormalIndexList = new int[] { 0, 0, 0 }
            };
            obj.AddFace(f2);

            obj.ClearGroups(); // should emit bare g to clear

            var f3 = new ObjParser.Types.Face
            {
                VertexIndexList = new[] { 2, 3, 1 },
                TextureVertexIndexList = new int[] { 0, 0, 0 },
                NormalIndexList = new int[] { 0, 0, 0 }
            };
            obj.AddFace(f3);

            // Act
            obj.Save(tempfilepath, headers);
            obj = new ObjModel();
            obj.Load(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.AreEqual(3, obj.Faces.Count);
            // Face 1
            Assert.AreEqual(new[] { "G1" }, obj.Faces[0].GroupNames);
            Assert.AreEqual("1", obj.Faces[0].SmoothingGroup);
            Assert.AreEqual("O1", obj.Faces[0].ObjectName);
            // Face 2
            Assert.AreEqual(new[] { "G2", "G3" }, obj.Faces[1].GroupNames);
            Assert.AreEqual("off", obj.Faces[1].SmoothingGroup);
            Assert.AreEqual("O1", obj.Faces[1].ObjectName);
            // Face 3: cleared group, smoothing remains off, object remains O1
            Assert.AreEqual(0, obj.Faces[2].GroupNames.Length);
            Assert.AreEqual("off", obj.Faces[2].SmoothingGroup);
            Assert.AreEqual("O1", obj.Faces[2].ObjectName);
        }

        [Test]
        public void Obj_WriteObj_MultipleGroups_OnLinesAndPoints_RoundTrip()
        {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            obj = new ObjModel();
            obj.Load(new[]
            {
                "v 0 0 0",
                "v 1 0 0",
                "v 1 1 0",
                "v 0 1 0",
                "vt 0 0",
                "vt 1 1"
            });

            obj.SetGroups("Edge", "Wire");
            var le = new ObjParser.Types.LineElement
            {
                VertexIndexList = new[] { 1, 2, 3, 4 },
                TextureVertexIndexList = new int[] { 0, 0, 0, 0 }
            };
            obj.AddLine(le);

            obj.SetGroups("Points");
            var pe = new ObjParser.Types.PointElement
            {
                VertexIndexList = new[] { 1, 3 }
            };
            obj.AddPoint(pe);

            // Act
            obj.Save(tempfilepath, headers);
            obj = new ObjModel();
            obj.Load(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.AreEqual(1, obj.Lines.Count);
            Assert.AreEqual(new[] { "Edge", "Wire" }, obj.Lines[0].GroupNames);
            Assert.AreEqual(1, obj.Points.Count);
            Assert.AreEqual(new[] { "Points" }, obj.Points[0].GroupNames);
        }
        #endregion

        #region Mtl
        [Test]
        public void Mtl_WriteMtl_TwoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var mtlFile = new[]
            {
                "newmtl Material",
                "Ns 96.078431",
                "Ka 1.000000 1.000000 1.000000",
                "Kd 0.630388 0.620861 0.640000",
                "Ks 0.500000 0.500000 0.500000",
                "Ke 0.000000 0.000000 0.000000",
                "Ni 1.000000",
                "d 1.000000",
                "illum 2",
                "",
                "newmtl Material.001",
                "Ns 96.078431",
                "Ka 1.000000 1.000000 1.000000",
                "Kd 0.640000 0.026578 0.014364",
                "Ks 0.500000 0.500000 0.500000",
                "Ke 0.000000 0.000000 0.000000",
                "Ni 1.000000",
                "d 1.000000",
                "illum 2"
            };

            // Act
            mtl.LoadMtl(mtlFile);
            mtl.WriteMtlFile(tempfilepath, headers);
            mtl = new MaterialLibrary();
            mtl.LoadMtl(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.AreEqual(2, mtl.Materials.Count);
            ObjParser.Types.Material first = mtl.Materials[0];
            Assert.AreEqual("Material", first.Name);
            Assert.AreEqual(96.078431f, first.SpecularExponent);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.R);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.G);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.B);
            Assert.AreEqual(0.630388f, first.DiffuseReflectivity.R);
            Assert.AreEqual(0.620861f, first.DiffuseReflectivity.G);
            Assert.AreEqual(0.640000f, first.DiffuseReflectivity.B);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.R);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.G);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.B);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.R);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.G);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.B);
            Assert.AreEqual(0.0f, first.TransmissionFilter.R);
            Assert.AreEqual(0.0f, first.TransmissionFilter.G);
            Assert.AreEqual(0.0f, first.TransmissionFilter.B);
            Assert.AreEqual(1.0f, first.OpticalDensity);
            Assert.AreEqual(1.0f, first.Dissolve);
            Assert.AreEqual(2, first.IlluminationModel);

            ObjParser.Types.Material second = mtl.Materials[1];
            Assert.AreEqual("Material.001", second.Name);
            Assert.AreEqual(96.078431f, second.SpecularExponent);
        }
        #endregion
    }
}
