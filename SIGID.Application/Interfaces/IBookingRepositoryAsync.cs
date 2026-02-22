using SIGID.Domain.Entities;

namespace SIGID.Application.Interfaces
{
    public interface IBookingRepositoryAsync
    {
        //crud admin
        public Task<Booking> CreateBookingAsync(Booking booking);
        public Task<Booking> UpdateBookingByIdAsync(Booking booking, Guid bookingId);
        public Task<bool> DeleteBookingByIdAsync(Guid bookingId);

        //general queries (Receptionist and Admin)
        public Task<List<Booking>>? GetAllBookingsAsync();
        public Task<Booking?> GetBookingByIdAsync(Guid bookingId);

        //crud client
        public Task<List<Booking>>? GetAvailableBookingsAsync();
        public Task<bool> BookAsync(Guid bookingId, string clientName);
    }
}
