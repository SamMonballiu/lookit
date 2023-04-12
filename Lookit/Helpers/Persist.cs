using Lookit.Models;
using System.IO;
using System.Text.Json;

namespace Lookit.Helpers
{
    internal class Persist
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static void PersistSession(PersistableSession session, string path)
        {
            var jsonString = JsonSerializer.Serialize(session, _options);
            File.WriteAllText(path, jsonString);
        }

        public static PersistableSession ReadSession(string path)
        {
            string jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PersistableSession>(jsonString, _options);
        }
    }
}
