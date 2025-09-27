namespace PalmHilsSemanticKernelBot.Helpers
{
    public class DataBaseSchemaReaderService : IDataBaseSchemaReaderService
    {
       
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment WebHostEnvironment;
        public DataBaseSchemaReaderService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }


        public string GetDatabaseSchema()
        {
            try
            {
                var basePath = WebHostEnvironment.WebRootPath;
                var schemaPath = Configuration.GetValue<string>("DataBaseSchema");
                return System.IO.File.ReadAllText(basePath + schemaPath);

            }
            catch (Exception)
            {

                throw;
            }

        }



    }
}
