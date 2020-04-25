using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Core.Tests.Fixtures;
using JKang.IpcServiceFramework.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
