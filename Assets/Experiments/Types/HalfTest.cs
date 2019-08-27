// #define UNSAFE_IS_ENABLED
// #define TEST_FRAMEWORK_INSTALLED

// Not yet implemented.
// Have to work out how to use test framework.
// currently even with test framework package installed assembly references are still not found.
#if TEST_FRAMEWORK_INSTALLED && UNSAFE_IS_ENABLED

using System;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace System.HalfTest
{   
    /// <summary>This is a test class for HalfTest and is intended to contain all HalfTest Unit Tests</summary>
    public class HalfTest
    { 
		/*
        [ClassInitialize()]
        public static void HalfTestInitialize(TestContext testContext)
        {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
		*/


#if UNSAFE_IS_ENABLED
		[UnityTest]
        public unsafe void TestAllPossibleHalfValues()
        {
            for (ushort i = ushort.MinValue; i < ushort.MaxValue; i++)
            {
                Half half1 = Half.ToHalf(i);
                Half half2 = (Half)((float)half1);

                Assert.IsTrue(half1.Equals(half2));
            }
        }
#endif

		/// <summary>A test for TryParse</summary>
		[UnityTest]
        public void TryParseTest1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("cs-CZ");

            string value = "1234,567e-2";
            float resultExpected = (float)12.34567f;
            
            bool expected = true;
            float result;
            bool actual = float.TryParse(value, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for TryParse</summary>
        [UnityTest]
        public void TryParseTest()
        {
            string value = "777";
            NumberStyles style = NumberStyles.None;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            Half result;
            Half resultExpected = (Half)777f;
            bool expected = true;
            bool actual = Half.TryParse(value, style, provider, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ToString</summary>
        [UnityTest]
        public void ToStringTest4()
        {
            Half target = Half.Epsilon;
            string format = "e";
            string expected = "5.960464e-008";
            string actual = target.ToString(format);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ToString</summary>
        [UnityTest]
        public void ToStringTest3()
        {
            Half target = (Half)333.333f;
            string format = "G";
            IFormatProvider formatProvider = CultureInfo.CreateSpecificCulture("cs-CZ");
            string expected = "333,25";
            string actual = target.ToString(format, formatProvider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ToString</summary>
        [UnityTest]
        public void ToStringTest2()
        {
            Half target = (Half)0.001f;
            IFormatProvider formatProvider = CultureInfo.CreateSpecificCulture("cs-CZ");
            string expected = "0,0009994507";
            string actual = target.ToString(formatProvider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ToString</summary>
        [UnityTest]
        public void ToStringTest1()
        {
            Half target = (Half)10000.00001f;
            string expected = "10000";
            string actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ToHalf</summary>
        [UnityTest]
        public void ToHalfTest1()
        {
            byte[] value = { 0x11, 0x22, 0x33, 0x44 };
            int startIndex = 1;
            Half expected = Half.ToHalf(0x3322);
            Half actual = Half.ToHalf(value, startIndex);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>A test for ToHalf</summary>
        [UnityTest]
        public void ToHalfTest()
        {
            ushort bits = 0x3322;
            Half expected = (Half)0.2229004f;
            Half actual = Half.ToHalf(bits);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for System.IConvertible.ToUInt64</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToUInt64Test()
        {
            IConvertible target = (Half)12345.999f;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            ulong expected = 12344;
            ulong actual = target.ToUInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for System.IConvertible.ToUInt32</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToUInt32Test()
        {
            IConvertible target = (Half)9999;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            uint expected = 9992;
            uint actual = target.ToUInt32(provider);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>A test for System.IConvertible.ToUInt16</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToUInt16Test()
        {
            IConvertible target = (Half)33.33;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            ushort expected = 33;
            ushort actual = target.ToUInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for System.IConvertible.ToType</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToTypeTest()
        {
            IConvertible target = (Half)111.111f;
            Type conversionType = typeof(double);
            IFormatProvider provider = CultureInfo.InvariantCulture;
            object expected = 111.0625;
            object actual = target.ToType(conversionType, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for System.IConvertible.ToString</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToStringTest()
        {
            IConvertible target = (Half)888.888;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            string expected = "888.5";
            string actual = target.ToString(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for System.IConvertible.ToSingle</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToSingleTest()
        {
            IConvertible target = (Half)55.77f;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            float expected = 55.75f;
            float actual = target.ToSingle(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToSByte
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToSByteTest()
        {
            IConvertible target = 123.5678f;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            sbyte expected = 124;
            sbyte actual = target.ToSByte(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt64
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToInt64Test()
        {
            IConvertible target = (Half)8562;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            long expected = 8560;
            long actual = target.ToInt64(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt32
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToInt32Test()
        {
            IConvertible target = (Half)555.5;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            int expected = 556;
            int actual = target.ToInt32(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToInt16
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToInt16Test()
        {
            IConvertible target = (Half)365;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            short expected = 365;
            short actual = target.ToInt16(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToChar
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToCharTest()
        {
            IConvertible target = (Half)64UL;
            IFormatProvider provider = CultureInfo.InvariantCulture;

            try
            {
                char actual = target.ToChar(provider);
                Assert.Fail();
            }
            catch (InvalidCastException) { }
        }

        /// <summary>
        ///A test for System.IConvertible.ToDouble
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToDoubleTest()
        {
            IConvertible target = Half.MaxValue;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            double expected = 65504;
            double actual = target.ToDouble(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToDecimal
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToDecimalTest()
        {
            IConvertible target = (Half)146.33f;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            Decimal expected = new Decimal(146.25f);
            Decimal actual = target.ToDecimal(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToDateTime
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToDateTimeTest()
        {
            IConvertible target = (Half)0;
            IFormatProvider provider = CultureInfo.InvariantCulture;

            try
            {
                DateTime actual = target.ToDateTime(provider);
                Assert.Fail();
            }
            catch (InvalidCastException) { }
        }

        /// <summary>
        ///A test for System.IConvertible.ToByte
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToByteTest()
        {
            IConvertible target = (Half)111;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            byte expected = 111;
            byte actual = target.ToByte(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.ToBoolean
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void ToBooleanTest()
        {
            IConvertible target = (Half)77;
            IFormatProvider provider = CultureInfo.InvariantCulture;
            bool expected = true;
            bool actual = target.ToBoolean(provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for System.IConvertible.GetTypeCode
        ///</summary>
        [UnityTest]
        // [DeploymentItem("System.Half.dll")]
        public void GetTypeCodeTest1()
        {
            IConvertible target = (Half)33;
            TypeCode expected = (TypeCode)255;
            TypeCode actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Subtract
        ///</summary>
        [UnityTest]
        public void SubtractTest()
        {
            Half half1 = (Half)1.12345f;
            Half half2 = (Half)0.01234f;
            Half expected = (Half)1.11111f;
            Half actual = Half.Subtract(half1, half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [UnityTest]
        public void SignTest()
        {
            Assert.AreEqual(1, Half.Sign((Half)333.5));
            Assert.AreEqual(1, Half.Sign(10));
            Assert.AreEqual(-1, Half.Sign((Half)(-333.5)));
            Assert.AreEqual(-1, Half.Sign(-10));
            Assert.AreEqual(0, Half.Sign(0));
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [UnityTest]
        public void ParseTest3()
        {
            string value = "112,456e-1";
            IFormatProvider provider = new CultureInfo("cs-CZ");
            Half expected = (Half)11.2456;
            Half actual = Half.Parse(value, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [UnityTest]
        public void ParseTest2()
        {
            string value = "55.55";
            Half expected = (Half)55.55;
            Half actual = Half.Parse(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [UnityTest]
        public void ParseTest1()
        {
            string value = "-1.063E-02";
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
            Half expected = (Half)(-0.01062775);
            Half actual  = Half.Parse(value, style, provider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [UnityTest]
        public void ParseTest()
        {
            string value = "-7";
            NumberStyles style = NumberStyles.Number;
            Half expected = (Half)(-7);
            Half actual = Half.Parse(value, style);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for op_UnaryPlus
        ///</summary>
        [UnityTest]
        public void op_UnaryPlusTest()
        {
            Half half = (Half)77;
            Half expected = (Half)77;
            Half actual = +(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_UnaryNegation
        ///</summary>
        [UnityTest]
        public void op_UnaryNegationTest()
        {
            Half half = (Half)77;
            Half expected = (Half)(-77);
            Half actual = -(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [UnityTest]
        public void op_SubtractionTest()
        {
            Half half1 = (Half)77.99;
            Half half2 = (Half)17.88;
            Half expected = (Half)60.0625;
            Half actual = (half1 - half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [UnityTest]
        public void op_MultiplyTest()
        {
            Half half1 = (Half)11.1;
            Half half2 = (Half)5;
            Half expected = (Half)55.46879;
            Half actual = (half1 * half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [UnityTest]
        public void op_LessThanOrEqualTest()
        {
            {
                Half half1 = (Half)111;
                Half half2 = (Half)120;
                bool expected = true;
                bool actual = (half1 <= half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = (Half)111;
                Half half2 = (Half)111;
                bool expected = true;
                bool actual = (half1 <= half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [UnityTest]
        public void op_LessThanTest()
        {
            {
                Half half1 = (Half)111;
                Half half2 = (Half)120;
                bool expected = true;
                bool actual = (half1 <= half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = (Half)111;
                Half half2 = (Half)111;
                bool expected = true;
                bool actual = (half1 <= half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [UnityTest]
        public void op_InequalityTest()
        {
            {
                Half half1 = (Half)0;
                Half half2 = (Half)1;
                bool expected = true;
                bool actual = (half1 != half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = Half.MaxValue;
                Half half2 = Half.MaxValue;
                bool expected = false;
                bool actual = (half1 != half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_Increment
        ///</summary>
        [UnityTest]
        public void op_IncrementTest()
        {
            Half half = (Half)125.33f;
            Half expected = (Half)126.33f;
            Half actual = ++(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest10()
        {
            Half value = (Half)55.55f;
            float expected = 55.53125f;
            float actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest9()
        {
            long value = 1295;
            Half expected = (Half)1295;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest8()
        {
            sbyte value = -15;
            Half expected = (Half)(-15);
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest7()
        {
            Half value = Half.Epsilon;
            double expected = 5.9604644775390625e-8;
            double actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest6()
        {
            short value = 15555;
            Half expected = (Half)15552;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest5()
        {
            byte value = 77;
            Half expected = (Half)77;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest4()
        {
            int value = 7777;
            Half expected = (Half)7776;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest3()
        {
            char value = '@';
            Half expected = 64;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest2()
        {
            ushort value = 546;
            Half expected = 546;
            Half actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest1()
        {
            ulong value = 123456UL;
            Half expected = Half.PositiveInfinity;
            Half actual = value;
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [UnityTest]
        public void op_ImplicitTest()
        {
            uint value = 728;
            Half expected = 728;
            Half actual;
            actual = value;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [UnityTest]
        public void op_GreaterThanOrEqualTest()
        {
            {
                Half half1 = (Half)111;
                Half half2 = (Half)120;
                bool expected = false;
                bool actual = (half1 >= half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = (Half)111;
                Half half2 = (Half)111;
                bool expected = true;
                bool actual = (half1 >= half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [UnityTest]
        public void op_GreaterThanTest()
        {
            {
                Half half1 = (Half)111;
                Half half2 = (Half)120;
                bool expected = false;
                bool actual = (half1 > half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = (Half)111;
                Half half2 = (Half)111;
                bool expected = false;
                bool actual = (half1 > half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest12()
        {
            Half value = 1245;
            uint expected = 1245;
            uint actual = ((uint)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest11()
        {
            Half value = 3333;
            ushort expected = 3332;
            ushort actual = ((ushort)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest10()
        {
            float value = 0.1234f;
            Half expected = (Half)0.1234f;
            Half actual = ((Half)(value));
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest9()
        {
            Half value = 9777;
            Decimal expected = 9776;
            Decimal actual = ((Decimal)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest8()
        {
            Half value = (Half)5.5;
            sbyte expected = 5;
            sbyte actual = ((sbyte)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest7()
        {
            Half value = 666;
            ulong expected = 666;
            ulong actual = ((ulong)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest6()
        {
            double value = -666.66;
            Half expected = (Half)(-666.66);
            Half actual = ((Half)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest5()
        {
            Half value = (Half)33.3;
            short expected = 33;
            short actual = ((short)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest4()
        {
            Half value = 12345;
            long expected = 12344;
            long actual = ((long)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest3()
        {
            Half value = (Half)15.15;
            int expected = 15;
            int actual = ((int)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest2()
        {
            Decimal value = new Decimal(333.1);
            Half expected = (Half)333.1;
            Half actual = ((Half)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest1()
        {
            Half value = (Half)(-77);
            byte expected = unchecked((byte)(-77));
            byte actual = ((byte)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [UnityTest]
        public void op_ExplicitTest()
        {
            Half value = 64;
            char expected = '@';
            char actual = ((char)(value));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [UnityTest]
        public void op_EqualityTest()
        {
            {
                Half half1 = Half.MaxValue;
                Half half2 = Half.MaxValue;
                bool expected = true;
                bool actual = (half1 == half2);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half1 = Half.NaN;
                Half half2 = Half.NaN;
                bool expected = false;
                bool actual = (half1 == half2);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [UnityTest]
        public void op_DivisionTest()
        {
            Half half1 = 333;
            Half half2 = 3;
            Half expected = 111;
            Half actual = (half1 / half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Decrement
        ///</summary>
        [UnityTest]
        public void op_DecrementTest()
        {
            Half half = 1234;
            Half expected = 1233;
            Half actual = --(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [UnityTest]
        public void op_AdditionTest()
        {
            Half half1 = (Half)1234.5f;
            Half half2 = (Half)1234.5f;
            Half expected = (Half)2469f;
            Half actual = (half1 + half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Negate
        ///</summary>
        [UnityTest]
        public void NegateTest()
        {
            Half half = new Half(658.51);
            Half expected = new Half(-658.51);
            Half actual = Half.Negate(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Multiply
        ///</summary>
        [UnityTest]
        public void MultiplyTest()
        {
            Half half1 = 7;
            Half half2 = 12;
            Half expected = 84;
            Half actual = Half.Multiply(half1, half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Min
        ///</summary>
        [UnityTest]
        public void MinTest()
        {
            Half val1 = -155;
            Half val2 = 155;
            Half expected = -155;
            Half actual = Half.Min(val1, val2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Max
        ///</summary>
        [UnityTest]
        public void MaxTest()
        {
            Half val1 = new Half(333);
            Half val2 = new Half(332);
            Half expected = new Half(333);
            Half actual = Half.Max(val1, val2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsPositiveInfinity
        ///</summary>
        [UnityTest]
        public void IsPositiveInfinityTest()
        {
            {
                Half half = Half.PositiveInfinity;
                bool expected = true;
                bool actual = Half.IsPositiveInfinity(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half = (Half)1234.5678f;
                bool expected = false;
                bool actual = Half.IsPositiveInfinity(half);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for IsNegativeInfinity
        ///</summary>
        [UnityTest]
        public void IsNegativeInfinityTest()
        {
            {
                Half half = Half.NegativeInfinity;
                bool expected = true;
                bool actual = Half.IsNegativeInfinity(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half = (Half)1234.5678f;
                bool expected = false;
                bool actual = Half.IsNegativeInfinity(half);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for IsNaN
        ///</summary>
        [UnityTest]
        public void IsNaNTest()
        {
            {
                Half half = Half.NaN;
                bool expected = true;
                bool actual = Half.IsNaN(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half = (Half)1234.5678f;
                bool expected = false;
                bool actual = Half.IsNaN(half);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for IsInfinity
        ///</summary>
        [UnityTest]
        public void IsInfinityTest()
        {
            {
                Half half = Half.NegativeInfinity;
                bool expected = true;
                bool actual = Half.IsInfinity(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half = Half.PositiveInfinity;
                bool expected = true;
                bool actual = Half.IsInfinity(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half half = (Half)1234.5678f;
                bool expected = false;
                bool actual = Half.IsInfinity(half);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for GetTypeCode
        ///</summary>
        [UnityTest]
        public void GetTypeCodeTest()
        {
            Half target = new Half();
            TypeCode expected = (TypeCode)255;
            TypeCode actual = target.GetTypeCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [UnityTest]
        public void GetHashCodeTest()
        {
            Half target = 777;
            int expected = 25106;
            int actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetBytes
        ///</summary>
        [UnityTest]
        public void GetBytesTest()
        {
            Half value = Half.ToHalf(0x1234);
            byte[] expected = {0x34, 0x12};
            byte[] actual = Half.GetBytes(value);
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);
        }

        /// <summary>
        ///A test for GetBits
        ///</summary>
        [UnityTest]
        public void GetBitsTest()
        {
            Half value = new Half(555.555);
            ushort expected = 24663;
            ushort actual = Half.GetBits(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [UnityTest]
        public void EqualsTest1()
        {
            {
                Half target = Half.MinValue;
                Half half = Half.MinValue;
                bool expected = true;
                bool actual = target.Equals(half);
                Assert.AreEqual(expected, actual);
            }
            {
                Half target = 12345;
                Half half = 12345;
                bool expected = true;
                bool actual = target.Equals(half);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [UnityTest]
        public void EqualsTest()
        {
            {
                Half target = new Half();
                object obj = new Single();
                bool expected = false;
                bool actual = target.Equals(obj);
                Assert.AreEqual(expected, actual);
            }
            {
                Half target = new Half();
                object obj = (Half)111;
                bool expected = false;
                bool actual = target.Equals(obj);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for Divide
        ///</summary>
        [UnityTest]
        public void DivideTest()
        {
            Half half1 = (Half)626.046f;
            Half half2 = (Half)8790.5f;
            Half expected = (Half)0.07122803f;
            Half actual = Half.Divide(half1, half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [UnityTest]
        public void CompareToTest1()
        {
            Half target = 1;
            Half half = 2;
            int expected = -1;
            int actual = target.CompareTo(half);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [UnityTest]
        public void CompareToTest()
        {
            Half target = 666;
            object obj = (Half)555;
            int expected = 1;
            int actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [UnityTest]
        public void AddTest()
        {
            Half half1 = (Half)33.33f;
            Half half2 = (Half)66.66f;
            Half expected = (Half)99.99f;
            Half actual = Half.Add(half1, half2);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Abs
        ///</summary>
        [UnityTest]
        public void AbsTest()
        {
            Half value = -55;
            Half expected = 55;
            Half actual = Half.Abs(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest6()
        {
            long value = 44;
            Half target = new Half(value);
            Assert.AreEqual((long)target,44);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest5()
        {
            int value = 789; // TODO: Initialize to an appropriate value
            Half target = new Half(value);
            Assert.AreEqual((int)target, 789);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest4()
        {
            float value = -0.1234f;
            Half target = new Half(value);
            Assert.AreEqual(target, (Half)(-0.1233521f));
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest3()
        {
            double value = 11.11;
            Half target = new Half(value);
            Assert.AreEqual((double)target, 11.109375);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest2()
        {
            ulong value = 99999999;
            Half target = new Half(value);
            Assert.AreEqual(target, Half.PositiveInfinity);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest1()
        {
            uint value = 3330;
            Half target = new Half(value);
            Assert.AreEqual((uint)target, (uint)3330);
        }

        /// <summary>
        ///A test for Half Constructor
        ///</summary>
        [UnityTest]
        public void HalfConstructorTest()
        {
            Decimal value = new Decimal(-11.11);
            Half target = new Half(value);
            Assert.AreEqual((Decimal)target, (Decimal)(-11.10938));
        }
    }
}
#endif