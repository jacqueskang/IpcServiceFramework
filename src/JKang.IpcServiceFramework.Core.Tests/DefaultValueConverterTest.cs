using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Core.Tests.Fixtures;
using JKang.IpcServiceFramework.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace JKang.IpcServiceFramework.Core.Tests
{
    public class DefaultValueConverterTest
    {
        private readonly DefaultValueConverter _sut = new DefaultValueConverter();

        [Theory, AutoData]
        public void TryConvert_FloatToDouble(float expected)
        {
            bool succeed = _sut.TryConvert(expected, typeof(double), out object actual);

            Assert.True(succeed);
            Assert.IsType<double>(actual);
            Assert.Equal((double)expected, actual);
        }

        [Theory, AutoData]
        public void TryConvert_Int32ToInt64(int expected)
        {
            bool succeed = _sut.TryConvert(expected, typeof(long), out object actual);

            Assert.True(succeed);
            Assert.IsType<long>(actual);
            Assert.Equal((long)expected, actual);
        }

        [Theory, AutoData]
        public void TryConvert_SameType(DateTime expected)
        {
            bool succeed = _sut.TryConvert(expected, typeof(DateTime), out object actual);

            Assert.True(succeed);
            Assert.IsType<DateTime>(actual);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public void TryConvert_JObjectToComplexType(ComplexType expected)
        {
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(ComplexType), out object actual);

            Assert.True(succeed);
            Assert.IsType<ComplexType>(actual);
            Assert.Equal(expected.Int32Value, ((ComplexType)actual).Int32Value);
            Assert.Equal(expected.StringValue, ((ComplexType)actual).StringValue);
        }

        [Theory, AutoData]
        public void TryConvert_Int32Array(int[] expected)
        {
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(int[]), out object actual);

            Assert.True(succeed);
            Assert.IsType<int[]>(actual);
            Assert.Equal(expected, actual as int[]);
        }

        [Theory, AutoData]
        public void TryConvert_Int32List(List<int> expected)
        {
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(List<int>), out object actual);

            Assert.True(succeed);
            Assert.IsType<List<int>>(actual);
            Assert.Equal(expected, actual as List<int>);
        }

        [Theory, AutoData]
        public void TryConvert_ComplexTypeArray(ComplexType[] expected)
        {
            object jObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expected));

            bool succeed = _sut.TryConvert(jObj, typeof(ComplexType[]), out object actual);

            Assert.True(succeed);
            Assert.IsType<ComplexType[]>(actual);
            var actualArray = actual as ComplexType[];
            Assert.Equal(expected.Length, actualArray.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.NotNull(actualArray[i]);
                Assert.Equal(expected[i].Int32Value, actualArray[i].Int32Value);
                Assert.Equal(expected[i].StringValue, actualArray[i].StringValue);
            }
        }

        [Fact]
        public void TryConvert_DerivedTypeToBaseType()
        {
            bool succeed = _sut.TryConvert(new ComplexType(), typeof(IComplexType), out object actual);

            Assert.True(succeed);
            Assert.IsType<ComplexType>(actual);
        }

        [Theory, AutoData]
        public void TryConvert_StringToEnum(EnumType expected)
        {
            bool succeed = _sut.TryConvert(expected.ToString(), typeof(EnumType), out object actual);

            Assert.True(succeed);
            Assert.IsType<EnumType>(actual);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public void TryConvert_Int32ToEnum(EnumType expected)
        {
            bool succeed = _sut.TryConvert((int)expected, typeof(EnumType), out object actual);

            Assert.True(succeed);
            Assert.IsType<EnumType>(actual);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public void TryConvert_StringToGuid(Guid expected)
        {
            bool succeed = _sut.TryConvert(expected.ToString(), typeof(Guid), out object actual);

            Assert.True(succeed);
            Assert.IsType<Guid>(actual);
            Assert.Equal(expected, actual);
        }

        private T ParseTestData<T>(string valueData)
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

        private void PerformRoundTripTest<T>(T value, Action<T, T> assertAreEqual = null)
        {
            // Act
            bool succeed = _sut.TryConvert(value, typeof(string), out object intermediate);
            bool succeed2 = _sut.TryConvert(intermediate, typeof(T), out object final);

            // Assert
            Assert.True(succeed);
            Assert.True(succeed2);

            Assert.IsType<T>(final);

            if (assertAreEqual != null)
                assertAreEqual(value, (T)final);
            else
                Assert.Equal(value, final);
        }

        [Theory]
        [InlineData(nameof(Guid.Empty))]
        [InlineData(nameof(Guid.NewGuid))]
        public void TryConvert_RoundTripGuid(string valueData)
        {
            PerformRoundTripTest(ParseTestData<Guid>(valueData));
        }

        [Theory]
        [InlineData(nameof(TimeSpan.Zero))]
        [InlineData(nameof(TimeSpan.MinValue))]
        [InlineData(nameof(TimeSpan.MaxValue))]
        [InlineData("-00:00:05.9167374")]
        public void TryConvert_RoundTripTimeSpan(string valueData)
        {
            PerformRoundTripTest(ParseTestData<TimeSpan>(valueData));
        }

        [Theory]
        [InlineData(nameof(DateTime.Now))]
        [InlineData(nameof(DateTime.Today))]
        [InlineData(nameof(DateTime.MinValue))]
        [InlineData(nameof(DateTime.MaxValue))]
        [InlineData("2020-02-05 3:10:27 PM")]
        public void TryConvert_RoundTripDateTime(string valueData)
        {
            PerformRoundTripTest(ParseTestData<DateTime>(valueData), assertAreEqual: (x, y) => Assert.Equal(DateTime.SpecifyKind(x, DateTimeKind.Unspecified), DateTime.SpecifyKind(y, DateTimeKind.Unspecified)));
        }

        public interface IComplexType
        {
            int Int32Value { get; }
            string StringValue { get; }
        }

        public class ComplexType : IComplexType
        {
            public int Int32Value { get; set; }
            public string StringValue { get; set; }
        }

        public enum EnumType
        {
            FirstOption,
            SecondOption
        }
    }
}
