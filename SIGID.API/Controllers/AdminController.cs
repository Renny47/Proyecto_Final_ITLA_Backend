using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingDto bookingDto)
        {
            return Ok(await _bookingService.CreateBookingAsync(bookingDto));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBooking(BookingDto bookingDto)
        {
            return Ok(await _bookingService.UpdateBookingByIdAsync(bookingDto, bookingDto.Id));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            return Ok(await _bookingService.DeleteBookingByIdAsync(bookingId));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            return Ok(await _bookingService.GetAllBookingsAsync()!);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            return Ok(await _bookingService.GetBookingByIdAsync(bookingId));
        }
    }
}
