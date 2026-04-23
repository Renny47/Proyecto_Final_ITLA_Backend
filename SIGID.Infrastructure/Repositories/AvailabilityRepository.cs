using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly AppDbContext _context;

    public AvailabilityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Availability?> GetByIdAsync(Guid id)
    {
        return await _context.Availabilities
            .Include(a => a.TimeSlots)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Availability>> GetByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.Availabilities
            .Include(a => a.TimeSlots)
            .Where(a => a.Date.Date == dateOnly)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Availability>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var startDateOnly = startDate.Date;
        var endDateOnly = endDate.Date;
        
        return await _context.Availabilities
            .Include(a => a.TimeSlots)
            .Where(a => a.Date.Date >= startDateOnly && a.Date.Date <= endDateOnly)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Availability>> GetAllAsync()
    {
        return await _context.Availabilities
            .Include(a => a.TimeSlots)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task<Availability> CreateAsync(Availability availability)
    {
        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();
        return availability;
    }

    public async Task<Availability> UpdateAsync(Availability availability)
    {
        availability.UpdatedAt = DateTime.UtcNow;
        _context.Availabilities.Update(availability);
        await _context.SaveChangesAsync();
        return availability;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var availability = await _context.Availabilities
            .Include(a => a.TimeSlots)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (availability == null)
            return false;

        // Check if any time slot is booked
        if (availability.TimeSlots.Any(ts => ts.IsBooked))
            throw new InvalidOperationException("No se puede eliminar disponibilidad con reservas existentes");

        _context.Availabilities.Remove(availability);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.Availabilities
            .AnyAsync(a => a.Date.Date == dateOnly);
    }
}