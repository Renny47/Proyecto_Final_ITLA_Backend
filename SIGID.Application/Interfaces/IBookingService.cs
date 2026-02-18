using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces
{
    public interface IBookingService
    {
        //crud admin
        public Task<BookingDto> CreateBookingAsync(BookingDto booking);
        public Task<BookingDto> UpdateBookingByIdAsync(BookingDto booking, Guid bookingId);
        public Task<bool> DeleteBookingByIdAsync(Guid bookingId);

        //general queries (Receptionist and Admin)
        public Task<IEnumerable<BookingDto>>? GetAllBookingsAsync();
        public Task<BookingDto?> GetBookingByIdAsync(Guid bookingId);

        //crud client
        public Task<IEnumerable<BookingDto>>? GetAvailableBookingsAsync();
        public Task<bool> BookAsync(Guid bookingId, string clientName);
    }
}
