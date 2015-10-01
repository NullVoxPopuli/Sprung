using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToCSharp;

namespace ToCSharpTests
{
    [TestClass]
    public class SprungTest
    {
        #region test classes

        public class GenericClass<T>
        {
            public readonly double One;
            public GenericClass(double one)
            {
                One = one;
            }
        }
        public class MiniClass
        {
            public readonly double One;
            public readonly string Two;
            public MiniClass(double one, string two)
            {
                One = one;
                Two = two;
            }

            // This shouldn't be selected as the constructor of choice
            public MiniClass(double one)
            {
                One = one;
                Two = null;
            }
        }

        public class WithDate
        {
            public readonly DateTime Date;

            public WithDate(DateTime date)
            {
                Date = date;
            }
        }

        public class WithReadOnlyList
        {
            public readonly IReadOnlyList<double> Ro;

            public WithReadOnlyList(List<double> ro)
            {
                Ro = ro.AsReadOnly();
            }
        }

        public class WithReadOnlyList2
        {
            public readonly IReadOnlyList<MiniClass> Ro;

            public WithReadOnlyList2(List<MiniClass> ro)
            {
                Ro = ro.AsReadOnly();
            }
        }

        public class WithReadOnlyCollection
        {
            public readonly IReadOnlyCollection<MiniClass> Ro;

            public WithReadOnlyCollection(List<MiniClass> ro)
            {
                Ro = ro.AsReadOnly();
            }
        }




        public class MiniClassWithBoolean
        {

            public readonly bool One;
            public MiniClassWithBoolean(bool one)
            {
                One = one;
            }
        }

        public class MiniClassWithEnum
        {
            public enum Example
            {
                Wat,
                What,
                Whhahaat
            }

            public readonly Example One;

            public MiniClassWithEnum(Example one)
            {
                One = one;
            }
        }

        public class MiniClassWithList
        {
            public readonly double One;
            public readonly string Two;
            public readonly List<double> Three;
            public MiniClassWithList(double one, string two, List<double> three)
            {
                One = one;
                Two = two;
                Three = three;
            }
        }

        public class MiniClassWithObject
        {
            public readonly double One;
            public readonly string Two;
            public readonly MiniClass Three;
            public MiniClassWithObject(double one, string two, MiniClass three)
            {
                One = one;
                Two = two;
                Three = three;
            }
        }

        public class MiniClassWithObjectInList
        {
            public readonly double One;
            public readonly string Two;
            public readonly List<MiniClass> Three;
            public MiniClassWithObjectInList(double one, string two, List<MiniClass> three)
            {
                One = one;
                Two = two;
                Three = three;
            }
        }

        #endregion

        #region Build
        [TestMethod]
        public void Build_Simple_Object()
        {
            var obj = new MiniClass(1,
                                    "blah");
            var result = Sprung.Build(obj);

            var expected = "new MiniClass(one: 1, two: \"blah\")";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_With_Generic()
        {
            var obj = new GenericClass<MiniClass>(1);
            var result = Sprung.Build(obj);

            var expected = "new GenericClass<MiniClass>(one: 1)";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_Date()
        {
            var obj = new WithDate(new DateTime(2015, 11, 09, 13, 02, 59));
            var result = Sprung.Build(obj);

            var expected = "new WithDate(date: new DateTime(2015, 11, 9, 13, 2, 59))";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_WithReadOnlyList()
        {
            var obj = new WithReadOnlyList(new List<double>() { 1, 2, 3 });
            var result = Sprung.Build(obj);

            var expected = "new WithReadOnlyList(ro: new List<double>(){1, 2, 3})";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_WithReadOnlyList2()
        {
            var obj = new WithReadOnlyList2(new List<MiniClass>() { new MiniClass(1, "blah") });
            var result = Sprung.Build(obj);

            var expected = "new WithReadOnlyList2(ro: new List<MiniClass>(){new MiniClass(one: 1, two: \"blah\")})";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_WithReadOnlyCollection()
        {
            var obj = new WithReadOnlyCollection(new List<MiniClass>() { new MiniClass(1, "blah") });
            var result = Sprung.Build(obj);

            var expected = "new WithReadOnlyCollection(ro: new List<MiniClass>(){new MiniClass(one: 1, two: \"blah\")})";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_Null_Value()
        {
            var obj = new MiniClass(1,
                                    null);
            var result = Sprung.Build(obj);

            var expected = "new MiniClass(one: 1, two: null)";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_Boolean()
        {
            var obj = new MiniClassWithBoolean(false);
            var result = Sprung.Build(obj);

            var expected = "new MiniClassWithBoolean(one: false)";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_Enum()
        {
            var obj = new MiniClassWithEnum(MiniClassWithEnum.Example.Wat);
            var result = Sprung.Build(obj);

            var expected = "new MiniClassWithEnum(one: Example.Wat)";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_List()
        {
            var obj = new MiniClassWithList(1,
                                    "blah",
                                    new List<double>()
                                    {
                                        1,2,3,4
                                    });
            var result = Sprung.Build(obj);

            var expected = "new MiniClassWithList(one: 1, two: \"blah\", three: new List<double>(){1, 2, 3, 4})";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_NestedObject()
        {
            var obj = new MiniClassWithObject(1,
                                    "blah",
                                    new MiniClass(1, "blah"));
            var result = Sprung.Build(obj);

            var expected = "new MiniClassWithObject(one: 1, two: \"blah\", three: new MiniClass(one: 1, two: \"blah\"))";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Build_Simple_Object_With_NestedObject_InList()
        {
            var obj = new MiniClassWithObjectInList(1,
                                    "blah",
                                    new List<MiniClass>()
                                    {
                                        new MiniClass(1, "blah")
                                    });
            var result = Sprung.Build(obj);

            var expected = "new MiniClassWithObjectInList(one: 1, two: \"blah\", three: new List<MiniClass>(){new MiniClass(one: 1, two: \"blah\")})";

            Assert.AreEqual(expected, result);
        }
        #endregion

    }
}
