using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using KwattAlertAPI.Models;
using System.Text.Json;
using AlertApi.Models;

namespace AlertApi.Services
{
    public class FirebaseService
    {
        private  HttpClient _httpClient;
        private readonly string _firebaseUrl = "https://kwatt-752ce-default-rtdb.firebaseio.com/";
        private  GoogleCredential _googleCredential;

        public FirebaseService()
        {
            var firebaseKeyPath = Environment.GetEnvironmentVariable("FIREBASE_KEY_PATH"); //  AJOUT ICI

            _googleCredential = GoogleCredential
                .FromFile(firebaseKeyPath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.database", "https://www.googleapis.com/auth/userinfo.email");

             _httpClient = new HttpClient();
           
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, _firebaseUrl + "users.json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erreur HTTP lors de GetUsersAsync : {ex.Message}");
                return new List<User>(); // ou null selon mon besoin
            }

            if (!response.IsSuccessStatusCode)
                return new List<User>();

            var json = await response.Content.ReadAsStringAsync();

            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var users = new List<User>();

            foreach (var kvp in dict)
            {
                try
                {
                    var user = kvp.Value.Deserialize<User>();
                    if (user != null)
                        users.Add(user);
                }
                catch
                {
                    // Ignore les entrées invalides
                }
            }

            return users;
        }



        // Tu peux garder GetAlertsAsync() si nécessaire
        public async Task<string?> GetAlertsAsync()
        {
            var accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, _firebaseUrl + "alerts.json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erreur HTTP lors de GetAlertsAsync : {ex.Message}");
                return null;
            }

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
        //  l'ajout d'une methode SaveAlertAsync pour enregistrer les alertes dans firebase 
        public async Task<bool> SaveAlertAsync(alert Alert)
        {
            var accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var requestUri = _firebaseUrl + $"alert/{Alert.id_alert}.json"; // Utilise l'ID pour la clé

            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var json = JsonSerializer.Serialize(Alert);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
