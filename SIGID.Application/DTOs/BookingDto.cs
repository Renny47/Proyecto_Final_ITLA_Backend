using SIGID.Domain.Enums;

namespace SIGID.Application.DTOs
{
    public class BookingDto
    {
        public required Guid Id { get; set; }
        public DateTime DateAndTime { get; set; }
        public BookingState BookingState { get; set; }
        public string? BookedByClientName { get; set; }
    }
}
