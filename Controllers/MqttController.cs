using AlertApi.Models;
using AlertApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlertApi.Controllers
{
    [ApiController]
    [Route("api/mqtt")]
    public class MqttController : ControllerBase
    {
        private readonly MqttService _mqttService;

        public MqttController(MqttService mqttService)
        {
            _mqttService = mqttService;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MqttRequestDto request)
        {
            await _mqttService.ConnectAsync();
            await _mqttService.PublishAsync(request.Topic, request.Message);
            return Ok(" Message MQTT envoyé.");
        }
        //  pour declencher la sousciption
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTopic([FromBody] MqttSubscribeDto request)
        {
            await _mqttService.ConnectAsync();
            await _mqttService.SubscribeAsync(request.Topic);
          
            return Ok("Abonné au topic ");
        }
    }
    }

