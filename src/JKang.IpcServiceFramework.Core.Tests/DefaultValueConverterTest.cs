using JKang.IpcServiceFramework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        [TestMethod]
        public void TryConvert_DerivedTypeToBaseType()
        {
            bool succeed = _sut.TryConvert(new ComplexType(), typeof(IComplexType), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(ComplexType));
        }

        [TestMethod]
        public void TryConvert_StringToEnum()
        {
            EnumType expected = EnumType.SecondOption;

            bool succeed = _sut.TryConvert(expected.ToString(), typeof(EnumType), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(EnumType));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TryConvert_Int32ToEnum()
        {
            EnumType expected = EnumType.SecondOption;

            bool succeed = _sut.TryConvert((int)expected, typeof(EnumType), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(EnumType));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TryConvert_StringToGuid()
        {
            var expected = Guid.NewGuid();

            bool succeed = _sut.TryConvert(expected.ToString(), typeof(Guid), out object actual);

            Assert.IsTrue(succeed);
            Assert.IsInstanceOfType(actual, typeof(Guid));
            Assert.AreEqual(expected, actual);
        }

        T ParseTestData<T>(string valueData)
        {
            if (typeof(T).GetMember(valueData).FirstOrDefault() is MemberInfo member)
            {
                if (member is FieldInfo field)
                    return (T)field.GetValue(null);
                else if (member is PropertyInfo property)
                    return (T)property.GetValue(null);
                else if (member is MethodInfo method)
                    return (T)method.Invoke(null, null);
            }

            var parseMethod = typeof(T).GetMethod("Parse", new[] { typeof(string) });

            return (T)parseMethod.Invoke(null, new[] { valueData });
        }

        void PerformRoundTripTest<T>(T value, Action<T, T> assertAreEqual = null)
        {
            // Act
            bool succeed = _sut.TryConvert(value, typeof(string), out object intermediate);
            bool succeed2 = _sut.TryConvert(intermediate, typeof(T), out object final);

            // Assert
            Assert.IsTrue(succeed);
            Assert.IsTrue(succeed2);

            Assert.IsInstanceOfType(final, typeof(T));

            if (assertAreEqual != null)
                assertAreEqual(value, (T)final);
            else
                Assert.AreEqual(value, final);
        }

        [TestMethod]
        [DataRow(nameof(Guid.Empty))]
        [DataRow(nameof(Guid.NewGuid))]
        public void TryConvert_RoundTripGuid(string valueData)
        {
            PerformRoundTripTest(ParseTestData<Guid>(valueData));
        }

        [TestMethod]
        [DataRow(nameof(TimeSpan.Zero))]
        [DataRow(nameof(TimeSpan.MinValue))]
        [DataRow(nameof(TimeSpan.MaxValue))]
        [DataRow("-00:00:05.9167374")]
        public void TryConvert_RoundTripTimeSpan(string valueData)
        {
            PerformRoundTripTest(ParseTestData<TimeSpan>(valueData));
        }

        [TestMethod]
        [DataRow(nameof(DateTime.Now))]
        [DataRow(nameof(DateTime.Today))]
        [DataRow(nameof(DateTime.MinValue))]
        [DataRow(nameof(DateTime.MaxValue))]
        [DataRow("2020-02-05 3:10:27 PM")]
        public void TryConvert_RoundTripDateTime(string valueData)
        {
            PerformRoundTripTest(ParseTestData<DateTime>(valueData), assertAreEqual: (x, y) => Assert.AreEqual(DateTime.SpecifyKind(x, DateTimeKind.Unspecified), DateTime.SpecifyKind(y, DateTimeKind.Unspecified)));
        }

        interface IComplexType
        {
            int Int32Value { get; }
            string StringValue { get; }
        }

        class ComplexType : IComplexType
        {
            public int Int32Value { get; set; }
            public string StringValue { get; set; }
        }

        enum EnumType
        {
            FirstOption,
            SecondOption
        }
    }
}
