using AutoMapper;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AvailabilityService> _logger;

    public AvailabilityService(
        IAvailabilityRepository availabilityRepository,
        ITimeSlotRepository timeSlotRepository,
        IReservaRepository reservaRepository,
        IMapper mapper,
        ILogger<AvailabilityService> logger)
    {
        _availabilityRepository = availabilityRepository;
        _timeSlotRepository = timeSlotRepository;
        _reservaRepository = reservaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AvailabilityDto?> GetByIdAsync(Guid id)
    {
        var availability = await _availabilityRepository.GetByIdAsync(id);
        return availability != null ? _mapper.Map<AvailabilityDto>(availability) : null;
    }

    public async Task<IEnumerable<AvailabilityDto>> GetByDateAsync(DateTime date)
    {
        var availabilities = await _availabilityRepository.GetByDateAsync(date);
        return _mapper.Map<IEnumerable<AvailabilityDto>>(availabilities);
    }

    public async Task<IEnumerable<AvailabilityDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var availabilities = await _availabilityRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<AvailabilityDto>>(availabilities);
    }

    public async Task<AvailabilityDto> CreateAsync(CreateAvailabilityDto createAvailabilityDto)
    {
        // Validar que la fecha no esté en el pasado (excepto hoy)
        if (createAvailabilityDto.Date.Date < DateTime.Today)
        {
            throw new ArgumentException("No se puede crear disponibilidad para fechas pasadas");
        }

        // Validar que no exista disponibilidad para esa fecha
        var existingAvailability = await _availabilityRepository.GetByDateAsync(createAvailabilityDto.Date);
        if (existingAvailability.Any())
        {
            throw new InvalidOperationException($"Ya existe disponibilidad para la fecha {createAvailabilityDto.Date:yyyy-MM-dd}");
        }

        // Validar horarios
        ValidateTimeSlots(createAvailabilityDto.TimeSlots);

        // Crear la entidad Availability
        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            Date = createAvailabilityDto.Date,
            CreatedBy = createAvailabilityDto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Crear los TimeSlots
        availability.TimeSlots = createAvailabilityDto.TimeSlots.Select(ts => new TimeSlot
        {
            Id = Guid.NewGuid(),
            AvailabilityId = availability.Id,
            StartTime = TimeSpan.Parse(ts.StartTime),
            EndTime = TimeSpan.Parse(ts.EndTime),
            IsBooked = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var createdAvailability = await _availabilityRepository.CreateAsync(availability);
        
        _logger.LogInformation("Availability created for date {Date} with {Count} time slots", 
            availability.Date, availability.TimeSlots.Count);

        return _mapper.Map<AvailabilityDto>(createdAvailability);
    }

    public async Task<AvailabilityDto> UpdateAsync(Guid id, UpdateAvailabilityDto updateAvailabilityDto)
    {
        var existingAvailability = await _availabilityRepository.GetByIdAsync(id);
        if (existingAvailability == null)
        {
            throw new ArgumentException("Disponibilidad no encontrada");
        }

        // Validar que no haya reservas activas si se van a modificar los horarios
        var bookedSlots = existingAvailability.TimeSlots.Where(ts => ts.IsBooked);
        if (bookedSlots.Any())
        {
            throw new InvalidOperationException("No se puede actualizar disponibilidad que tiene reservas activas");
        }

        // Validar nuevos horarios
        ValidateTimeSlots(updateAvailabilityDto.TimeSlots);

        // Eliminar TimeSlots existentes
        await _timeSlotRepository.DeleteByAvailabilityIdAsync(id);

        // Actualizar availability
        existingAvailability.Date = updateAvailabilityDto.Date;
        existingAvailability.UpdatedAt = DateTime.UtcNow;

        // Crear nuevos TimeSlots
        existingAvailability.TimeSlots = updateAvailabilityDto.TimeSlots.Select(ts => new TimeSlot
        {
            Id = Guid.NewGuid(),
            AvailabilityId = existingAvailability.Id,
            StartTime = TimeSpan.Parse(ts.StartTime),
            EndTime = TimeSpan.Parse(ts.EndTime),
            IsBooked = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var updatedAvailability = await _availabilityRepository.UpdateAsync(existingAvailability);
        
        _logger.LogInformation("Availability {Id} updated for date {Date}", id, existingAvailability.Date);

        return _mapper.Map<AvailabilityDto>(updatedAvailability);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var availability = await _availabilityRepository.GetByIdAsync(id);
        if (availability == null)
        {
            return false;
        }

        // Verificar que no haya reservas activas
        var bookedSlots = availability.TimeSlots.Where(ts => ts.IsBooked);
        if (bookedSlots.Any())
        {
            throw new InvalidOperationException("No se puede eliminar disponibilidad que tiene reservas activas");
        }

        var result = await _availabilityRepository.DeleteAsync(id);
        
        if (result)
        {
            _logger.LogInformation("Availability {Id} deleted for date {Date}", id, availability.Date);
        }

        return result;
    }

    public async Task<bool> BookTimeSlotAsync(Guid timeSlotId, Guid reservaId)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
        if (timeSlot == null)
        {
            throw new ArgumentException("Horario no encontrado");
        }

        if (timeSlot.IsBooked)
        {
            throw new InvalidOperationException("El horario ya está reservado");
        }

        timeSlot.IsBooked = true;
        timeSlot.ReservaId = reservaId;
        timeSlot.UpdatedAt = DateTime.UtcNow;

        await _timeSlotRepository.UpdateAsync(timeSlot);
        
        _logger.LogInformation("TimeSlot {TimeSlotId} booked for Reserva {ReservaId}", timeSlotId, reservaId);

        return true;
    }

    public async Task<bool> UnbookTimeSlotAsync(Guid timeSlotId)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
        if (timeSlot == null)
        {
            return false;
        }

        timeSlot.IsBooked = false;
        timeSlot.ReservaId = null;
        timeSlot.UpdatedAt = DateTime.UtcNow;

        await _timeSlotRepository.UpdateAsync(timeSlot);
        
        _logger.LogInformation("TimeSlot {TimeSlotId} unbooked", timeSlotId);

        return true;
    }

    public async Task<bool> ValidateTimeSlotAvailableAsync(Guid timeSlotId)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
        return timeSlot != null && !timeSlot.IsBooked;
    }

    private static void ValidateTimeSlots(ICollection<CreateTimeSlotDto> timeSlots)
    {
        if (!timeSlots.Any())
        {
            throw new ArgumentException("Debe incluir al menos un horario");
        }

        foreach (var timeSlot in timeSlots)
        {
            var startTime = TimeSpan.Parse(timeSlot.StartTime);
            var endTime = TimeSpan.Parse(timeSlot.EndTime);

            if (startTime >= endTime)
            {
                throw new ArgumentException($"La hora de inicio ({timeSlot.StartTime}) debe ser anterior a la hora de fin ({timeSlot.EndTime})");
            }

            if (startTime.Days > 0 || endTime.Days > 0)
            {
                throw new ArgumentException("Los horarios deben estar dentro del mismo día");
            }
        }

        // Validar solapamientos
        var sortedSlots = timeSlots.Select(ts => new { Start = TimeSpan.Parse(ts.StartTime), End = TimeSpan.Parse(ts.EndTime) }).OrderBy(ts => ts.Start).ToList();
        
        for (int i = 0; i < sortedSlots.Count - 1; i++)
        {
            if (sortedSlots[i].End > sortedSlots[i + 1].Start)
            {
                throw new ArgumentException($"Los horarios se solapan: {sortedSlots[i].Start}-{sortedSlots[i].End} y {sortedSlots[i + 1].Start}-{sortedSlots[i + 1].End}");
            }
        }
    }
}