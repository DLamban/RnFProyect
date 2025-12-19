namespace Core.Networking.config
{
    public class NakamaOptions
    {
        public string host { get; set; }
        public int port { get; set; }
        public string serverKey { get; set; }
        public bool useSSL { get; set; }
    }
    public static class AppSettings
    {
        public static NakamaOptions getNetworkConfig()
        {
            NakamaOptions options = new NakamaOptions()
            {
                host = "192.168.50.2",
                port = 7350,
                serverKey = "defaultkey",
                useSSL = false
            };
            return options;
        }
    }
}
