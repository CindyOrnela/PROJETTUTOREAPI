namespace KwattAlertAPI.Services
{
    public class ESP32Service
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;

        public ESP32Service(IConfiguration config)
        {
            _url = config["ESP32:AlertUrl"];
            _httpClient = new HttpClient();
        }

        public async Task SendAlertAsync()
        {
            await _httpClient.GetAsync(_url);
        }
    }
}
