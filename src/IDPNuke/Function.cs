using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace IDPNuke;

public class Function
{
    private static readonly string[] states = {
        "Закарпатська область", "Івано-Франківська область", "Тернопільська область", "Львівська область",
        "Волинська область", "Рівненська область", "Житомирська область", "Київська область", "Чернігівська область",
        "Сумська область", "Харківська область", "Луганська область", "Донецька область", "Запорізька область",
        "Херсонська область", "АР Крим", "Одеська область", "Миколаївська область", "Дніпропетровська область",
        "Полтавська область", "Черкаська область", "Кіровоградська область", "Вінницька область", "Хмельницька область",
        "Чернівецька область"
    };
    private static readonly HttpClient client = new();
    private static DateTime m_lastUpdate = DateTime.MinValue;
    private static Dictionary<string, StatesResponse.State> m_states = new();
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public Task<IEnumerable<int>> FunctionHandler(ILambdaContext context)
    {
        return Get();
    }
    
    public async Task<IEnumerable<int>> Get()
    {
        await GetAlarmData();
        IEnumerable<int> alarmData = states.Select(s => (m_states.TryGetValue(s, out StatesResponse.State value) && value.Enabled) ? 1 : 0);
        return alarmData.ToArray();
    }

    private static async Task GetAlarmData()
    {
        if (DateTime.Now - m_lastUpdate < TimeSpan.FromSeconds(15)) return;

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        client.DefaultRequestHeaders.Add("Host", "map.infobyte.ua");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0");

        HttpResponseMessage httpResponse = await client.GetAsync("http://map.infobyte.ua/map/statuses.json");
        httpResponse.EnsureSuccessStatusCode();
        GZipStream gZipStream = new(await httpResponse.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
        StatesResponse? response = await JsonSerializer.DeserializeAsync<StatesResponse>(gZipStream);
        if (response == null) return;
        m_states = response.States;
        m_lastUpdate = DateTime.Now;
    }


      [DataContract]
      private sealed class StatesResponse
      {
         [DataMember] public Dictionary<string, State> States { get; set; } = new();

         [DataContract]
         public struct State
         {
            public State(bool enabled, DateTime enabledAt)
            {
                Enabled = enabled;
                EnabledAt = enabledAt;
            }

            [DataMember(Name = "enabled")]
            public bool Enabled { get; set; }

            [DataMember(Name = "enabled_at")]
            public DateTime EnabledAt { get; set; }

            public override string ToString()
            {
                return $"{Enabled} {EnabledAt}";
            }
         }
      }
}
