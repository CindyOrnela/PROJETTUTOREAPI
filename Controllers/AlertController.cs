using KwattAlertAPI.Services;
using Microsoft.AspNetCore.Mvc;
using AlertApi.Services;
using System.Linq;
using System.Threading.Tasks;
using KwattAlertAPI.Models;
using AlertApi.Models;
using Vonage.Users;
using Microsoft.AspNetCore.Authorization;

namespace KwattAlertAPI.Controllers
{
    
    [ApiController]
    [Route("api/alert")]
    public class AlertController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly VonageService _vonageService;
        private readonly ESP32Service _esp32Service;

        public AlertController(
            FirebaseService firebaseService,
            VonageService vonageService,
            ESP32Service esp32Service)
        {
            _firebaseService = firebaseService;
            _vonageService = vonageService;
            _esp32Service = esp32Service;

        }

        [HttpPost]
        public async Task<IActionResult> SendAlert()
        {
            var users = await _firebaseService.GetUsersAsync();

            var tasks = users
                .Where(user => !string.IsNullOrWhiteSpace(user.PhoneNumber))
                .Select(user => _vonageService.SendSmsAsync(user.PhoneNumber!, $"Alerte envoyée à {user.Name}"));

            await Task.WhenAll(tasks);

            await _esp32Service.SendAlertAsync();

            return Ok("Alertes envoyées avec succès.");
        }
        

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("AlertController fonctionne !");
        }

        [HttpGet("test-users")]
        public async Task<IActionResult> TestGetUsers()
        {
            var users = await _firebaseService.GetUsersAsync();

            if (users == null || users.Count == 0)
                return NotFound("Aucun utilisateur trouvé.");

            return Ok(users);
        }

       
        // j'appelle SaveAlertAsync dans mon controller pour enregistrer les alertes dans firebase 

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAlert([FromBody] alert Alert)
        {
            var success = await _firebaseService.SaveAlertAsync(Alert);

            return success ? Ok("Alerte enregistrée avec succès.") : StatusCode(500, "Erreur lors de l'enregistrement.");
        }

        [Authorize]
        [HttpGet("test-one-personal")]
        public async Task<IActionResult> SendPersonalAlert()
        {
              

            var users = await _firebaseService.GetUsersAsync();
           
            //var user = users.FirstOrDefault(u => u.PhoneNumber == targetPhone);

            if (users.Count == 0)
                return NotFound("aucun utilisateur trouve.");

          foreach(var user in users) { 
                Console.WriteLine(user.PhoneNumber);
                string message = $"Bonjour {user.Name}, ceci est une alerte personnalisée KwattAlert.";
                await _vonageService.SendSmsAsync( user.PhoneNumber, message);
           }
            
            return Ok($" SMS envoyé à tous");
        }

        [Authorize]
        [HttpGet("test-one")]
        public IActionResult TestOne()
        {
            return Ok("Accès autorisé !");
        }


    }
    
}
