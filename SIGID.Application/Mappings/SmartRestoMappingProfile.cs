using AutoMapper;
using SIGID.Application.DTOs;
using SIGID.Domain.Entities;

namespace SIGID.Application.Mappings;

public class SmartRestoMappingProfile : Profile
{
    public SmartRestoMappingProfile()
    {
        // Usuario mappings
        CreateMap<Usuario, UsuarioDto>().ReverseMap();
        
        // Empleado mappings
        CreateMap<Empleado, EmpleadoDto>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => 
                src.Usuario != null ? $"{src.Usuario.FirstName} {src.Usuario.LastName}" : null))
            .ForMember(dest => dest.UsuarioEmail, opt => opt.MapFrom(src => 
                src.Usuario != null ? src.Usuario.Email : null));
        
        CreateMap<CreateEmpleadoDto, Empleado>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());
        
        CreateMap<EmpleadoDto, Empleado>()
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());

        // Reserva mappings
        CreateMap<Reserva, ReservaDto>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => 
                src.Usuario != null ? $"{src.Usuario.FirstName} {src.Usuario.LastName}" : null))
            .ForMember(dest => dest.UsuarioEmail, opt => opt.MapFrom(src => 
                src.Usuario != null ? src.Usuario.Email : null));
        
        CreateMap<CreateReservaDto, Reserva>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaReserva, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());
        
        CreateMap<UpdateReservaDto, Reserva>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaReserva, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());

        // Turno mappings
        CreateMap<Turno, TurnoDto>()
            .ForMember(dest => dest.EmpleadoNombre, opt => opt.MapFrom(src => 
                src.Empleado != null && src.Empleado.Usuario != null ? 
                $"{src.Empleado.Usuario.FirstName} {src.Empleado.Usuario.LastName}" : null))
            .ForMember(dest => dest.EmpleadoPuesto, opt => opt.MapFrom(src => 
                src.Empleado != null ? src.Empleado.Puesto : null));
        
        CreateMap<CreateTurnoDto, Turno>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => Domain.Enums.EstadoTurno.Programado))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Empleado, opt => opt.Ignore());
        
        CreateMap<UpdateTurnoDto, Turno>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmpleadoId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Empleado, opt => opt.Ignore());

        // Inventario mappings
        CreateMap<Inventario, InventarioDto>()
            .ForMember(dest => dest.RequiereReposicion, opt => opt.MapFrom(src => src.RequiereReposicion))
            .ForMember(dest => dest.EstaVencido, opt => opt.MapFrom(src => src.EstaVencido));
        
        CreateMap<CreateInventarioDto, Inventario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NivelStock, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PrediccionesRelacionadas, opt => opt.Ignore());
        
        CreateMap<UpdateInventarioDto, Inventario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PrediccionesRelacionadas, opt => opt.Ignore());

        // PrediccionDemanda mappings
        CreateMap<PrediccionDemanda, PrediccionDemandaDto>()
            .ForMember(dest => dest.EsPrediccionActual, opt => opt.MapFrom(src => src.EsPrediccionActual));
        
        CreateMap<CreatePrediccionDemandaDto, PrediccionDemanda>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaPrediccion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasPronosticadas, opt => opt.Ignore())
            .ForMember(dest => dest.PersonasEstimadas, opt => opt.Ignore())
            .ForMember(dest => dest.IngresosEstimados, opt => opt.Ignore())
            .ForMember(dest => dest.ElementosAltos, opt => opt.Ignore())
            .ForMember(dest => dest.ElementosMedios, opt => opt.Ignore())
            .ForMember(dest => dest.ElementosBajos, opt => opt.Ignore())
            .ForMember(dest => dest.PromedioReservasDiarias, opt => opt.Ignore())
            .ForMember(dest => dest.PromedioPersonasPorReserva, opt => opt.Ignore())
            .ForMember(dest => dest.PromedioIngresoPorPersona, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Administrador mappings
        CreateMap<Administrador, AdministradorDto>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => 
                src.Usuario != null ? $"{src.Usuario.FirstName} {src.Usuario.LastName}" : null))
            .ForMember(dest => dest.UsuarioEmail, opt => opt.MapFrom(src => 
                src.Usuario != null ? src.Usuario.Email : null))
            .ForMember(dest => dest.UsuarioTelefono, opt => opt.MapFrom(src => 
                src.Usuario != null ? src.Usuario.PhoneNumber : null));
        
        CreateMap<CreateAdministradorDto, Administrador>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaAsignacion, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());
        
        CreateMap<UpdateAdministradorDto, Administrador>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaAsignacion, opt => opt.Ignore())
            .ForMember(dest => dest.Usuario, opt => opt.Ignore());

        // Availability mappings
        CreateMap<Availability, AvailabilityDto>()
            .ForMember(dest => dest.TimeSlots, opt => opt.MapFrom(src => src.TimeSlots));
        
        CreateMap<CreateAvailabilityDto, Availability>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeSlots, opt => opt.Ignore());

        // TimeSlot mappings
        CreateMap<TimeSlot, TimeSlotDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));
        
        CreateMap<CreateTimeSlotDto, TimeSlot>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AvailabilityId, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.EndTime)))
            .ForMember(dest => dest.IsBooked, opt => opt.Ignore())
            .ForMember(dest => dest.ReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Availability, opt => opt.Ignore())
            .ForMember(dest => dest.Reserva, opt => opt.Ignore());
    }
}