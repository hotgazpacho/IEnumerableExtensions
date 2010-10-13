using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using FizzWare.NBuilder;
using NUnit.Framework;

namespace IEnumerableExtensions.Tests
{
    [TestFixture]
    public class IEnumerableExtensionsTests
    {
        [Test]
        public void ToDataTable_SimpleObject_CreatesDataTableWithColumnsForProperties()
        {
            IEnumerable<PrimitiveTestObject> enumeration = Builder<PrimitiveTestObject>.CreateListOfSize(10).Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Id", typeof (int));
            expected.Columns.Add("Name", typeof (string));
            expected.Columns.Add("Amount", typeof (decimal));

            DataTable actual = enumeration.ToDataTable();

            AssertColumnsAreEqual(expected, actual);
        }

        [Test] 
        public void ToDataTable_SimpleObject_PopulatesTable()
        {
            IEnumerable<PrimitiveTestObject> enumeration = Builder<PrimitiveTestObject>.CreateListOfSize(10).Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Id", typeof(int));
            expected.Columns.Add("Name", typeof(string));
            expected.Columns.Add("Amount", typeof(decimal));

            foreach (var primitiveTestObject in enumeration)
            {
                expected.Rows.Add(new object[] {primitiveTestObject.Id, primitiveTestObject.Name, primitiveTestObject.Amount});
            }
            
            DataTable actual = enumeration.ToDataTable();

            AssertRowsAreEqual(expected, actual);
        }

        [Test]
        public void ToDataTable_CustomObject_CreatesDataTableWithColumnsForCustomTypedProperties()
        {
            IEnumerable<CustomObject> enumeration = Builder<CustomObject>
                .CreateListOfSize(10)
                .WhereAll().HaveDoneToThem(x => x.Foo = Builder<Foo>.CreateNew().Build())
                .Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Id", typeof (int));
            expected.Columns.Add("Name", typeof (string));
            expected.Columns.Add("Age", typeof (int));
            expected.Columns.Add("Bar", typeof (Bar));
            expected.Columns.Add("Hot", typeof (bool));

            DataTable actual = enumeration.ToDataTable(new[] { typeof(CustomObject).GetProperty("Foo") });

            AssertColumnsAreEqual(expected, actual);
        }

        [Test]
        public void ToDataTable_CustomObject_PopulatesTable() {
            IEnumerable<CustomObject> enumeration = Builder<CustomObject>
                .CreateListOfSize(10)
                .WhereAll().HaveDoneToThem(x => x.Foo = Builder<Foo>.CreateNew().Build())
                .And(x => x.Bar = Builder<Bar>.CreateNew().Build())
                .Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Id", typeof(int));
            expected.Columns.Add("Name", typeof(string));
            expected.Columns.Add("Age", typeof(int));
            expected.Columns.Add("Bar", typeof(Bar));
            expected.Columns.Add("Hot", typeof(bool));

            foreach (CustomObject customObject in enumeration)
            {
                expected.Rows.Add(new object[]
                                      {
                                          customObject.Id, customObject.Foo.Name, customObject.Foo.Age, customObject.Bar,
                                          customObject.Hot
                                      });
            }

            DataTable actual = enumeration.ToDataTable(new[] { typeof(CustomObject).GetProperty("Foo") });

            AssertRowsAreEqual(expected, actual);
        }

        [Test]
        public void ToDataTable_CreatesDataTableWithColumnsForExplicitlyDeclaredProperties()
        {
            IEnumerable<CustomObject> enumeration = Builder<CustomObject>
                .CreateListOfSize(10)
                .WhereAll().HaveDoneToThem(x => x.Foo = Builder<Foo>.CreateNew().Build())
                .Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Age", typeof(int));
            expected.Columns.Add("Tab", typeof(double));
            expected.Columns.Add("Hot", typeof(bool));

            PropertyInfo[] onlyTheseProperties = new PropertyInfo[]
                                                     {
                                                         typeof(Foo).GetProperty("Age"),
                                                         typeof(Bar).GetProperty("Tab"),
                                                         typeof(CustomObject).GetProperty("Hot")
                                                     };

            DataTable actual = enumeration.ToDataTableExplicit(onlyTheseProperties);

            AssertColumnsAreEqual(expected, actual);
        }

        [Test]
        public void ToDataTable_ExplicitlyDeclaredProperties_PopulatesTable() {
            IEnumerable<CustomObject> enumeration = Builder<CustomObject>
                .CreateListOfSize(10)
                .WhereAll().HaveDoneToThem(x => x.Foo = Builder<Foo>.CreateNew().Build())
                .And(x => x.Bar = Builder<Bar>.CreateNew().Build())
                .Build();

            DataTable expected = new DataTable();
            expected.Columns.Add("Age", typeof(int));
            expected.Columns.Add("Tab", typeof(double));
            expected.Columns.Add("Hot", typeof(bool));

            foreach (CustomObject customObject in enumeration)
            {
                expected.Rows.Add(new object[] { customObject.Foo.Age, customObject.Bar.Tab, customObject.Hot });
            }

            PropertyInfo[] onlyTheseProperties = new PropertyInfo[]
                                                     {
                                                         typeof(Foo).GetProperty("Age"),
                                                         typeof(Bar).GetProperty("Tab"),
                                                         typeof(CustomObject).GetProperty("Hot")
                                                     };

            DataTable actual = enumeration.ToDataTableExplicit(onlyTheseProperties);

            AssertRowsAreEqual(expected, actual);
        }

        internal void AssertColumnsAreEqual(DataTable expected, DataTable actual)
        {
            Assert.AreEqual(expected.Columns.Count, actual.Columns.Count, String.Format("Expected {0} Columns, Actual contained {1}", expected.Columns.Count, actual.Columns.Count));
            for (int i = 0; i < expected.Columns.Count; i++)
            {
                DataColumn expectedColumn = expected.Columns[i];
                DataColumn actualColumn = actual.Columns[i];
                Assert.AreEqual(expectedColumn.ColumnName, actualColumn.ColumnName);
                Assert.AreEqual(expectedColumn.DataType, actualColumn.DataType);
            }
        }

        private void AssertRowsAreEqual(DataTable expected, DataTable actual) {
            Assert.AreEqual(expected.Rows.Count, actual.Rows.Count);
            for (int i = 0; i < actual.Rows.Count; i++) {
                DataRow expectedRow = expected.Rows[i];
                DataRow actualRow = actual.Rows[i];
                foreach (DataColumn column in expected.Columns) {
                    Assert.AreEqual(
                        expectedRow[column.ColumnName],
                        actualRow[column.ColumnName],
                        String.Format("Column values are different for {0}", column.ColumnName)
                        );
                }
            }
        }

        class PrimitiveTestObject {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Amount { get; set; }
        }

        class CustomObject
        {
            public int Id { get; set; }
            public Foo Foo { get; set; }
            public Bar Bar { get; set; }
            public bool Hot { get; set; }

        }

        private class Foo
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private class Bar
        {
            public double Tab { get; set; }
            public bool PayableNow { get; set; }
        }
    }
}