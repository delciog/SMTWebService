using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace SMTWebService
{
    public class Event
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string location { get; set; }
        public DateTime session { get; set; }
        public string description { get; set; }
        public DateTime dateCreated { get; set; }
    }

    public class EventList : IEnumerable<Event>
    {
        public List<Event> eventList { get; set; }

        public EventList()
        {
            eventList = new List<Event>();
        }

        public void Add(Event eventItem) {
            eventList.Add(eventItem);
        }

        public IEnumerator<Event> GetEnumerator()
        {
            return eventList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return eventList.GetEnumerator();
        }
    }

    public class Contact
    {
        public int id { get; set; }
        public string name { get; set; }
        public string jobTitle { get; set; }
        public string organisation { get; set; }
        public string notes { get; set; }
        public DateTime dateCreated { get; set; }
    }

    public class ContactList : IEnumerable<Contact>
    {
        public List<Contact> contactList { get; set; }

        public ContactList()
        {
            contactList = new List<Contact>();
        }

        public void Add(Contact contactItem)
        {
            contactList.Add(contactItem);
        }

        public IEnumerator<Contact> GetEnumerator()
        {
            return contactList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return contactList.GetEnumerator();
        }
    }
}
