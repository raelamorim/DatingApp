using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class ValueForPutDto
    {
        [Required]
        public string Name { get; set; }
    }
}