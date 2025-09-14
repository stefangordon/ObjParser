using System;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using ObjParser;

namespace ObjParser_Tests
{
    [TestFixture]
    public class LoadObjTests
    {
        private ObjModel obj = null!;
        private MaterialLibrary mtl = null!;

        [SetUp]
        public void SetUp()
        {
            obj = new ObjModel();
            mtl = new MaterialLibrary();
        }

        #region Vertex
        [Test]
        public void LoadObj_OneVert_OneVertCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 1);
        }

        [Test]
        public void LoadOBj_TwoVerts_TwoVertCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0",
                "v 1.0 1.0 1.0"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 2);
        }

        [Test]
        public void LoadObj_EmptyObj_EmptyObjNoVertsNoFaces()
        {
            // Arrange
            var objFile = new string[] {};

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 0);
            Assert.IsTrue(obj.Faces.Count == 0);
        }

        [Test]
        public void LoadObj_NoVertPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0",
                "v"
            };

            // Act

            // Assert
            Assert.That(() => obj.Load(objFile), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void LoadObj_CommaSeperatedVertPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                // Valid
                "v 0, 0, 0,",

                // Invalid
                "v 0.1, 0.1, 0.2,",
                "v 0.1, 0.1, 0.3,",
                "v 0.1, 0.1, 0.4,"
            };

            // Act

            // Assert
            Assert.That(() => obj.Load(objFile), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void LoadObj_LettersInsteadOfPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                "v a b c"
            };

            // Act

            // Assert
            Assert.That(() => obj.Load(objFile), Throws.TypeOf<ArgumentException>());
        }
        #endregion

        #region TextureVertex
        [Test]
        public void LoadObj_OneTextureVert_OneTextureVertCount() {
            // Arrange
            var objFile = new[]
            {
                "vt 0.0 0.0"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.TextureVertices.Count == 1);
        }

        [Test]
        public void LoadOBj_TwoTextureVerts_TwoTextureVertCount() {
            // Arrange
            var objFile = new[]
            {
                "vt 0.0 0.0",
                "vt 1.0 1.0"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.TextureVertices.Count == 2);
        }

        [Test]
        public void LoadOBj_TwoTextureVerts_TwoTextureVertValues() {
            // Arrange
            var objFile = new[]
            {
                "vt 5.0711 0.0003",
                "vt 5.4612 1.0000"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.TextureVertices.Count == 2);
            Assert.AreEqual(5.0711d, obj.TextureVertices[0].X);
            Assert.AreEqual(0.0003d, obj.TextureVertices[0].Y);
            Assert.AreEqual(5.4612d, obj.TextureVertices[1].X);
            Assert.AreEqual(1.0000d, obj.TextureVertices[1].Y);
        }
        #endregion

        #region Mtl
        [Test]
        public void Mtl_LoadMtl_TwoMaterials() {
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

        #region Face
        [Test]
        public void LoadObj_FourVertsSingleFace_FourVertsOneFaceCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vn 0.000000 0.000000 1.000000",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 1);
            Assert.IsNull(obj.Faces[0].MaterialName);
        }

        [Test]
        public void LoadObj_FourVertsThreeFace_TwoMaterialsCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vn 0.000000 0.000000 1.000000",
                "usemtl Material",
                "f 1//1 2//1 3//1",
                "usemtl Material.001",
                "f 1//1 2//1 3//1",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 3);
            Assert.AreEqual(obj.Faces[0].MaterialName, "Material");
            Assert.AreEqual(obj.Faces[1].MaterialName, "Material.001");
            Assert.AreEqual(obj.Faces[2].MaterialName, "Material.001");
        }

        [Test]
        public void LoadObj_FourVertsTwoFace_OneMaterialCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vn 0.000000 0.000000 1.000000",
                "f 1//1 2//1 3//1",
                "usemtl Material",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 2);
            Assert.IsNull(obj.Faces[0].MaterialName);
            Assert.AreEqual(obj.Faces[1].MaterialName, "Material");
        }

        [Test]
        public void LoadObj_FourVertsSingleFaceNoTextureVerts_FourVertsOneFaceCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vn 0.000000 0.000000 1.000000",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.IsTrue(obj.Vertices.Count == 4);
            Assert.IsTrue(obj.Faces.Count == 1);
        }

        [Test]
        public void LoadObj_NegativeVertexIndicesOnFace_AreResolvedPerSpec()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.000000 0.000000 0.000000",
                "v 1.000000 0.000000 0.000000",
                "v 0.000000 1.000000 0.000000",
                // Use negative indices to reference the three vertices above
                "f -3 -2 -1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.AreEqual(3, obj.Vertices.Count);
            Assert.AreEqual(1, obj.Faces.Count);
            Assert.AreEqual(new[] { 1, 2, 3 }, obj.Faces[0].VertexIndexList);
        }

        [Test]
        public void LoadObj_NegativeTextureIndicesOnFace_AreResolvedPerSpec()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.000000 0.000000 0.000000",
                "v 1.000000 0.000000 0.000000",
                "v 0.000000 1.000000 0.000000",
                "vt 0.0 0.0",
                "vt 0.5 0.5",
                "vt 1.0 1.0",
                // Negative vertex and texture vertex indices
                "f -3/-3 -2/-2 -1/-1"
            };

            // Act
            obj.Load(objFile);

            // Assert
            Assert.AreEqual(3, obj.Vertices.Count);
            Assert.AreEqual(3, obj.TextureVertices.Count);
            Assert.AreEqual(1, obj.Faces.Count);
            Assert.AreEqual(new[] { 1, 2, 3 }, obj.Faces[0].VertexIndexList);
            Assert.AreEqual(new[] { 1, 2, 3 }, obj.Faces[0].TextureVertexIndexList);
        }
        #endregion
    }
}
