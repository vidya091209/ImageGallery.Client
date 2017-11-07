namespace ImageGallery.Client.Configuration
{
    public class ConfigurationOptions
    {
        public string ApiUri { get; set; } = "https://localhost:44370/";

        public string ImagesUri { get; set; } = "https://localhost:44370/Images/";
    
        public Dataprotection Dataprotection { get; set; }

        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; }

    }

    public class Dataprotection
    {
        public string RedisConnection { get; set; }

        public string RedisKey { get; set; }

        public bool Enabled { get; set; }

    }

    public class OpenIdConnectConfiguration
    {
        public string Authority { get; set; } //= "https://localhost:44379/";

        public string ClientSecret { get; set; } //= "secret";

    }

}
