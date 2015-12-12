using System;
using System.Collections.Generic;
using System.Data.SqlClient;
//using System.Linq;
using System.Web;
using System.Web.Services;

namespace SMTWebService
{
    /// <summary>
    /// Summary description for SMT
    /// </summary>
    [WebService(Namespace = "http://SMTWebservice/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SMT : System.Web.Services.WebService
    {
        public String connectionString = "Password=password;Persist Security Info=True;User ID=user;Initial Catalog=SMT;Data Source=host";
        public enum queryTimeOption
        {
            Past,
            Present,
            Future,
            All
        }

        #region "App"
        [WebMethod] 
        public int login(string username, string password)
        {
            // Return value
            int userId = -1;

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT id FROM [User] where username = '" + username + "' AND password = '" + password + "' AND active = 1";
            // Assign query to command
            cmd.CommandText = query;
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    userId = (int)dr["id"];
                }
            }

            // Close data reader and connection to database 
            dr.Close();
            conn.Close();

            return userId;
        }
        #endregion

        #region "Events"
        [WebMethod] 
        public EventList listEvents(queryTimeOption eventOption)
        {
            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT * FROM Event";
            switch (eventOption)
            {
                case queryTimeOption.Past:
                    query += @" WHERE date_time < GETUTCDATE()";
                    break;
                case queryTimeOption.Present:
                    query += @" WHERE CONVERT(CHAR(10), date_time, 126) = CONVERT(CHAR(10),  GETUTCDATE(), 126)";
                    break;
                case queryTimeOption.Future:
                    query += @" WHERE CONVERT(CHAR(10), date_time, 126) >= CONVERT(CHAR(10),  GETUTCDATE(), 126)";
                    break;
                case queryTimeOption.All:
                    // No condition to apply
                    break;
                default:
                    break;
            }

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            EventList events = new EventList();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Event eventRecord = new Event();

                    eventRecord.id = (int)dr["id"];
                    eventRecord.name = (String)dr["name"];
                    eventRecord.description = (String)dr["description"];
                    eventRecord.session = (DateTime)dr["date_time"];
                    eventRecord.url = (String)dr["url"];
                    eventRecord.location = ((int)dr["venue_id"]).ToString(); // Have this from a view
                    eventRecord.dateCreated = (DateTime)dr["date_created"];

                    events.eventList.Add(eventRecord);
                }
            }

            // Close connection to database
            conn.Close();

            return events;
        }

        [WebMethod] 
        public EventList listEventsByContact(String contactId, queryTimeOption eventOption)
        {
            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT PersonEvent.id AS person_id, Event.id AS event_id, Event.name, Event.description, Event.url, Event.venue_id, Event.date_time, Event.date_created ";
            query += "FROM PersonEvent INNER JOIN dbo.Event ON dbo.PersonEvent.event_id = dbo.Event.id";
            switch (eventOption)
            {
                case queryTimeOption.Past:
                    query += @" WHERE Event.date_time < GETUTCDATE()";
                    break;
                case queryTimeOption.Present:
                    query += @" WHERE CONVERT(CHAR(10), Event.date_time, 126) = CONVERT(CHAR(10),  GETUTCDATE(), 126)";
                    break;
                case queryTimeOption.Future:
                    query += @" WHERE Event.date_time > GETUTCDATE()";
                    break;
                case queryTimeOption.All:
                    // No condition to apply
                    break;
                default:
                    break;
            }
            if (eventOption == queryTimeOption.All)
            {
                query += " WHERE person_id = " + contactId;
            }
            else
            {
                query += " AND person_id = " + contactId;
            }

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            EventList events = new EventList();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Event eventRecord = new Event();

                    eventRecord.id = (int)dr["event_id"];
                    eventRecord.name = dr["name"].ToString();
                    eventRecord.description = dr["description"].ToString();
                    eventRecord.session = (DateTime)dr["date_time"];
                    eventRecord.url = dr["url"].ToString();
                    eventRecord.location = ((int)dr["venue_id"]).ToString(); // Have this from a view
                    eventRecord.dateCreated = (DateTime)dr["date_created"];

                    events.eventList.Add(eventRecord);
                }
            }

            // Close connection to database
            conn.Close();

            return events;
        }

        [WebMethod] 
        public Event getEventById(string eventId)
        {
            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT Event.id, Event.name AS name, Event.url, Event.date_time, Event.description, ";
            query += "Event.date_created, Venue.name AS venue_id FROM dbo.Event ";
            query += "INNER JOIN dbo.Venue ON dbo.Event.venue_id = dbo.Venue.id WHERE Event.id = " + eventId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Event eventRecord = new Event();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while(dr.Read())
                {
                    eventRecord.id = (int)dr["id"];
                    eventRecord.name = dr["name"].ToString();
                    eventRecord.description = dr["description"].ToString();
                    eventRecord.session = (DateTime)dr["date_time"];
                    eventRecord.url = dr["url"].ToString();
                    eventRecord.location = dr["venue_id"].ToString();
                    eventRecord.dateCreated = (DateTime)dr["date_created"];
                }
            }

            // Close connection to database
            conn.Close();

            return eventRecord;
        }

        [WebMethod] 
        public bool toggleAttendEvent(string eventId, string personId)
        {
            bool attending = false;

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT * FROM PersonEvent WHERE person_id = " + personId + " AND event_id = " + eventId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Event eventRecord = new Event();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows) // If contains row, delete it
            {
                // Get ID of record to delete
                while (dr.Read())
                {
                    eventRecord.id = (int)dr["id"];
                }

                // Delete record
                query = @"DELETE FROM PersonEvent WHERE person_id = " + personId + " AND event_id = " + eventId;

                // Return false
                attending = false;
            }
            else
            {
                // Create record
                query = @"INSERT INTO PersonEvent (person_id, event_id) values (" + personId + ", " + eventId + ")";
                attending = true;
            }

            // Execute INSERT or DELETE based on conditions above
            dr.Close();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            // Close connection to database
            conn.Close();

            return attending;
        }

        [WebMethod] 
        public bool isAttendingEvent(string eventId, string personId)
        {
            bool attending = false;

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT 1 FROM PersonEvent WHERE person_id = " + personId + " AND event_id = " + eventId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Event eventRecord = new Event();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows) // If contains row, delete it
            {
                // Return true
                attending = true;
            }
            else
            {
                // Return false
                attending = false;
            }

            dr.Close();
            dr.Dispose();

            // Close connection to database
            conn.Close();

            return attending;
        }
        
        //[WebMethod] // Not is use
        //public EventList searchEventByName(String eventId, queryTimeOption eventOption)
        //{
        //    Event testEvent1 = new Event();
        //    testEvent1.name = "Test Event";
        //    testEvent1.description = "First event";
        //    testEvent1.session = DateTime.UtcNow;

        //    Event testEvent2 = new Event();
        //    testEvent2.name = "Test Event 2";
        //    testEvent2.description = "Second event";
        //    testEvent2.session = DateTime.UtcNow;

        //    Event testEvent3 = new Event();
        //    testEvent3.name = "Test Event 3";
        //    testEvent3.description = "Third event";
        //    testEvent3.session = DateTime.UtcNow;

        //    EventList events = new EventList();
        //    events.eventList.Add(testEvent1);
        //    events.eventList.Add(testEvent2);
        //    events.eventList.Add(testEvent3);

        //    return events;
        //}
        #endregion

        #region "Contacts"
        [WebMethod] 
        public ContactList listContacts()
        {
            // List all contacts

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT * from Person WHERE active = 1";

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            ContactList contacts = new ContactList();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Contact contactRecord = new Contact();

                    contactRecord.id = (int)dr["id"];
                    contactRecord.name = dr["name"].ToString();
                    contactRecord.jobTitle = dr["job_title"].ToString();
                    contactRecord.organisation = dr["organisation"].ToString();
                    contactRecord.dateCreated = (DateTime)dr["date_created"];
                    contactRecord.notes = dr["notes"].ToString();

                    contacts.contactList.Add(contactRecord);
                }
            }

            // Close connection to database
            conn.Close();

            return contacts;
        }

        [WebMethod] 
        public ContactList listContactsMet(string userId)
        {
            // List all contacts the user met

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT PersonPerson.person_id AS user_id, Person.id, Person.name, Person.job_title, Person.organisation, Person.date_created, Person.notes ";
            query += "FROM Person INNER JOIN PersonPerson ON Person.id = PersonPerson.person_met_id ";
            query += "WHERE Person.active = 1 and PersonPerson.person_id = " + userId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            ContactList contacts = new ContactList();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Contact contactRecord = new Contact();

                    contactRecord.id = (int)dr["id"];
                    contactRecord.name = dr["name"].ToString();
                    contactRecord.jobTitle = dr["job_title"].ToString();
                    contactRecord.organisation = dr["organisation"].ToString();
                    contactRecord.dateCreated = (DateTime)dr["date_created"];
                    contactRecord.notes = dr["notes"].ToString();

                    contacts.contactList.Add(contactRecord);
                }
            }

            // Close connection to database
            conn.Close();

            return contacts;
        }

        [WebMethod] 
        public ContactList listContactsInEvent(string eventId, queryTimeOption eventOption)
        {
            // List all contacts attending an event
            
            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT PersonEvent.event_id, Person.id AS contact_id, Person.name, Person.job_title, Person.active, Person.organisation, ";
            query += "Person.date_created, Person.notes FROM PersonEvent INNER JOIN Person ON PersonEvent.person_id = Person.id ";
            query += "INNER JOIN Event ON PersonEvent.event_id = Event.id ";

            switch (eventOption)
            {
                case queryTimeOption.Past:
                    query += @" WHERE Event.date_time < GETUTCDATE()";
                    break;
                case queryTimeOption.Present:
                    query += @" WHERE CONVERT(CHAR(10), Event.date_time, 126) = CONVERT(CHAR(10),  GETUTCDATE(), 126)";
                    break;
                case queryTimeOption.Future:
                    query += @" WHERE Event.date_time > GETUTCDATE()";
                    break;
                case queryTimeOption.All:
                    // No condition to apply
                    break;
                default:
                    break;
            }

            if (eventOption == queryTimeOption.All)
            {
                query += " WHERE PersonEvent.event_id = " + eventId;
            }
            else
            {
                query += " AND PersonEvent.event_id = " + eventId;
            }

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            ContactList contacts = new ContactList();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Contact contactRecord = new Contact();

                    contactRecord.id = (int)dr["contact_id"];
                    contactRecord.name = dr["name"].ToString();
                    contactRecord.jobTitle = dr["job_title"].ToString();
                    contactRecord.organisation = dr["organisation"].ToString();
                    contactRecord.dateCreated = (DateTime)dr["date_created"];
                    contactRecord.notes = dr["notes"].ToString();

                    contacts.contactList.Add(contactRecord);
                }
            }

            // Close connection to database
            conn.Close();

            return contacts;
        }

        [WebMethod] 
        public Contact getContactById(string contactId)
        {
            // Query for contact Id
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT * FROM Person WHERE active = 1 and id =" + contactId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Contact contactRecord = new Contact();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    contactRecord.id = (int)dr["id"];
                    contactRecord.name = dr["name"].ToString();
                    contactRecord.jobTitle = dr["job_title"].ToString();
                    contactRecord.organisation = dr["organisation"].ToString();
                    contactRecord.dateCreated = (DateTime)dr["date_created"];
                    contactRecord.notes = dr["notes"].ToString();
                }
            }

            // Close connection to database
            conn.Close();

            return contactRecord;
        }

        //[WebMethod] // Not in use
        //public ContactList searchContactByName(String name)
        //{
        //    // List all contacts with name like %name%
        //    Contact myContact = new Contact();
        //    myContact.id = new Guid("C0380753-9DEF-4BB8-8C81-B3E3690339C7");
        //    myContact.name = "Delcio Gomes";
        //    myContact.organisation = "British Red Cross";
        //    myContact.jobTitle = "Development DBA";
        //    myContact.dateOfBirth = DateTime.UtcNow;
        //    myContact.notes = "Likes computer games";

        //    Contact mySon = new Contact();
        //    mySon.id = new Guid("5ED87881-56F1-41CF-A943-D085E22840B1");
        //    mySon.name = "Daniel Gomes";
        //    mySon.organisation = "Gomes Family";
        //    mySon.jobTitle = "Fun R US";
        //    mySon.dateOfBirth = DateTime.UtcNow;
        //    mySon.notes = "Likes computer games and chocolate";

        //    Contact myDaughter = new Contact();
        //    myDaughter.id = new Guid("E0F47E65-E7F6-4D5F-8777-F84770B51502");
        //    myDaughter.name = "Gabriela Gomes";
        //    myDaughter.organisation = "Small Explorers";
        //    myDaughter.jobTitle = "Toy Ranger";
        //    myDaughter.dateOfBirth = DateTime.UtcNow;
        //    myDaughter.notes = "Eats everything";

        //    ContactList contacts = new ContactList();
        //    contacts.contactList.Add(myContact);
        //    contacts.contactList.Add(mySon);
        //    contacts.contactList.Add(myDaughter);

        //    return contacts;
        //}

        [WebMethod] 
        public bool toggleContactMet(string userId, string personId)
        {
            bool contactMet = false;

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT * FROM PersonPerson WHERE person_id = " + userId + " and person_met_id = " + personId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Contact contactRecord = new Contact();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows) // If contains row, delete it
            {
                // Get ID of record to delete
                while (dr.Read()) 
                {
                    contactRecord.id = (int)dr["id"];
                }

                // Delete record
                query = @"DELETE FROM PersonPerson WHERE person_id = " + userId + " and person_met_id = " + personId;

                // Return false
                contactMet = false;
            }
            else
            {
                // Create record
                query = @"INSERT INTO PersonPerson (person_id, person_met_id) values (" + userId + ", " + personId + ")";
                contactMet = true;
            }

            // Execute INSERT or DELETE based on conditions above
            dr.Close();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            // Close connection to database
            conn.Close();

            return contactMet;
        }

        [WebMethod] 
        public bool isContactMet(string userId, string personId)
        {
            bool contactMet = false;

            // Database connection objects
            String query = String.Empty;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = conn.CreateCommand();

            // Build query
            query = @"SELECT 1 FROM PersonPerson WHERE person_id = " + userId + " and person_met_id = " + personId;

            // Assign query to command
            cmd.CommandText = query;
            conn.Open();

            Event eventRecord = new Event();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows) // If contains row, delete it
            {
                // Return true
                contactMet = true;
            }
            else
            {
                // Return false
                contactMet = false;
            }

            dr.Close();
            dr.Dispose();

            // Close connection to database
            conn.Close();

            return contactMet;
        }
        #endregion
    }
}
