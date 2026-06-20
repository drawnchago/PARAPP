namespace PsilmtyApp.Models
{
    // ── Auth ─────────────────────────────────────────────────
    public class LoginRequest
    {
        public string UsuarioOCorreo { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public DateTime Expiracion { get; set; }
        public UsuarioInfo Usuario { get; set; } = new();
        public List<ModuloPermiso> Modulos { get; set; } = [];
    }

    public class UsuarioInfo
    {
        public uint Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string NombreUsuario { get; set; } = "";
        public string? FotoUrl { get; set; }
        public int ParishId { get; set; }
        public string? ParishName { get; set; }
        public string? ParishColor { get; set; }
        public string? ParishColorSec { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsParishAdmin { get; set; }
        public bool IsGroupLeader { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }

    public class ModuloPermiso
    {
        public uint ModuloId { get; set; }
        public uint? ParentId { get; set; }
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public bool PuedeVer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
    }

    // ── Adoraciones ──────────────────────────────────────────
    public class AdoracionDto
    {
        public uint Id { get; set; }
        public string Titulo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int? Duracion { get; set; }
        public string Tipo { get; set; } = "";
        public string Estado { get; set; } = "";
        public string? Notas { get; set; }
        public string NombreCreador { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<AdoradorEnAdoracion> Adoradores { get; set; } = [];
    }

    public class AdoradorEnAdoracion
    {
        public uint AdoradorId { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string? Instrumento { get; set; }
        public string? Rol { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }

    public class CrearAdoracionRequest
    {
        public string Titulo { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? Duracion { get; set; }
        public string Tipo { get; set; } = "mixta";
        public string? Notas { get; set; }
        public List<AsignarAdorador> Adoradores { get; set; } = [];
    }

    public class AsignarAdorador
    {
        public uint AdoradorId { get; set; }
        public string? Rol { get; set; }
    }

    // ── Adoradores ───────────────────────────────────────────
    public class AdoradorDto
    {
        public uint Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Instrumento { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CrearAdoradorRequest
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Instrumento { get; set; }
    }

    // ── Eventos ──────────────────────────────────────────────
    public class EventoDto
    {
        public uint Id { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Ubicacion { get; set; }
        public string? Tipo { get; set; }
        public string Estado { get; set; } = "";
        public int? Capacidad { get; set; }
        public string NombreCreador { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<AdoracionEnEvento> Adoraciones { get; set; } = [];
    }

    public class AdoracionEnEvento
    {
        public uint AdoracionId { get; set; }
        public string Titulo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public byte Orden { get; set; }
    }

    public class CrearEventoRequest
    {
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }
        public string? Ubicacion { get; set; }
        public string? Tipo { get; set; }
        public int? Capacidad { get; set; }
        public List<uint> AdoracionIds { get; set; } = [];
    }

    // ── Configuración ────────────────────────────────────────
    public class PreferenciasDto
    {
        public uint LenguajeId { get; set; }
        public uint TemaId { get; set; }
        public bool NotifPushActivas { get; set; }
        public bool NotifEventos { get; set; }
        public bool NotifNoticias { get; set; }
        public bool NotifRecordatorios { get; set; }
        public bool NotifAdoracion { get; set; }
        public bool NotifSonido { get; set; }
        public bool NotifVibracion { get; set; }
        public string CalendarioVista { get; set; } = "mes";
        public byte CalendarioPrimerDia { get; set; }
        public string TamanoFuente { get; set; } = "normal";
        public bool AltoContraste { get; set; }
        public LenguajeDto? Lenguaje { get; set; }
        public TemaDto? Tema { get; set; }
    }

    public class LenguajeDto
    {
        public uint Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string NombreNativo { get; set; } = "";
        public string? BanderaEmoji { get; set; }
        public bool EsDefault { get; set; }
        public byte Orden { get; set; }
    }

    public class TemaDto
    {
        public uint Id { get; set; }
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string ColorPrimario { get; set; } = "";
        public string ColorSecundario { get; set; } = "";
        public string ColorFondo { get; set; } = "";
        public string ColorSuperficie { get; set; } = "";
        public string ColorTexto { get; set; } = "";
        public string ColorTextoSuave { get; set; } = "";
        public string ColorAcento { get; set; } = "";
        public string ColorError { get; set; } = "#D32F2F";
        public bool EsOscuro { get; set; }
        public bool EsDefault { get; set; }
    }

    public class ActualizarPreferenciasRequest
    {
        public uint? LenguajeId { get; set; }
        public uint? TemaId { get; set; }
        public bool? NotifPushActivas { get; set; }
        public bool? NotifEventos { get; set; }
        public bool? NotifNoticias { get; set; }
        public bool? NotifRecordatorios { get; set; }
        public bool? NotifAdoracion { get; set; }
        public bool? NotifSonido { get; set; }
        public bool? NotifVibracion { get; set; }
        public string? CalendarioVista { get; set; }
        public byte? CalendarioPrimerDia { get; set; }
        public string? TamanoFuente { get; set; }
        public bool? AltoContraste { get; set; }
    }

    // ── Usuarios ─────────────────────────────────────────────
    public class UsuarioDto
    {
        public uint Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string? Apellido2 { get; set; }
        public string Correo { get; set; } = "";
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }
        public string? Address { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string? StateName { get; set; }
        public string? NeighborhoodName { get; set; }
        public bool Activo { get; set; }
        public int ParishId { get; set; }
        public string? ParishName { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<GrupoMembresiaDto> Grupos { get; set; } = [];
        public string? CreatedAt { get; set; }
        public string? LastLogin { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido} {Apellido2}".Trim();
    }

    public class GrupoMembresiaDto
    {
        public string Name { get; set; } = "";
        public string RoleInGroup { get; set; } = "";
    }

    public class CrearUsuarioForm
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? SecondLastName { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? MobilePhone { get; set; }
        public string Gender { get; set; } = "unspecified";
        public string RoleName { get; set; } = "parishioner";
        public int? ParishId { get; set; }
        public string? Address { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
    }

    // ── Permisos ─────────────────────────────────────────────
    public class ModuloPermisoDetalle
    {
        public uint ModuleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class AsignarPermisosRequest
    {
        public List<uint> ModuleIds { get; set; } = [];
        public PermisosFlags Permissions { get; set; } = new();
    }

    public class PermisosFlags
    {
        public bool View { get; set; } = true;
        public bool Create { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
    }

    public class ModuloInfo
    {
        public uint ModuleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Icon { get; set; }
        public string? Route { get; set; }
    }

    // ── Roles ─────────────────────────────────────────────────
    public class RolDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
    }

    // ── Catálogo de Roles (CRUD) ──────────────────────────────
    public class RolCatalogoDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RolCatalogoForm
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── Grupos ────────────────────────────────────────────────
    public class GrupoDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int ParishId { get; set; }
        public string? ParishName { get; set; }
        public string? LeaderName { get; set; }
        public int MemberCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class GrupoMiembroDto
    {
        public uint UserId { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }
        public string RoleInGroup { get; set; } = "";
        public string? JoinedAt { get; set; }
    }

    // ── Parroquias ────────────────────────────────────────────
    public class ParroquiaDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string? Diocese { get; set; }
        public string? Address { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? PatronSaint { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public bool IsActive { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }

        // alias para código previo (filtros en Usuarios.razor)
        public string Nombre => Name;
        public string Ciudad => City ?? "";
    }

    public class ParishScheduleDayDto
    {
        public byte DayOfWeek { get; set; }
        public string DayName { get; set; } = "";
        public bool IsClosed { get; set; }
        public string? Notes { get; set; }
        public List<ParishScheduleBlockDto> Blocks { get; set; } = [];
    }

    public class ParishScheduleBlockDto
    {
        public uint Id { get; set; }
        public string OpenTime { get; set; } = "09:00";
        public string CloseTime { get; set; } = "13:00";
    }

    public class ParishScheduleRequest
    {
        public List<ParishScheduleDayDto> Days { get; set; } = [];
    }

    // ── Registro público de feligrés ──────────────────────────
    public class ParroquiaPublicaDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string Display => string.IsNullOrEmpty(City) ? Name : $"{Name} — {City}";
    }

    public class RegistroRequest
    {
        public string FirstName { get; set; } = "";
        public string? SecondName { get; set; }
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public uint ParishId { get; set; }
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public string? Address { get; set; }
        public string? Neighborhood { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class PaisLada
    {
        public string Nombre { get; set; } = "";
        public string Codigo { get; set; } = "";   // ej: +52
        public string Bandera { get; set; } = "";
        public string Display => $"{Bandera} {Nombre} ({Codigo})";
    }

    // ── Perfil propio + contraseña ────────────────────────────
    public class PerfilDto
    {
        public uint Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Apellido2 { get; set; }
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? ParishName { get; set; }
        public string? ZipCode { get; set; }
        public uint? CountryId { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string? NeighborhoodName { get; set; }
    }

    public class ActualizarPerfilRequest
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? LastName { get; set; }
        public string? MobilePhone { get; set; }
        public string? Address { get; set; }
        public string? Neighborhood { get; set; }
        public uint? StateId { get; set; }
        public uint? NeighborhoodId { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class CountryDto
    {
        public uint Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class StateCatalogDto
    {
        public uint Id { get; set; }
        public uint CountryId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string CountryName { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class NeighborhoodCatalogDto
    {
        public uint Id { get; set; }
        public uint StateId { get; set; }
        public string PostalCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? SettlementType { get; set; }
        public string? Municipality { get; set; }
        public string? City { get; set; }
        public string Display { get; set; } = "";
        public string StateName { get; set; } = "";
        public string CountryName { get; set; } = "";
        public string? PostalStateCode { get; set; }
        public string? MunicipalityCode { get; set; }
        public string? SettlementCode { get; set; }
        public string? Zone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CountryCatalogForm
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class StateCatalogForm
    {
        public uint CountryId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class NeighborhoodCatalogForm
    {
        public uint CountryId { get; set; }
        public uint StateId { get; set; }
        public string PostalCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? SettlementType { get; set; }
        public string? Municipality { get; set; }
        public string? City { get; set; }
        public string? PostalStateCode { get; set; }
        public string? MunicipalityCode { get; set; }
        public string? SettlementCode { get; set; }
        public string? Zone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CambiarPasswordRequest
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    // ── Noticias ──────────────────────────────────────────────
    public class NoticiaDto
    {
        public uint Id { get; set; }
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; }
        public string? PublishedAt { get; set; }
        public bool IsFeatured { get; set; }
        public int Views { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class NoticiaForm
    {
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool SendNotification { get; set; }
        public int ReminderMinutes { get; set; } = 60;
    }

    public class CategoriaDto
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }

    // ── Calendario ────────────────────────────────────────────
    public class CalendarioDto
    {
        public uint Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Color { get; set; } = "#4E342E";
        public string StartDatetime { get; set; } = "";
        public string EndDatetime { get; set; } = "";
        public bool AllDay { get; set; }
        public string Type { get; set; } = "event";
        public string? Location { get; set; }
        public int ReminderMinutes { get; set; }
    }

    public class CalendarioForm
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Color { get; set; } = "#4E342E";
        public DateTime StartDatetime { get; set; } = DateTime.Now;
        public DateTime EndDatetime { get; set; } = DateTime.Now.AddHours(1);
        public bool AllDay { get; set; }
        public string Type { get; set; } = "event";
        public string? Location { get; set; }
        public int ReminderMinutes { get; set; } = 60;
        public bool SendNotification { get; set; }
    }

    // ── Sacramentos ───────────────────────────────────────────
    public class SacramentoDto
    {
        public uint Id { get; set; }
        public uint UserId { get; set; }
        public string? UserName { get; set; }
        public uint TypeId { get; set; }
        public string? TypeName { get; set; }
        public string Date { get; set; } = "";
        public string? Minister { get; set; }
        public string? Godfather { get; set; }
        public string? Godmother { get; set; }
        public string? RecordNumber { get; set; }
        public string? Observations { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class SacramentoForm
    {
        public uint UserId { get; set; }
        public uint TypeId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Minister { get; set; }
        public string? Godfather { get; set; }
        public string? Godmother { get; set; }
        public string? RecordNumber { get; set; }
        public string? Observations { get; set; }
    }

    // ── Donaciones ────────────────────────────────────────────
    public class DonacionDto
    {
        public uint Id { get; set; }
        public uint? UserId { get; set; }
        public string? UserName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "MXN";
        public string Type { get; set; } = "offering";
        public string? Concept { get; set; }
        public string PaymentMethod { get; set; } = "cash";
        public bool IsAnonymous { get; set; }
        public string Status { get; set; } = "completed";
        public string? DonatedAt { get; set; }
    }

    public class DonacionForm
    {
        public uint? UserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "offering";
        public string? Concept { get; set; }
        public string PaymentMethod { get; set; } = "cash";
        public bool IsAnonymous { get; set; }
    }

    public class DonacionResumen
    {
        public decimal Total { get; set; }
        public decimal TotalTithe { get; set; }
        public decimal TotalOffering { get; set; }
        public int Count { get; set; }
    }

    // ── Intenciones de Misa ───────────────────────────────────
    public class IntencionDto
    {
        public uint Id { get; set; }
        public uint UserId { get; set; }
        public string? UserName { get; set; }
        public string Intention { get; set; } = "";
        public string Type { get; set; } = "petition";
        public string? MassDate { get; set; }
        public string Status { get; set; } = "pending";
        public decimal? Donation { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class IntencionForm
    {
        public uint? UserId { get; set; }
        public string Intention { get; set; } = "";
        public string Type { get; set; } = "petition";
        public DateTime? MassDate { get; set; }
        public decimal? Donation { get; set; }
    }

    // ── Solicitudes ───────────────────────────────────────────
    public class SolicitudDto
    {
        public uint Id { get; set; }
        public uint UserId { get; set; }
        public string? UserName { get; set; }
        public uint ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public string? DesiredDate { get; set; }
        public string Status { get; set; } = "new";
        public string? Response { get; set; }
        public string? AttendedName { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class SolicitudForm
    {
        public uint ServiceId { get; set; }
        public string? Description { get; set; }
        public DateTime? DesiredDate { get; set; }
    }

    // ── Asignación de módulos (con vigencia) ──────────────────
    public class ModuloBusquedaDto
    {
        public uint ModuleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Route { get; set; }
    }

    public class AccesoUsuarioDto
    {
        public uint ModuleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidTo { get; set; }
        public bool Vigente { get; set; }
    }

    public class UsuarioAccesoDto
    {
        public uint Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? FotoUrl { get; set; }
        public string? ParishName { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidTo { get; set; }
        public bool Vigente { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
