using Microsoft.AspNetCore.Mvc;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public ClientController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableBookings()
        {
            return Ok(await _bookingService.GetAvailableBookingsAsync()!);
        }

        [HttpPost]
        public async Task<IActionResult> Book(Guid bookingId, string clientName)
        {
            return Ok(await _bookingService.BookAsync(bookingId, clientName));
        }
    }
}
