using Vonage;
using Vonage.Request;
using Vonage.Messaging;
using System.Threading.Tasks;

namespace AlertApi.Services
{
    public class VonageService
    {
        private readonly VonageClient _vonageClient;

        public VonageService()
        {
            var credentials = Credentials.FromApiKeyAndSecret(
                "c9c88262", // ma clé API Vonage
                "yCvv3DqZcCxKPkw8" // mon code secret Vonage
            );

            _vonageClient = new VonageClient(credentials);
        }

       public async Task SendSmsAsync(string toPhoneNumber, string message)
{
    var response = await _vonageClient.SmsClient.SendAnSmsAsync(new SendSmsRequest
    {
        To = toPhoneNumber,
        From = "KwattAlert",
        Text = message
    });

            // LOG ici :
            // var msgId = response.Messages[0].MessageId;
            // Console.WriteLine($"Message ID : {msgId ?? "aucun"}");

            // LOG ici je viens d ajouter :
            var status = response.Messages[0].Status;
            var errorText = response.Messages[0].ErrorText;
            var msgId = response.Messages[0].MessageId;

            Console.WriteLine(status == "0"
                ? $" SMS accepté pour {toPhoneNumber} | Message ID : {msgId}"
                : $" Échec d’envoi pour {toPhoneNumber} → Status={status} | Erreur={errorText ?? "Aucune"}");
        }

    }
}
