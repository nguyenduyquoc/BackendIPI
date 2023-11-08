using System.ComponentModel.DataAnnotations;

namespace Backend_API.ViewModels
{
    public class ReturnRequestConfirm
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Response { get; set; }
    }
}
