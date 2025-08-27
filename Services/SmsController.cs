using AlertApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AlertApi.Controllers
{
    [ApiController]
    [Route("api/sms")]
    public class SmsController : ControllerBase
    {
        private readonly VonageService _vonageService;

        public SmsController(VonageService vonageService)
        {
            _vonageService = vonageService;
        }
        //code de test
        [HttpPost("send-all-firebase")]
        public async Task<IActionResult> SendSmsToAllFromFirebase()
        {
            var firebaseService = new FirebaseService();
            var users = await firebaseService.GetUsersAsync();

            var tasks = users
                .Where(u => !string.IsNullOrWhiteSpace(u.PhoneNumber))
                .Select(async user =>
                {
                    string message = $" Bonjour {user.Name}, une alerte a été déclenchée dans votre quartier.";
                    await _vonageService.SendSmsAsync(user.PhoneNumber, message);
                    Console.WriteLine($"✔ Message envoyé à {user.Name} ({user.PhoneNumber})");
                });
            Console.WriteLine("ok");
            await Task.WhenAll(tasks);
            return Ok(" Tous les SMS ont été envoyés aux utilisateurs Firebase.");

          
        }
    }
}
