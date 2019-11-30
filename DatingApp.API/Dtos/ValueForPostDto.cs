using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class ValueForPostDto
    {
        [Required]
        public string Name { get; set; }
    }
}