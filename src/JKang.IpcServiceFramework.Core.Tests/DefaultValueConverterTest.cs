using JKang.IpcServiceFramework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

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

        class ComplexType
        {
            public int Int32Value { get; set; }
            public string StringValue { get; set; }
        }
    }
}
