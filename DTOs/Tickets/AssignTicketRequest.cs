using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.DTOs.Tickets
{
    public class AssignTicketRequest
    {
        [Required]
        public int AgentId { get; set; }
    }
}
