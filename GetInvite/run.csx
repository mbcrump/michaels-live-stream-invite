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
  
    var icalevent = new Event()
        {
            DtStart = new CalDateTime(new DateTime(2020, 1, 28, 17, 00, 0, DateTimeKind.Utc)),
            DtEnd = new CalDateTime(new DateTime(2020, 1, 28, 19, 00, 0, DateTimeKind.Utc)),
            Created = new CalDateTime(DateTime.Now),
            Location = "https://twitch.tv/mbcrump",
            Summary = "Mbcrump's Live Stream",
            Url = new Uri("https://twitch.tv/mbcrump")
        };

    string description = "Join Michael's Live Stream as we cover some cool developer tips and tricks, do some live-coding and take your questions!";

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
