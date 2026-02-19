using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
            return Ok(await _bookingService.GetAllBookingsAsync()!);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            return Ok(await _bookingService.GetBookingByIdAsync(bookingId));
        }
    }
}
