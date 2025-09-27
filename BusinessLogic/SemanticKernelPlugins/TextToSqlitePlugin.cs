using Dapper;
using Microsoft.SemanticKernel;
using PalmHilsSemanticKernelBot.Helpers;
using System.ComponentModel;
using System.Data;
using System.Text.Json;


namespace PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins
{
    public class TextToSqlitePlugin
    {
        /*public Kernel Kernel { get; set; }*/
        public IDataBaseSchemaReaderService DatabaseSchemaReaderService { get; set; }
        private readonly IConfiguration Configuration;
        private readonly IDbConnection Connection;

        public TextToSqlitePlugin(IDataBaseSchemaReaderService databaseSchemaReaderService, IConfiguration configuration, IDbConnection connection)
        {
            DatabaseSchemaReaderService = databaseSchemaReaderService;
            Configuration = configuration;
            Connection = connection;
        }

        [KernelFunction]
        [Description("Converts natural language requests into SQLite queries for database operations. This function is automatically invoked whenever data needs to be retrieved from the database.")]
        public async Task<string> GenerateSqLiteQuery(string userInput, Kernel kernel)
        {
            try
            {
                var prompt = $@"you are an expert SQLite query generator. Given the database schema and user input,
                                generate a valid SQLite query.

                                DATABASE SCHEMA:
                                {DatabaseSchemaReaderService.GetDatabaseSchema()}

                                USER INPUT: {userInput}

                                RULES:
                                1. Generate ONLY valid SQLite syntax with only the details that user asked for .
                                2. Use proper table and column names from the schema.
                                3. Include appropriate WHERE clauses, JOINs, ORDER BY, etc. If needed
                                4. Return only the SQL query, no explanations.
                                5. Ensure query is safe and doesn't include harmful operations.
                                6. IMPORTANT: When comparing dates with the 'Day' column (format: yyyy-mm-dd 00:00:00), 
                                                        use DATE(Day) = 'yyyy-mm-dd' to extract just the date part for comparison.

                                SQL Query:
                                 ";



                var result = await kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception)
            {

                throw;
            }

        }

        [KernelFunction, Description("Execute the generated query then  return results as JSON")]
        public async Task<string> ExecuteSelectQueryAsync(
       [Description("The generated SQLite SELECT query to execute")] string query)
        {
            try
            {
                
                var results = await Connection.QueryAsync(query);

                return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return $"Error executing query: {ex.Message}";
            }
        }

    }
}
