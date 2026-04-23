using SIGID.Domain.Entities;

namespace SIGID.Domain.Interfaces;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeSlot>> GetByAvailabilityIdAsync(Guid availabilityId);
    Task<TimeSlot> CreateAsync(TimeSlot timeSlot);
    Task<TimeSlot> UpdateAsync(TimeSlot timeSlot);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteByAvailabilityIdAsync(Guid availabilityId);
    Task<bool> IsBookedAsync(Guid id);
    Task<bool> HasOverlapAsync(Guid availabilityId, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null);
}