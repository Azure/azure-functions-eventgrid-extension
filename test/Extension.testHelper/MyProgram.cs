using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.EventGrid.Tests
{
    public class MyProgram
    {
        public static string functionOut = null;

        public void TestEventGridToJObject([EventGridTrigger] JObject value)
        {
            functionOut = (string)value["subject"];
        }
    }
}
