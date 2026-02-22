using AutoMapper;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservaService> _logger;

    public ReservaService(
        IReservaRepository reservaRepository,
        IUsuarioRepository usuarioRepository,
        IMapper mapper,
        ILogger<ReservaService> logger)
    {
        _reservaRepository = reservaRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReservaDto?> GetByIdAsync(Guid id)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id);
        return reserva != null ? _mapper.Map<ReservaDto>(reserva) : null;
    }

    public async Task<IEnumerable<ReservaDto>> GetAllAsync()
    {
        var reservas = await _reservaRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
    }

    public async Task<IEnumerable<ReservaDto>> GetByUsuarioAsync(string usuarioId)
    {
        var reservas = await _reservaRepository.GetByUsuarioAsync(usuarioId);
        return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
    }

    public async Task<IEnumerable<ReservaDto>> GetByEstadoAsync(EstadoReserva estado)
    {
        var reservas = await _reservaRepository.GetByEstadoAsync(estado);
        return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
    }

    public async Task<IEnumerable<ReservaDto>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetByFechaRangoAsync(fechaInicio, fechaFin);
        return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
    }

    public async Task<IEnumerable<ReservaDto>> GetReservasDelDiaAsync(DateTime fecha)
    {
        var reservas = await _reservaRepository.GetReservasDelDiaAsync(fecha);
        return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
    }

    public async Task<ReservaDto> CreateAsync(CreateReservaDto createReservaDto)
    {
        // Validar que el usuario existe
        var usuario = await _usuarioRepository.GetByIdAsync(createReservaDto.UsuarioId);
        if (usuario == null)
            throw new ArgumentException("Usuario no encontrado");

        // Validar fecha futura
        if (createReservaDto.FechaHoraReserva <= DateTime.Now)
            throw new ArgumentException("La fecha de reserva debe ser en el futuro");

        // Verificar capacidad disponible 
        var capacidadDisponible = await GetCapacidadDisponibleAsync(
            createReservaDto.FechaHoraReserva.Date, 
            createReservaDto.FechaHoraReserva.TimeOfDay);

        if (capacidadDisponible < createReservaDto.NumeroPersonas)
            throw new InvalidOperationException($"Capacidad insuficiente. Disponible: {capacidadDisponible} personas");

        var reserva = _mapper.Map<Reserva>(createReservaDto);
        reserva.Id = Guid.NewGuid();
        reserva.FechaReserva = DateTime.UtcNow;
        reserva.Estado = EstadoReserva.Pendiente;
        reserva.CreatedAt = DateTime.UtcNow;

        var nuevaReserva = await _reservaRepository.CreateAsync(reserva);
        
        _logger.LogInformation("Nueva reserva creada: {Id} para {NumeroPersonas} personas el {Fecha}", 
            nuevaReserva.Id, nuevaReserva.NumeroPersonas, nuevaReserva.FechaHoraReserva);

        return _mapper.Map<ReservaDto>(nuevaReserva);
    }

    public async Task<ReservaDto> UpdateAsync(Guid id, UpdateReservaDto updateReservaDto)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id);
        if (reserva == null)
            throw new ArgumentException("Reserva no encontrada");

        // Validar fecha futura si se cambia
        if (updateReservaDto.FechaHoraReserva != reserva.FechaHoraReserva && 
            updateReservaDto.FechaHoraReserva <= DateTime.Now)
            throw new ArgumentException("La fecha de reserva debe ser en el futuro");

        _mapper.Map(updateReservaDto, reserva);
        reserva.UpdatedAt = DateTime.UtcNow;

        var reservaActualizada = await _reservaRepository.UpdateAsync(reserva);
        return _mapper.Map<ReservaDto>(reservaActualizada);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _reservaRepository.DeleteAsync(id);
    }

    public async Task<bool> ConfirmarReservaAsync(Guid id)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id);
        if (reserva == null) return false;

        reserva.Estado = EstadoReserva.Confirmada;
        reserva.UpdatedAt = DateTime.UtcNow;

        await _reservaRepository.UpdateAsync(reserva);
        
        _logger.LogInformation("Reserva confirmada: {Id}", id);
        return true;
    }

    public async Task<bool> CancelarReservaAsync(Guid id)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id);
        if (reserva == null) return false;

        reserva.Estado = EstadoReserva.Cancelada;
        reserva.UpdatedAt = DateTime.UtcNow;

        await _reservaRepository.UpdateAsync(reserva);
        
        _logger.LogInformation("Reserva cancelada: {Id}", id);
        return true;
    }

    public async Task<int> GetCapacidadDisponibleAsync(DateTime fecha, TimeSpan hora)
    {
        // Obtener reservas del mismo día y hora similar (±1 hora)
        var reservasDelDia = await _reservaRepository.GetReservasDelDiaAsync(fecha.Date);
        
        var horaInicio = hora.Subtract(TimeSpan.FromHours(1));
        var horaFin = hora.Add(TimeSpan.FromHours(1));

        var reservasEnRango = reservasDelDia.Where(r => 
            r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Pendiente)
            .Where(r => 
            {
                var horaReserva = r.FechaHoraReserva.TimeOfDay;
                return horaReserva >= horaInicio && horaReserva <= horaFin;
            });

        var personasReservadas = reservasEnRango.Sum(r => r.NumeroPersonas);
        
        // Capacidad máxima del restaurante (esto podría venir de configuración)
        const int CapacidadMaxima = 100;
        
        return Math.Max(0, CapacidadMaxima - personasReservadas);
    }
}