using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IAvailabilityService
{
    Task<AvailabilityDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AvailabilityDto>> GetByDateAsync(DateTime date);
    Task<IEnumerable<AvailabilityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<AvailabilityDto> CreateAsync(CreateAvailabilityDto createAvailabilityDto);
    Task<AvailabilityDto> UpdateAsync(Guid id, UpdateAvailabilityDto updateAvailabilityDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> BookTimeSlotAsync(Guid timeSlotId, Guid reservaId);
    Task<bool> UnbookTimeSlotAsync(Guid timeSlotId);
    Task<bool> ValidateTimeSlotAvailableAsync(Guid timeSlotId);
}