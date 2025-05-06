using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace VEXA.Helper
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var jsonData = session.GetString(key);
            return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}