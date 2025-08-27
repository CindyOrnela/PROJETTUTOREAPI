using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;



namespace AlertApi.Services
{   // Créer un service pour se connecter au broker
    public class MqttService:ControllerBase
    {
        private  FirebaseService _firebaseService = new FirebaseService();
        private  VonageService _vonageService =new VonageService();
        private  IMqttClient _mqttClient;
        // éviter de te reconnecter à chaque requête POST. Voici une version améliorée
        public async Task ConnectAsync()
        {
            if (_mqttClient != null && _mqttClient.IsConnected) // éviter de te reconnecter à chaque requête POST
                return;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
               .WithTcpServer("test.mosquitto.org", 1883)
               .WithClientId("api-client")
               .Build();

            try
            {
                await _mqttClient.ConnectAsync(options, CancellationToken.None);
                Console.WriteLine(" Connecté au broker Mosquitto !");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Erreur de connexion : " + ex.Message);
            }
        }
        // pour la publication
        public async Task PublishAsync(string topic, string message)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                Console.WriteLine(" reconnexion de MQTT client.");
                await this.ConnectAsync();
            }

          if(_mqttClient!.IsConnected) 
           { var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                Console.WriteLine($"Message publié sur le topic '{topic}'");
           }

        }
        // pour la souscription
        public async Task SendPersonalAlert()
        {


            var users = await _firebaseService.GetUsersAsync();

            //var user = users.FirstOrDefault(u => u.PhoneNumber == targetPhone);

            if (users.Count == 0)
                throw new Exception("aucun utilisateur trouve.");
            //return NotFound("aucun utilisateur trouve.");

            foreach (var user in users)
            {
                Console.WriteLine(user.PhoneNumber);
                string message = $"Bonjour {user.Name}, ceci est une alerte personnalisée KwattAlert.";
                await _vonageService.SendSmsAsync(user.PhoneNumber, message);
            }
     
        }



        public async Task SubscribeAsync(string topic)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var receivedTopic = e.ApplicationMessage.Topic;
                var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                Console.WriteLine($" Message reçu sur le topic '{receivedTopic}': {payload}");
               
                // on peut  ajouter ici une logique de traitement ou de stockage
                await this.SendPersonalAlert();
            };

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            Console.WriteLine($" Abonné au topic '{topic}'");
        }
    }

   
}

