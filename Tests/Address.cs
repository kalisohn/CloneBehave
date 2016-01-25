using System;
using CloneBehave;

namespace Tests
{
    [Serializable]
    public class Address
    {
        private int _addressID;
        private string _city;
        private Entry<Address> _myEntry = new Entry<Address>();

        [DeepClone(DeepCloneBehavior.UpdateReferences)]
        private Person _personLivingHere;

        private string _street;

        [DeepClone(DeepCloneBehavior.UpdateReferences)]
        private Person _testPerson1;

        [DeepClone(DeepCloneBehavior.UpdateReferences)]
        private Person _testPerson2;

        public Address(int aid)
        {
            this.AddressID = aid;
        }

        public Address()
        {
        }

        public event EventHandler<AddressChangedEventArgs> AddressChanged;

        public static Address Empty
        {
            get { return new Address(-1); }
        }

        public int AddressID
        {
            get { return _addressID; }
            set { _addressID = value; }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        public Entry<Address> MyEntry
        {
            get { return _myEntry; }
            set { _myEntry = value; }
        }

        public Person PersonLivingHere
        {
            get { return _personLivingHere; }
            set { _personLivingHere = value; }
        }

        public string Street
        {
            get { return _street; }
            set { _street = value; }
        }

        public Person TestPerson1
        {
            get { return _testPerson1; }
            set { _testPerson1 = value; }
        }

        public Person TestPerson2
        {
            get { return _testPerson2; }
            set { _testPerson2 = value; }
        }

        public bool AddressChangedIsNull()
        {
            return AddressChanged == null;
        }

        public void RaiseAddressChanged()
        {
            if (AddressChanged != null)
            {
                AddressChanged(this, new AddressChangedEventArgs(this, this));
            }
        }
    }

    public class AddressChangedEventArgs : EventArgs
    {
        public AddressChangedEventArgs(Address previousAdr, Address currentAdr)
        {
            PreviousAdr = previousAdr;
            CurrentAdr = currentAdr;
        }

        public Address CurrentAdr { get; set; }

        public Address PreviousAdr { get; set; }
    }
}