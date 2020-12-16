using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ElgatoKeyLightToggler
{
    class Program
    {
        static async Task Main(string[] args)
        {   
            foreach (string url in GetUrls())
            {
                var apiUrl = $"{url}/elgato/lights";
                using var client = new HttpClient();
                var result = await client.GetStringAsync(apiUrl);
                dynamic currentStatus = JsonConvert.DeserializeObject<dynamic>(result);

                JObject newStatus = JObject.FromObject(new
                {
                    numberOfLights = "1",
                    lights = new dynamic[] {
                        new
                        {
                            on = currentStatus.lights[0].on == 0 ? 1 : 0
                        }
                    }
                });

                var content = new StringContent(newStatus.ToString(), Encoding.UTF8, "application/json");
                await client.PutAsync(apiUrl, content);
            }
        }

        static string[] GetUrls() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
                
            var configuration = builder.Build();
            var section = configuration.GetSection("ElgatoKeyLightURLs");
            var urls = section.GetChildren().Select(u => u.Value).ToArray();
            return urls;
        }
    }
}
