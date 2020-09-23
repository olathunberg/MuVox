using System.IO;
using System.Text.Json;

namespace MuVox.Metering
{
    public class ChannelConfiguration
    {
        public static ChannelConfiguration Read()
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<ChannelConfiguration>(File.ReadAllText("Config/tracks.json"), options);
        }

        public Channel[] Channels { get; set; }

        public class Channel
        {
            public string Label { get; set; }
        }
    }
}
