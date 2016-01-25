using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CloneBehave;

namespace Tests
{
    [Serializable]
    public class Person
    {
        public readonly static Person God = new Person(0, "God");

        [DeepClone(DeepCloneBehavior.Shallow)]
        private Address _adress = Address.Empty;

        private string _privateName; //To have an uninitialized field in Test Model

        public Person()
        {
            HomeAdress = Address.Empty;
            FriendsDict = new Dictionary<int, Person>();
            Addresses = new List<Address>();
            ObservedAddresses = new ObservableCollection<Address>();
            Friends = new List<Person>();
        }

        public Person(string name, int age)
            : this()
        {
            Name = name;
            Age = age;
        }

        public Person(int id, string name)
            : this()
        {
            ID = id;
            Name = name;
            _privateName = name + "-private";
        }

        public List<Address> Addresses { get; set; }

        public Address Adress
        {
            get { return _adress; }
            set
            {
                _adress = value;
                _adress.AddressChanged += AdressOnAdressChanged;
            }
        }

        public int Age { get; set; }

        public Person BestFriend { get; set; }

        public IList<Person> Friends { get; set; }

        public Dictionary<int, Person> FriendsDict { get; set; }

        public Address HomeAdress { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public ObservableCollection<Address> ObservedAddresses { get; set; }

        private void AdressOnAdressChanged(object sender, AddressChangedEventArgs addressChangedEventArgs)
        {
            //left empty by purpose
        }
    }
}