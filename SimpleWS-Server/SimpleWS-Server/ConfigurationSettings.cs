namespace SimpleWS_Server
{
    public static class ConfigurationSettings
    {

        public static IConfiguration AppSetting
        {
            get;
        }
        static ConfigurationSettings()
        {
            AppSetting = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.Development.json").Build();
        }
    }
}
