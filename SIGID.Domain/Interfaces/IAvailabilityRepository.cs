using SIGID.Domain.Entities;

namespace SIGID.Domain.Interfaces;

public interface IAvailabilityRepository
{
    Task<Availability?> GetByIdAsync(Guid id);
    Task<IEnumerable<Availability>> GetByDateAsync(DateTime date);
    Task<IEnumerable<Availability>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Availability> CreateAsync(Availability availability);
    Task<Availability> UpdateAsync(Availability availability);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(DateTime date);
    Task<IEnumerable<Availability>> GetAllAsync();
}