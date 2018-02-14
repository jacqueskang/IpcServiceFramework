using JKang.IpcServiceFramework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JKang.IpcServiceFramework.Core.Tests
{
    [TestClass]
    public class DefaultValueConverterTest
    {
        private DefaultValueConverter _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new DefaultValueConverter();
        }

        [TestMethod]
        public void TryConvert_FloatToDouble()
        {
            float expected = 123.4f;

            bool succeed = _sut.TryConvert(expected, typeof(double), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(double));
            Assert.AreEqual((double)expected, actual);
        }

        [TestMethod]
        public void TryConvert_Int32ToInt64()
        {
            int expected = 123;

            bool succeed = _sut.TryConvert(expected, typeof(long), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(long));
            Assert.AreEqual((long)expected, actual);
        }

        [TestMethod]
        public void TryConvert_SameType()
        {
            DateTime expected = DateTime.UtcNow;

            bool succeed = _sut.TryConvert(expected, typeof(DateTime), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(DateTime));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TryConvert_JObjectToComplexType()
        {
            var expected = new ComplexType
            {
                Int32Value = 123,
                StringValue = "hello"
            };
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(ComplexType), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(ComplexType));
            Assert.AreEqual(expected.Int32Value, ((ComplexType)actual).Int32Value);
            Assert.AreEqual(expected.StringValue, ((ComplexType)actual).StringValue);
        }

        [TestMethod]
        public void TryConvert_Int32Array()
        {
            int[] expected = new[] { 1, 2 };
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(int[]), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(int[]));
            var actualArray = actual as int[];
            Assert.AreEqual(expected.Length, actualArray.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actualArray[i]);
            }
        }

        [TestMethod]
        public void TryConvert_Int32List()
        {
            var expected = new List<int> { 1, 2 };
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(List<int>), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(List<int>));
            var actualList = actual as List<int>;
            Assert.AreEqual(expected.Count, actualList.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], actualList[i]);
            }
        }

        [TestMethod]
        public void TryConvert_ComplexTypeArray()
        {
            ComplexType[] expected = new[]
            {
                new ComplexType { Int32Value = 123, StringValue = "abc" },
                new ComplexType { Int32Value = 456, StringValue = "edf" },
            };
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(ComplexType[]), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(ComplexType[]));
            var actualArray = actual as ComplexType[];
            Assert.AreEqual(expected.Length, actualArray.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.IsNotNull(actualArray[i]);
                Assert.AreEqual(expected[i].Int32Value, actualArray[i].Int32Value);
                Assert.AreEqual(expected[i].StringValue, actualArray[i].StringValue);
            }
        }

        class ComplexType
        {
            public int Int32Value { get; set; }
            public string StringValue { get; set; }
        }
    }
}
