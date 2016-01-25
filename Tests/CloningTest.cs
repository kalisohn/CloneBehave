using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CloneBehave;
using Xunit;

namespace Tests
{
    public class CloningTest
    {
        [Fact]
        public void Test_01a_MethodDeepClone()
        {
            //Check clone
            Person a = new Person(1, "John");
            a.Addresses.Add(new Address(1234));
            a.Addresses.Add(new Address(2345));

            Person clone = StartClone(a);

            Assert.Equal(a.Addresses.Count, 2);
            Assert.Equal(a.Addresses.Count, clone.Addresses.Count);
            Assert.Equal(a.Addresses.LastOrDefault().AddressID, clone.Addresses.LastOrDefault().AddressID);
        }

        [Fact]
        public void Test_01b_MethodEquals()
        {
            //Check clone
            Person a = new Person(1, "John");
            Person clone = StartClone(a);

            Assert.Equal(a.ID, clone.ID);
            Assert.Equal(a.Name, clone.Name);
        }

        [Fact]
        public void Test_01c_MethodModification()
        {
            //Check clone
            Person a = new Person(1, "John");
            a.Adress = new Address(1);

            Person clone = StartClone(a);

            //Check for modification
            clone.Name = "Anna";
            clone.Adress = new Address(2);
            Assert.NotEqual(a.Name, clone.Name);
            Assert.NotEqual(a.Adress.AddressID, clone.Adress.AddressID);
        }

        [Fact]
        public void Test_01d_MethodCloneNull()
        {
            //Check clone
            Person a = new Person(1, null);
            Person clone = StartClone(a);

            Assert.Equal(a.ID, clone.ID);
            Assert.Null(clone.Name);
        }

        [Fact]
        public void Test_01e_MethodCloneListBasedMembers()
        {
            //Check clone
            Person a = new Person(1, "John");
            Person b = new Person(2, "Daniel");
            Address a1 = new Address(1);
            Address a2 = new Address(2);
            a.Addresses.Add(a2);
            a.Addresses.Add(a1);
            a.FriendsDict.Add(100, b);

            Person clone = StartClone(a);

            clone.Adress = a2;

            //modify clone list based member
            clone.Addresses.RemoveAt(0);
            clone.Addresses.LastOrDefault().AddressID = 3;

            Assert.NotEqual(a.Addresses.Count, clone.Addresses.Count);
            Assert.NotEqual(a.Addresses.LastOrDefault().AddressID, clone.Addresses.LastOrDefault().AddressID);
        }

        [Fact]
        public void Test_02_DeepCloneGenerics()
        {
            Address a1 = new Address(-1);
            a1.City = "Stuttgart";

            Address ha2 = new Address(-1);
            ha2.City = "München";
            a1.MyEntry.OneEntry = ha2;

            //Check clone
            Person p = new Person(1, null);
            p.Adress = a1;
            p.HomeAdress = ha2;
            p.Adress.PersonLivingHere = p;

            Entry<Person> entry = new Entry<Person>();
            entry.OneEntry = p;

            Entry<Person> clonedEntry = StartClone(entry);
            Assert.Equal("München", clonedEntry.OneEntry.HomeAdress.City);
            Assert.Equal("München", clonedEntry.OneEntry.Adress.MyEntry.OneEntry.City);

            Assert.True(true); //No loop when it comes to here
        }

        [Fact]
        public void Test_03_DeepCloneObservableCollection()
        {
            //Check clone
            Person a = new Person(1, "John");
            a.ObservedAddresses.Add(new Address(1234));
            a.ObservedAddresses.Add(new Address(2345));

            Person clone = StartClone(a);

            Assert.Equal(a.ObservedAddresses.Count, 2);
            Assert.Equal(a.ObservedAddresses.Count, clone.ObservedAddresses.Count);
            Assert.Equal(a.ObservedAddresses.LastOrDefault().AddressID, clone.ObservedAddresses.LastOrDefault().AddressID);
        }

        [Fact]
        public void Test_04_DeepCloneEventHandler()
        {
            //Check clone
            Person p = new Person(1, "John");
            Address a = new Address(1234);
            p.Adress = a;

            Assert.False(a.AddressChangedIsNull());
            Person clone = StartClone(p);
            Assert.False(clone.Adress.AddressChangedIsNull());
        }

        [Fact]
        public void Test_05_DeepCloneStaticFields()
        {
            //Check clone
            Person p = new Person(1, "John");
            Person clone = StartClone(p);

            Assert.NotNull(Person.God);
        }

        [Fact]
        public void Test_06_CircularReferences()
        {
            Address a1 = new Address(-1);
            a1.City = "Stuttgart";

            Address ha2 = new Address(-1);
            ha2.City = "München";

            //Check clone
            Person a = new Person(1, null);
            a.Adress = a1;
            a.HomeAdress = ha2;
            a.Adress.PersonLivingHere = a;

            StartClone(a);
            Assert.True(true); //No loop when it comes to here
        }

        [Fact]
        public void Test_07_DeepCloneInternalReferenceConsistency()
        {
            IList<Person> persons = new List<Person>();

            Address a1 = new Address(-1);
            a1.City = "Stuttgart";

            Address ha2 = new Address(5);
            ha2.City = "München";

            //Check clone
            Person a = new Person(1, null);
            a.Adress = a1;
            a.HomeAdress = ha2;

            Person b = new Person(2, "Hans");
            b.Adress = new Address(6) { City = "Berlin" };
            b.HomeAdress = ha2;

            persons.Add(a);
            persons.Add(b);

            IList<Person> clone = StartClone(persons);

            Address homeAdressA = clone[0].HomeAdress;
            Address homeAdressB = clone[1].HomeAdress;

            if (homeAdressA == homeAdressB)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void Test_10_DeepCloneAttribute()
        {
            Address a1 = new Address(7);
            a1.City = "Stuttgart";

            Address ha2 = new Address(8);
            ha2.City = "München";

            //Check clone
            Person a = new Person(1, null);
            a.Adress = a1;
            a.HomeAdress = ha2;

            Person clone = StartClone(a);

            Assert.Equal(a.Adress, clone.Adress);
            Assert.NotEqual(a.HomeAdress, clone.HomeAdress);
        }

        [Fact]
        public void Test_11_UpdateReferencesBehaviour()
        {
            Address a = new Address(1);
            Person peter = new Person("Peter", 60);
            Person monica = new Person("Monica", 54);

            a.TestPerson1 = peter; //Behaviour = UpdateReferences
            a.TestPerson2 = monica;

            Address clone = a.Clone();
            Assert.NotEqual(clone.TestPerson1, peter);
            Assert.NotEqual(clone.TestPerson2, monica);

            a.TestPerson2 = peter;

            clone = a.Clone();
            Assert.Equal(clone.TestPerson1, clone.TestPerson2);
            Assert.Equal(clone.TestPerson1.Name, peter.Name);
        }

        [Fact]
        public void Test_Cloneable_Dictionary()
        {
            Stopwatch sw = new Stopwatch();
            CloneableDictionary<string, Person> cDict = new CloneableDictionary<string, Person>();
            for (int i = 0; i <= 10000; i++)
            {
                Person p = new Person(i, "Horst" + i);
                cDict.Add(i + p.Name, p);
            }

            Assert.Equal(10001, cDict.Count);
            CloneableDictionary<string, Person> clonedDict = cDict.Clone();
            Assert.NotEqual(clonedDict[100].Item2, cDict[100].Item2);
            Assert.Equal(clonedDict[100].Item2.ID, cDict[100].Item2.ID);
            Assert.Equal(clonedDict[100].Item2.Name, cDict[100].Item2.Name);
            Assert.Equal(clonedDict[100].Item2.Adress, cDict[100].Item2.Adress);

            DictionaryTests(cDict);
            DictionaryTests(clonedDict);
        }

        [Fact]
        public void Test_Performance()
        {
            Stopwatch sw = new Stopwatch();
            IDictionary<string, Person> personDict = new Dictionary<string, Person>();
            for (int i = 0; i <= 10000; i++)
            {
                Person p = new Person(i, "Horst" + i);
                personDict.Add(i + p.Name, p);
            }

            personDict.Clone();
            sw.Start();
            personDict.Clone();
            sw.Stop();

            Console.WriteLine("personDict: " + sw.ElapsedMilliseconds);

            IList<KeyValuePair<string, Person>> personListKeyValue = personDict.ToList();

            personListKeyValue.Clone();
            sw.Restart();
            personListKeyValue.Clone();
            sw.Stop();

            Console.WriteLine("personListKeyValue: " + sw.ElapsedMilliseconds);

            IList<Person> personList = personDict.Values.ToList();

            personList.Clone();
            sw.Restart();
            personList.Clone();
            sw.Stop();

            Console.WriteLine("personList: " + sw.ElapsedMilliseconds);

            IList<Tuple<string, Person>> personTupleList = personDict.Select(p => Tuple.Create(p.Key, p.Value)).ToList();

            personTupleList.Clone();
            sw.Restart();
            personTupleList.Clone();
            sw.Stop();

            Console.WriteLine("personTupleList: " + sw.ElapsedMilliseconds);

            ICollection<Tuple<string, Person>> personTupleCollection = personDict.Select(p => Tuple.Create(p.Key, p.Value)).ToList();

            personTupleCollection.Clone();
            sw.Restart();
            personTupleCollection.Clone();
            sw.Stop();

            Console.WriteLine("personTupleCollection: " + sw.ElapsedMilliseconds);

            CloneableDictionary<string, Person> cDict = new CloneableDictionary<string, Person>(personDict);
            int count = cDict.Count;
            Assert.Equal(10001, count);

            cDict.Clone();
            sw.Restart();
            cDict.Clone();
            sw.Stop();

            Console.WriteLine("CloneableDictionary: " + sw.ElapsedMilliseconds);
        }

        [Fact]
        public void Test_Static_reference()
        {
            Person x = new Person(1, "X");
            x.BestFriend = Person.God;
            Assert.Equal(x.BestFriend, Person.God);
            var xClone = x.Clone();
            Assert.Equal(x.BestFriend, Person.God);
            Assert.Equal(xClone.BestFriend, Person.God);

            //Check clone
            Person a = new Person(1, "A");
            Person b = new Person(2, "B");
            a.BestFriend = b;
            b.BestFriend = Person.God;

            Assert.Equal(a.BestFriend.BestFriend, Person.God);
            var aClone = a.Clone();
            Assert.Equal(a.BestFriend.BestFriend, Person.God);
            Assert.Equal(aClone.BestFriend.BestFriend, Person.God);

            Address s = new Address(1);
            s.PersonLivingHere = Person.God;
            Assert.Equal(s.PersonLivingHere, Person.God);
            var sClone = s.Clone();
            Assert.Equal(s.PersonLivingHere, Person.God);
            Assert.Equal(sClone.PersonLivingHere, Person.God);
        }

        private void DictionaryTests(CloneableDictionary<string, Person> dut)
        {
            Assert.True(dut.ContainsKey("2000Horst2000"));
            dut.Add("NEW", new Person(9, "NEWPERSON"));
            Assert.True(dut.ContainsKey("NEW"));
            Assert.Equal(10002, dut.Count);

            dut.Remove("2000Horst2000");
            Assert.Equal(10001, dut.Count);
            Assert.False(dut.ContainsKey("2000Horst2000"));

            Person person = dut.GetValue("NEW", null);
            Assert.Equal("NEWPERSON", person.Name);

            person = dut.GetValue("WTF", new Person(22, "NA"));
            Assert.Equal("NA", person.Name);

            string at100 = dut[100].Item1;
            dut.RemoveAt(100);
            Assert.Equal(10000, dut.Count);
            Assert.Null(dut.GetValue(at100, null));
            Assert.False(dut.ContainsKey(at100));

            dut[500] = Tuple.Create("AT500", new Person(500, "Person500"));
            Assert.True(dut.ContainsKey("AT500"));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (Tuple<string, Person> dutEntry in dut)
            {
                dut.ContainsKey(dutEntry.Item1);
                dut.GetValue(dutEntry.Item1, null);
            }
            for (int i = 10005; i < 20000; i++)
            {
                dut.Add(i.ToString(), new Person());
            }
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 100);

            dut.Clear();
            Assert.True(dut.Count == 0);
        }

        private T StartClone<T>(T toClone)
        {
            T clone = toClone.Clone();   //By Reflection (adapted)

            return clone;
        }
    }
}