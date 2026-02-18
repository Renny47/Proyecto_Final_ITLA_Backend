using Microsoft.EntityFrameworkCore;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepositoryAsync
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await _context.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteBookingByIdAsync(Guid bookingId)
        {
            var entity = GetBookingByIdAsync(bookingId);

            if (entity == null) return false;

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Booking>>? GetAllBookingsAsync()
        {
            return await _context.Bookings
                .OrderBy(b => b.DateAndTime)
                .ToListAsync();
        }

        public async Task<List<Booking>>? GetAvailableBookingsAsync()
        {
            return await _context.Bookings
                .Where(b => b.BookingState == BookingState.NOT_BOOKED)
                .OrderBy(b => b.DateAndTime)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            return await _context.Bookings.FindAsync(bookingId);
        }

        public async Task<Booking> UpdateBookingByIdAsync(Booking booking)
        {
            var entity = await GetBookingByIdAsync(booking.Id) 
                ?? throw new Exception($"booking with id: '{booking.Id}'not found");

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
    }
}
