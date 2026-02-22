using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGID.API.Migrations
{
    /// <inheritdoc />
    public partial class UnifyUserToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas de Usuario a la tabla Users (idempotente por si la migración se re-ejecuta)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Direccion')
                    ALTER TABLE [Users] ADD [Direccion] nvarchar(200) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EsAdministrador')
                    ALTER TABLE [Users] ADD [EsAdministrador] bit NOT NULL DEFAULT CAST(0 AS bit);
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EsEmpleado')
                    ALTER TABLE [Users] ADD [EsEmpleado] bit NOT NULL DEFAULT CAST(0 AS bit);
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FechaNacimiento')
                    ALTER TABLE [Users] ADD [FechaNacimiento] datetime2 NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Telefono')
                    ALTER TABLE [Users] ADD [Telefono] nvarchar(20) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'TipoUsuario')
                    ALTER TABLE [Users] ADD [TipoUsuario] int NOT NULL DEFAULT 0;
            ");

            // Copiar datos desde Usuarios (TPT) a Users si existe la tabla Usuarios
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
                BEGIN
                    UPDATE u SET
                        u.TipoUsuario = us.TipoUsuario,
                        u.FechaNacimiento = us.FechaNacimiento,
                        u.Telefono = us.Telefono,
                        u.Direccion = us.Direccion,
                        u.EsEmpleado = us.EsEmpleado,
                        u.EsAdministrador = us.EsAdministrador
                    FROM Users u
                    INNER JOIN Usuarios us ON u.Id = us.Id;
                END
            ");

            // Quitar FKs que referencian Usuarios antes de eliminar la tabla
            migrationBuilder.DropForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores");
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Usuarios_UsuarioId",
                table: "Empleados");
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId",
                table: "Reservas");

            // Eliminar tabla Usuarios (TPT)
            migrationBuilder.DropTable(name: "Usuarios");

            // Renombrar Users a Usuarios
            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Usuarios");

            // Volver a crear FKs hacia Usuarios (tabla renombrada)
            migrationBuilder.AddForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Usuarios_UsuarioId",
                table: "Empleados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId",
                table: "Reservas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Quitar FKs que apuntan a Usuarios
            migrationBuilder.DropForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores");
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Usuarios_UsuarioId",
                table: "Empleados");
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId",
                table: "Reservas");

            // Renombrar Usuarios de vuelta a Users
            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Users");

            // Recrear tabla Usuarios (TPT)
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TipoUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EsEmpleado = table.Column<bool>(type: "bit", nullable: false),
                    EsAdministrador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Copiar datos de Users a Usuarios antes de quitar columnas
            migrationBuilder.Sql(@"
                INSERT INTO Usuarios (Id, TipoUsuario, FechaNacimiento, Telefono, Direccion, EsEmpleado, EsAdministrador)
                SELECT Id, TipoUsuario, FechaNacimiento, Telefono, Direccion, EsEmpleado, EsAdministrador FROM Users
            ");

            // Restaurar FKs hacia Usuarios
            migrationBuilder.AddForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Usuarios_UsuarioId",
                table: "Empleados",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Usuarios_UsuarioId",
                table: "Reservas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Quitar columnas de Users
            migrationBuilder.DropColumn(name: "Direccion", table: "Users");
            migrationBuilder.DropColumn(name: "EsAdministrador", table: "Users");
            migrationBuilder.DropColumn(name: "EsEmpleado", table: "Users");
            migrationBuilder.DropColumn(name: "FechaNacimiento", table: "Users");
            migrationBuilder.DropColumn(name: "Telefono", table: "Users");
            migrationBuilder.DropColumn(name: "TipoUsuario", table: "Users");
        }
    }
}
