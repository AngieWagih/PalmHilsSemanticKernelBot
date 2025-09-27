using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PalmHilsSemanticKernelBot.BusinessLogic.Bot.ConversationMemory
{
    public class ResilientMemoryStorage : IStorage
    {
        private readonly MemoryStorage _memoryStorage;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly Dictionary<string, object> _backupData = new();

        public ResilientMemoryStorage()
        {
            Console.WriteLine("=== ResilientMemoryStorage constructor called ===");
            _memoryStorage = new MemoryStorage();

            _jsonSettings = new JsonSerializerSettings
            {
                Converters = { new SafeChatMessageConverter() },
                Error = HandleDeserializationError,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 32
            };
            Console.WriteLine("=== ResilientMemoryStorage initialized with SafeChatMessageConverter ===");
        }

        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Console.WriteLine($"JSON Deserialization Error: {e.ErrorContext.Error.Message}");
            Console.WriteLine($"Path: {e.ErrorContext.Path}");
            e.ErrorContext.Handled = true;
        }

        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Reading keys: {string.Join(", ", keys)} ===");
                var result = await _memoryStorage.ReadAsync(keys, cancellationToken);

                // Backup successful reads
                foreach (var kvp in result)
                {
                    _backupData[kvp.Key] = kvp.Value;
                }

                Console.WriteLine($"=== ResilientMemoryStorage: Successfully read {result.Count} items ===");
                return result;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"=== ResilientMemoryStorage: JsonSerializationException caught: {ex.Message} ===");

                // Try to recover from backup
                var recoveredData = new Dictionary<string, object>();
                foreach (var key in keys)
                {
                    if (_backupData.ContainsKey(key))
                    {
                        Console.WriteLine($"=== Recovering data for key: {key} ===");
                        recoveredData[key] = _backupData[key];
                    }
                }

                return recoveredData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Other exception caught: {ex.Message} ===");
                return new Dictionary<string, object>();
            }
        }

        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Writing {changes.Count} items ===");

                // Create backup before writing
                foreach (var kvp in changes)
                {
                    _backupData[kvp.Key] = kvp.Value;
                    Console.WriteLine($"Writing Key: {kvp.Key}, Value type: {kvp.Value?.GetType().Name}");
                }

                // Try to serialize first to catch errors early
                foreach (var kvp in changes)
                {
                    try
                    {
                        var testSerialization = JsonConvert.SerializeObject(kvp.Value, _jsonSettings);
                        Console.WriteLine($"Serialization test passed for {kvp.Key}");
                    }
                    catch (Exception serEx)
                    {
                        Console.WriteLine($"Serialization test failed for {kvp.Key}: {serEx.Message}");
                        // Continue anyway, the converter should handle it
                    }
                }

                await _memoryStorage.WriteAsync(changes, cancellationToken);
                Console.WriteLine("=== ResilientMemoryStorage: Write completed successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Write error: {ex.Message} ===");
                Console.WriteLine($"=== Stack trace: {ex.StackTrace} ===");

                // Don't throw - just log the error and continue
                // The backup data is still available for recovery
            }
        }

        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Deleting keys: {string.Join(", ", keys)} ===");
                await _memoryStorage.DeleteAsync(keys, cancellationToken);

                // Remove from backup as well
                foreach (var key in keys)
                {
                    _backupData.Remove(key);
                }

                Console.WriteLine("=== ResilientMemoryStorage: Delete completed successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ResilientMemoryStorage: Delete error: {ex.Message} ===");
                // Don't throw for delete errors either
            }
        }
    }
}