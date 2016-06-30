using System;
using NUnit.Framework;
using ObjParser;

namespace ObjParser_Tests
{
    [TestFixture]
    public class ProcessLine_Tests
    {
        private Obj obj;

        [SetUp]
        public void SetUp()
        {
            obj = new Obj();
        }

        #region Vertex
        [Test]
        public void ProcessLine_SingleVert_ValidInput()
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
        public void ProcessLine_TwoVert_ValidInput()
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
        public void ProcessLine_EmptyFile_ValidInput()
        {
            // Arrange
            var objFile = new string[] {};

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.IsTrue(obj.VertexList.Count == 0);
        }

        [Test]
        public void ProcessLine_EmptyVertexLine_ValidInput()
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
        public void ProcessLine_CommaSeperated_InvalidInput()
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
        public void ProcessLine_LettersInsteadOfPositions_InvalidInput()
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
    }
}