using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepositoryAsync _repo;

        public BookingService(IBookingRepositoryAsync repo)
        {
            _repo = repo;
        }

        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                DateAndTime = booking.DateAndTime,
                BookingState = booking.BookingState,
                BookedByClientName = booking.BookedByClientName
            };
        }

        public Task<bool> BookAsync(Guid bookingId, string clientName)
        {
            return _repo.BookAsync(bookingId, clientName);
        }

        public async Task<BookingDto> CreateBookingAsync(BookingDto bookingDto)
        {
            var entity = new Booking
            {
                Id = Guid.NewGuid(),
                DateAndTime = bookingDto.DateAndTime,
                BookingState = BookingState.NOT_BOOKED,
                BookedByClientName = null
            };

            var result = await _repo.CreateBookingAsync(entity)
                ?? throw new Exception("No se pudo crear la cita.");

            return MapToDto(entity);
        }

        public async Task<bool> DeleteBookingByIdAsync(Guid bookingId)
        {
            return await _repo.DeleteBookingByIdAsync(bookingId);
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var entities = await _repo.GetAllBookingsAsync()!;

            var dtos = entities.Select(b => new BookingDto
            {
                Id = b.Id,
                DateAndTime = b.DateAndTime,
                BookingState = b.BookingState,
                BookedByClientName = b.BookedByClientName
            });

            return dtos;
        }

        public async Task<IEnumerable<BookingDto>> GetAvailableBookingsAsync()
        {
            var entities = await _repo.GetAvailableBookingsAsync()!;
            var dtos = entities.Select(b => MapToDto(b));

            return dtos;
        }

        public async Task<BookingDto?> GetBookingByIdAsync(Guid bookingId)
        {
            var entity = await _repo.GetBookingByIdAsync(bookingId) 
                ?? throw new Exception($"No se pudo encontrar la cita de id '{bookingId}'");

            return MapToDto(entity);
        }

        public async Task<BookingDto> UpdateBookingByIdAsync(BookingDto booking, Guid bookingId)
        {
            var entity = new Booking
            {
                Id = booking.Id,
                DateAndTime = booking.DateAndTime,
                BookingState = booking.BookingState,
                BookedByClientName = booking.BookedByClientName,
            };

            var result = await _repo.UpdateBookingByIdAsync(entity, bookingId)
                ?? throw new Exception("No se pudo editar la cita.");

            return MapToDto(entity);
        }
    }
}
