using System;
using NUnit.Framework;
using ObjParser;

namespace ObjParser_Tests
{
    [TestFixture]
    public class LoadObjTests
    {
        private Obj obj;

        [SetUp]
        public void SetUp()
        {
            obj = new Obj();
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
            obj.LoadObj(objFile);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 1);
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
            obj.LoadObj(objFile);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 2);
        }

        [Test]
        public void LoadObj_EmptyObj_EmptyObjNoVertsNoFaces()
        {
            // Arrange
            var objFile = new string[] {};

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 0);
            Assert.IsTrue(obj.FaceList.Count == 0);
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
            Assert.That(() => obj.LoadObj(objFile), Throws.TypeOf<ArgumentException>());
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
            Assert.That(() => obj.LoadObj(objFile), Throws.TypeOf<ArgumentException>());
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
            Assert.That(() => obj.LoadObj(objFile), Throws.TypeOf<ArgumentException>());
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
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 4);
            Assert.IsTrue(obj.FaceList.Count == 1);
        }
        #endregion
    }
}