using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace PalmHilsSemanticKernelBot.BusinessLogic.Bot.ConversationMemory
{
    public class SafeChatMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.FullName?.StartsWith("OpenAI") == true ||
                   objectType.FullName?.StartsWith("Microsoft.SemanticKernel") == true ||
                   objectType.FullName?.Contains("ChatMessage") == true ||
                   objectType.FullName?.Contains("FunctionCall") == true ||
                   objectType.FullName?.Contains("ChatCompletion") == true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var token = JToken.Load(reader);
                RemoveProblematicProperties(token);

                var safeSerializer = new JsonSerializer();
                foreach (var converter in serializer.Converters)
                {
                    if (!(converter is SafeChatMessageConverter))
                    {
                        safeSerializer.Converters.Add(converter);
                    }
                }

                return token.ToObject(objectType, safeSerializer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeChatMessageConverter ReadJson error: {ex.Message}");
                return GetDefaultValue(objectType);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var safeSerializer = new JsonSerializer();
                foreach (var converter in serializer.Converters)
                {
                    if (!(converter is SafeChatMessageConverter))
                    {
                        safeSerializer.Converters.Add(converter);
                    }
                }

                var token = JToken.FromObject(value, safeSerializer);
                RemoveProblematicProperties(token);
                token.WriteTo(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeChatMessageConverter WriteJson error: {ex.Message}");
                writer.WriteValue(value?.ToString() ?? "null");
            }
        }

        private void RemoveProblematicProperties(JToken token)
        {
            if (token is JObject obj)
            {
                var problematicProperties = new[]
                {
                    "ContentTokenLogProbabilities",
                    "FinishReason",
                    "LogProbabilities",
                    "TokenLogProbabilities",
                    "TopLogProbabilities",
                    "Usage",
                    "SystemFingerprint",
                    "Created",
                    "Model",
                    "Id"
                };

                foreach (var prop in problematicProperties)
                {
                    obj.Remove(prop);
                }

                foreach (var property in obj.Properties().ToList())
                {
                    RemoveProblematicProperties(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array)
                {
                    RemoveProblematicProperties(item);
                }
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}