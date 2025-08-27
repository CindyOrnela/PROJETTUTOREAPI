using System.ComponentModel.DataAnnotations;

namespace AlertApi.Models
{
    public class MqttSubscribeDto
    {
        

    [Required]
        public string Topic { get; set; }
    }

    public class PublishDto
    {
        [Required]
        public string Topic { get; set; }

     
    }
}

