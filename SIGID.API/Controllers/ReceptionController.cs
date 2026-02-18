using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceptionController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public ReceptionController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            return Ok(_bookingService.GetAllBookingsAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            return Ok(_bookingService.GetBookingByIdAsync(bookingId));
        }
    }
}
