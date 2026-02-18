using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces
{
    public interface IBookingService
    {
        //crud admin
        public Task<BookingDto> CreateBookingAsync(BookingDto booking);
        public Task<BookingDto> UpdateBookingByIdAsync(BookingDto booking);
        public Task<bool> DeleteBookingByIdAsync(Guid bookingId);

        //general queries (Receptionist and Admin)
        public Task<List<BookingDto>>? GetAllBookingsAsync();
        public Task<BookingDto?> GetBookingByIdAsync(Guid bookingId);

        //crud client
        public Task<List<BookingDto>>? GetAvailableBookingsAsync();
    }
}
