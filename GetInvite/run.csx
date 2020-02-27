using System.Net;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
	//Testing continuous deployment 070617
    var calendar = new Calendar();
    calendar.AddProperty("X-WR-CALNAME", "Mbcrump's Live Stream"); // sets the calendar title
    calendar.AddProperty("X-ORIGINAL-URL", "https://twitch.tv/mbcrump");
    calendar.AddProperty("METHOD", "PUBLISH");
    int starthour, stophour, startminute, stopminute = 00;
    DateTime nextStreamDay = GetNextWeekday(DateTime.Today, DayOfWeek.Tuesday);
	
    // parse query parameter
    string dayofstream = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "day", true) == 0)
        .Value;
	
// Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    dayofstream = dayofstream ?? data?.dayofstream;
	
    if (dayofstream == "friday"){
	    nextStreamDay = GetNextWeekday(DateTime.Today, DayOfWeek.Friday);
	    starthour = 17;
	    stophour = 18;
	    stopminute = 30;
    }
	else{
	 nextStreamDay = GetNextWeekday(DateTime.Today, DayOfWeek.Tuesday);
	   starthour = 01;
	    stophour = 02;
	    stopminute = 30;
	
    }
    string description = "Join Michael's Live Stream (https://twitch.tv/mbcrump) as we cover some cool developer tips and tricks, do some live-coding and take your questions!";

    var icalevent = new Event()
        {
            DtStart = new CalDateTime(new DateTime(nextStreamDay.Year.ToUniversalTime(), nextStreamDay.Month.ToUniversalTime(), nextStreamDay.Day.ToUniversalTime(), starthour, 0, 0, DateTimeKind.Utc)),
            DtEnd = new CalDateTime(new DateTime(nextStreamDay.YearToUniversalTime(), nextStreamDay.Month.ToUniversalTime(), nextStreamDay.Day.ToUniversalTime(), stophour, stopminute, 0, DateTimeKind.Utc)),
            Created = new CalDateTime(DateTime.Now),
            Location = "https://twitch.tv/mbcrump",
            Summary = "Mbcrump's Live Stream",
            Url = new Uri("https://twitch.tv/mbcrump"),
	    Description = description
        };


    icalevent.AddProperty("X-ALT-DESC;FMTTYPE=text/html", description); // creates an HTML description
    calendar.Events.Add(icalevent);

    var serializer = new CalendarSerializer(new SerializationContext());

    var serializedCalendar = serializer.SerializeToString(calendar);
    var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);
    var result = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new ByteArrayContent(bytesCalendar)
    };

    result.Content.Headers.ContentDisposition =
        new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
    {
        FileName = "streaminvite.ics"
    };

    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

    return result;
}

   public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
   {
      // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
      int daysToAdd = ((int) day - (int) start.DayOfWeek + 7) % 7;
      return start.AddDays(daysToAdd);
   }
