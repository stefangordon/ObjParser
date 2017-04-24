using System;
using System.IO;
using NUnit.Framework;
using ObjParser;

namespace ObjParser_Tests
{
    [TestFixture]
    public class WriteObjTests
    {
        private Obj obj;
        private Mtl mtl;

        [SetUp]
        public void SetUp()
        {
            obj = new Obj();
            mtl = new Mtl();
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
                "f 1/1/1 2/2/1 3/3/1",
                "usemtl Material.001",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);
            obj.WriteObjFile(tempfilepath, headers);
            obj = new Obj();
            obj.LoadObj(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 4);
            Assert.IsTrue(obj.FaceList.Count == 3);
            Assert.AreEqual("Material", obj.FaceList[0].UseMtl);
            Assert.AreEqual("Material.001", obj.FaceList[1].UseMtl);
            Assert.AreEqual("Material.001", obj.FaceList[2].UseMtl);
            Assert.AreEqual(5.0711d, obj.TextureList[0].X);
            Assert.AreEqual(0.0003d, obj.TextureList[0].Y);
            Assert.AreEqual(5.4612d, obj.TextureList[1].X);
            Assert.AreEqual(1.0000d, obj.TextureList[1].Y);
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
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);
            obj.WriteObjFile(tempfilepath, headers);
            obj = new Obj();
            obj.LoadObj(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 4);
            Assert.IsTrue(obj.FaceList.Count == 3);
            Assert.IsNull(obj.FaceList[0].UseMtl);
            Assert.IsNull(obj.FaceList[1].UseMtl);
            Assert.IsNull(obj.FaceList[2].UseMtl);
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
                "Tf 0.000000 0.000000 0.000000",
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
            mtl = new Mtl();
            mtl.LoadMtl(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.AreEqual(2, mtl.MaterialList.Count);
            ObjParser.Types.Material first = mtl.MaterialList[0];
            Assert.AreEqual("Material", first.Name);
            Assert.AreEqual(96.078431f, first.SpecularExponent);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.r);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.g);
            Assert.AreEqual(1.0f, first.AmbientReflectivity.b);
            Assert.AreEqual(0.630388f, first.DiffuseReflectivity.r);
            Assert.AreEqual(0.620861f, first.DiffuseReflectivity.g);
            Assert.AreEqual(0.640000f, first.DiffuseReflectivity.b);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.r);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.g);
            Assert.AreEqual(0.5f, first.SpecularReflectivity.b);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.r);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.g);
            Assert.AreEqual(0.0f, first.EmissiveCoefficient.b);
            Assert.AreEqual(0.0f, first.TransmissionFilter.r);
            Assert.AreEqual(0.0f, first.TransmissionFilter.g);
            Assert.AreEqual(0.0f, first.TransmissionFilter.b);
            Assert.AreEqual(1.0f, first.OpticalDensity);
            Assert.AreEqual(1.0f, first.Dissolve);
            Assert.AreEqual(2, first.IlluminationModel);

            ObjParser.Types.Material second = mtl.MaterialList[1];
            Assert.AreEqual("Material.001", second.Name);
            Assert.AreEqual(96.078431f, second.SpecularExponent);
        }
        #endregion
    }
}
