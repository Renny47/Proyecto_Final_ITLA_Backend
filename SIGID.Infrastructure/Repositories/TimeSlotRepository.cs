using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly AppDbContext _context;

    public TimeSlotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TimeSlot?> GetByIdAsync(Guid id)
    {
        return await _context.TimeSlots
            .Include(ts => ts.Availability)
            .Include(ts => ts.Reserva)
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }

    public async Task<IEnumerable<TimeSlot>> GetByAvailabilityIdAsync(Guid availabilityId)
    {
        return await _context.TimeSlots
            .Where(ts => ts.AvailabilityId == availabilityId)
            .OrderBy(ts => ts.StartTime)
            .ToListAsync();
    }

    public async Task<TimeSlot> CreateAsync(TimeSlot timeSlot)
    {
        _context.TimeSlots.Add(timeSlot);
        await _context.SaveChangesAsync();
        return timeSlot;
    }

    public async Task<TimeSlot> UpdateAsync(TimeSlot timeSlot)
    {
        timeSlot.UpdatedAt = DateTime.UtcNow;
        _context.TimeSlots.Update(timeSlot);
        await _context.SaveChangesAsync();
        return timeSlot;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var timeSlot = await _context.TimeSlots.FindAsync(id);
        if (timeSlot == null)
            return false;

        if (timeSlot.IsBooked)
            throw new InvalidOperationException("No se puede eliminar un horario que tiene una reserva");

        _context.TimeSlots.Remove(timeSlot);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByAvailabilityIdAsync(Guid availabilityId)
    {
        var timeSlots = await _context.TimeSlots
            .Where(ts => ts.AvailabilityId == availabilityId)
            .ToListAsync();

        if (timeSlots.Any(ts => ts.IsBooked))
            throw new InvalidOperationException("No se pueden eliminar horarios que tienen reservas");

        _context.TimeSlots.RemoveRange(timeSlots);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsBookedAsync(Guid id)
    {
        var timeSlot = await _context.TimeSlots.FindAsync(id);
        return timeSlot?.IsBooked ?? false;
    }

    public async Task<bool> HasOverlapAsync(Guid availabilityId, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null)
    {
        var query = _context.TimeSlots
            .Where(ts => ts.AvailabilityId == availabilityId);

        if (excludeId.HasValue)
            query = query.Where(ts => ts.Id != excludeId.Value);

        return await query.AnyAsync(ts => 
            (startTime < ts.EndTime && endTime > ts.StartTime));
    }
}