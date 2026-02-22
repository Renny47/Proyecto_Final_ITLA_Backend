namespace SIGID.Domain.Enums;

public enum TipoUsuario
{
    Cliente = 1,
    Empleado = 2,
    Administrador = 3
}

public enum EstadoReserva
{
    Pendiente = 1,
    Confirmada = 2,
    Cancelada = 3,
    Completada = 4
}

public enum EstadoTurno
{
    Programado = 1,
    EnCurso = 2,
    Completado = 3,
    Cancelado = 4
}

public enum CategoriaInventario
{
    Bebidas = 1,
    Comidas = 2,
    Ingredientes = 3,
    Utensilios = 4,
    Limpieza = 5
}

public enum NivelStock
{
    Critico = 1,
    Bajo = 2,
    Normal = 3,
    Alto = 4
}