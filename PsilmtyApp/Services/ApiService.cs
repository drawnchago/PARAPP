using PsilmtyApp.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PsilmtyApp.Services
{
    public class ApiService(HttpClient http, SessionService session)
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private void SetAuth()
        {
            http.DefaultRequestHeaders.Authorization = session.IsAuthenticated
                ? new AuthenticationHeaderValue("Bearer", session.Session!.Token)
                : null;
        }

        // ── Auth ─────────────────────────────────────────────
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await http.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts);
        }

        // ── Registro público (feligrés) ──────────────────────
        public async Task<List<ParroquiaPublicaDto>> GetPublicParishesAsync()
        {
            try { return await http.GetFromJsonAsync<List<ParroquiaPublicaDto>>("api/auth/parroquias", JsonOpts) ?? []; }
            catch { return []; }
        }

        /// <summary>Registra un feligrés. Devuelve (sesión, errorMensaje).</summary>
        public async Task<(LoginResponse? sesion, string? error)> RegisterAsync(RegistroRequest req)
        {
            var response = await http.PostAsJsonAsync("api/auth/registro", req);
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts), null);

            try
            {
                var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOpts);
                return (null, err != null && err.TryGetValue("message", out var m) ? m : "No se pudo registrar.");
            }
            catch { return (null, "No se pudo registrar."); }
        }

        // ── Adoraciones ──────────────────────────────────────
        public async Task<List<AdoracionDto>> GetAdorationsAsync()
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<AdoracionDto>>("api/adoraciones", JsonOpts);
            return r ?? [];
        }

        public async Task<AdoracionDto?> GetAdorationAsync(uint id)
        {
            SetAuth();
            return await http.GetFromJsonAsync<AdoracionDto>($"api/adoraciones/{id}", JsonOpts);
        }

        public async Task<AdoracionDto?> CreateAdorationAsync(CrearAdoracionRequest req)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync("api/adoraciones", req);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<AdoracionDto>(JsonOpts) : null;
        }

        public async Task<bool> DeleteAdorationAsync(uint id)
        {
            SetAuth();
            var r = await http.DeleteAsync($"api/adoraciones/{id}");
            return r.IsSuccessStatusCode;
        }

        // ── Adoradores ───────────────────────────────────────
        public async Task<List<AdoradorDto>> GetAdorersAsync(bool soloActivos = false)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<AdoradorDto>>($"api/adoradores?soloActivos={soloActivos}", JsonOpts);
            return r ?? [];
        }

        public async Task<AdoradorDto?> CreateAdorerAsync(CrearAdoradorRequest req)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync("api/adoradores", req);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<AdoradorDto>(JsonOpts) : null;
        }

        public async Task<bool> DeleteAdorerAsync(uint id)
        {
            SetAuth();
            var r = await http.DeleteAsync($"api/adoradores/{id}");
            return r.IsSuccessStatusCode;
        }

        // ── Eventos ──────────────────────────────────────────
        public async Task<List<EventoDto>> GetEventsAsync()
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<EventoDto>>("api/eventos", JsonOpts);
            return r ?? [];
        }

        public async Task<EventoDto?> CreateEventAsync(CrearEventoRequest req)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync("api/eventos", req);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<EventoDto>(JsonOpts) : null;
        }

        public async Task<bool> DeleteEventAsync(uint id)
        {
            SetAuth();
            var r = await http.DeleteAsync($"api/eventos/{id}");
            return r.IsSuccessStatusCode;
        }

        // ── Configuración ────────────────────────────────────
        public async Task<List<LenguajeDto>> GetLanguagesAsync()
        {
            var r = await http.GetFromJsonAsync<List<LenguajeDto>>("api/configuracion/lenguajes", JsonOpts);
            return r ?? [];
        }

        public async Task<List<TemaDto>> GetThemesAsync()
        {
            var r = await http.GetFromJsonAsync<List<TemaDto>>("api/configuracion/temas", JsonOpts);
            return r ?? [];
        }

        public async Task<PreferenciasDto?> GetPreferencesAsync()
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<PreferenciasDto>("api/configuracion/preferencias", JsonOpts); }
            catch { return null; }
        }

        public async Task<PreferenciasDto?> UpdatePreferencesAsync(ActualizarPreferenciasRequest req)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync("api/configuracion/preferencias", req);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<PreferenciasDto>(JsonOpts) : null;
        }

        // ── Parroquia ────────────────────────────────────────
        public async Task<ParroquiaDto?> GetMyParishAsync()
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<ParroquiaDto>("api/parroquias/mia", JsonOpts); }
            catch { return null; }
        }

        public async Task<ParroquiaDto?> UpdateMyParishAsync(object body)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync("api/parroquias/mia", body);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<ParroquiaDto>(JsonOpts) : null;
        }

        public async Task<List<ParroquiaDto>> GetParishesAsync()
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<List<ParroquiaDto>>("api/parroquias", JsonOpts) ?? []; }
            catch { return []; }
        }

        public async Task<ParroquiaDto?> UpdateParishAsync(uint id, object body)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync($"api/parroquias/{id}", body);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<ParroquiaDto>(JsonOpts) : null;
        }

        public async Task<List<ParishScheduleDayDto>> GetParishSchedulesAsync(uint parishId)
        {
            SetAuth();
            try
            {
                return await http.GetFromJsonAsync<List<ParishScheduleDayDto>>(
                    $"api/parroquias/{parishId}/horarios", JsonOpts) ?? [];
            }
            catch { return []; }
        }

        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            try { return await http.GetFromJsonAsync<List<CountryDto>>("api/catalogos/paises", JsonOpts) ?? []; }
            catch { return []; }
        }

        public async Task<List<StateCatalogDto>> GetStatesAsync(uint countryId)
        {
            try { return await http.GetFromJsonAsync<List<StateCatalogDto>>($"api/catalogos/estados?countryId={countryId}", JsonOpts) ?? []; }
            catch { return []; }
        }

        public async Task<List<NeighborhoodCatalogDto>> GetNeighborhoodsAsync(
            uint stateId,
            string query = "",
            string postalCode = "")
        {
            try
            {
                var url = $"api/catalogos/colonias?stateId={stateId}&q={Uri.EscapeDataString(query)}&postalCode={Uri.EscapeDataString(postalCode)}";
                return await http.GetFromJsonAsync<List<NeighborhoodCatalogDto>>(url, JsonOpts) ?? [];
            }
            catch { return []; }
        }

        public async Task<List<CountryDto>> GetAdminCountriesAsync(string query = "")
        {
            SetAuth();
            return await GetListAsync<CountryDto>($"api/catalogos/admin/paises?q={Uri.EscapeDataString(query)}&includeInactive=true");
        }

        public Task<CountryDto?> CreateCountryAsync(CountryCatalogForm form) =>
            PostAsync<CountryDto>("api/catalogos/admin/paises", form);
        public Task<CountryDto?> UpdateCountryAsync(uint id, CountryCatalogForm form) =>
            PutAsync<CountryDto>($"api/catalogos/admin/paises/{id}", form);
        public Task<bool> DeactivateCountryAsync(uint id) =>
            DeleteAsync($"api/catalogos/admin/paises/{id}");

        public async Task<List<StateCatalogDto>> GetAdminStatesAsync(uint? countryId, string query = "")
        {
            SetAuth();
            var country = countryId.HasValue ? countryId.Value.ToString() : "";
            return await GetListAsync<StateCatalogDto>(
                $"api/catalogos/admin/estados?countryId={country}&q={Uri.EscapeDataString(query)}&includeInactive=true");
        }

        public Task<StateCatalogDto?> CreateStateAsync(StateCatalogForm form) =>
            PostAsync<StateCatalogDto>("api/catalogos/admin/estados", form);
        public Task<StateCatalogDto?> UpdateStateAsync(uint id, StateCatalogForm form) =>
            PutAsync<StateCatalogDto>($"api/catalogos/admin/estados/{id}", form);
        public Task<bool> DeactivateStateAsync(uint id) =>
            DeleteAsync($"api/catalogos/admin/estados/{id}");

        public async Task<List<NeighborhoodCatalogDto>> GetAdminNeighborhoodsAsync(
            uint? stateId,
            string query = "",
            string postalCode = "")
        {
            SetAuth();
            var state = stateId.HasValue ? stateId.Value.ToString() : "";
            return await GetListAsync<NeighborhoodCatalogDto>(
                $"api/catalogos/admin/colonias?stateId={state}&q={Uri.EscapeDataString(query)}&postalCode={Uri.EscapeDataString(postalCode)}&includeInactive=true");
        }

        public Task<NeighborhoodCatalogDto?> CreateNeighborhoodAsync(NeighborhoodCatalogForm form) =>
            PostAsync<NeighborhoodCatalogDto>("api/catalogos/admin/colonias", form);
        public Task<NeighborhoodCatalogDto?> UpdateNeighborhoodAsync(uint id, NeighborhoodCatalogForm form) =>
            PutAsync<NeighborhoodCatalogDto>($"api/catalogos/admin/colonias/{id}", form);
        public Task<bool> DeactivateNeighborhoodAsync(uint id) =>
            DeleteAsync($"api/catalogos/admin/colonias/{id}");

        // ── Catálogo de Roles (CRUD) ──────────────────────────
        public async Task<List<RolCatalogoDto>> GetAdminRolesAsync(string query = "")
        {
            SetAuth();
            return await GetListAsync<RolCatalogoDto>(
                $"api/catalogos/admin/roles?q={Uri.EscapeDataString(query)}&includeInactive=true");
        }

        public Task<RolCatalogoDto?> CreateRoleAsync(RolCatalogoForm form) =>
            PostAsync<RolCatalogoDto>("api/catalogos/admin/roles", form);
        public Task<RolCatalogoDto?> UpdateRoleAsync(uint id, RolCatalogoForm form) =>
            PutAsync<RolCatalogoDto>($"api/catalogos/admin/roles/{id}", form);
        public Task<bool> DeactivateRoleAsync(uint id) =>
            DeleteAsync($"api/catalogos/admin/roles/{id}");

        public async Task<List<ParishScheduleDayDto>?> SaveParishSchedulesAsync(
            uint parishId,
            ParishScheduleRequest request)
        {
            SetAuth();
            var response = await http.PutAsJsonAsync($"api/parroquias/{parishId}/horarios", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<ParishScheduleDayDto>>(JsonOpts)
                : null;
        }

        /// <summary>Sube un logo de parroquia. Devuelve (urlLogo, error).</summary>
        public async Task<(string? url, string? error)> UploadParishLogoAsync(Stream archivo, string nombre, uint? parishId)
        {
            SetAuth();
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(archivo), "logo", nombre);
            if (parishId.HasValue)
                content.Add(new StringContent(parishId.Value.ToString()), "parishId");

            var r = await http.PostAsync("api/parroquias/logo", content);
            if (r.IsSuccessStatusCode)
            {
                var data = await r.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(JsonOpts);
                return (data != null && data.TryGetValue("logoUrl", out var u) ? u.GetString() : null, null);
            }
            try
            {
                var err = await r.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOpts);
                return (null, err != null && err.TryGetValue("message", out var m) ? m : "No se pudo subir el logo.");
            }
            catch { return (null, "No se pudo subir el logo."); }
        }

        // ── Usuarios ─────────────────────────────────────────
        public async Task<List<UsuarioDto>> SearchUsersAsync(string q)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<UsuarioDto>>($"api/usuarios/buscar?q={Uri.EscapeDataString(q)}", JsonOpts);
            return r ?? [];
        }

        public async Task<List<UsuarioDto>> GetUsersAsync(string q = "", string parishId = "", string groupId = "")
        {
            SetAuth();
            var url = $"api/usuarios?q={Uri.EscapeDataString(q)}&parishId={parishId}&groupId={groupId}";
            var r = await http.GetFromJsonAsync<List<UsuarioDto>>(url, JsonOpts);
            return r ?? [];
        }

        public async Task<UsuarioDto?> GetUserByIdAsync(uint id)
        {
            SetAuth();
            return await http.GetFromJsonAsync<UsuarioDto>($"api/usuarios/{id}", JsonOpts);
        }

        // ── Perfil propio + contraseña ───────────────────────
        public async Task<PerfilDto?> GetMyProfileAsync()
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<PerfilDto>("api/usuarios/perfil", JsonOpts); }
            catch { return null; }
        }

        public async Task<PerfilDto?> UpdateMyProfileAsync(ActualizarPerfilRequest req)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync("api/usuarios/perfil", req);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<PerfilDto>(JsonOpts) : null;
        }

        /// <summary>Cambia la contraseña propia. Devuelve null si OK, o el mensaje de error.</summary>
        public async Task<string?> ChangeMyPasswordAsync(CambiarPasswordRequest req)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync("api/usuarios/password", req);
            if (r.IsSuccessStatusCode) return null;
            try
            {
                var err = await r.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOpts);
                return err != null && err.TryGetValue("message", out var m) ? m : "No se pudo cambiar la contraseña.";
            }
            catch { return "No se pudo cambiar la contraseña."; }
        }

        public async Task<UsuarioDto?> CreateUserAsync(CrearUsuarioForm form)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync("api/usuarios", form);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<UsuarioDto>(JsonOpts) : null;
        }

        public async Task<UsuarioDto?> UpdateUserAsync(uint id, CrearUsuarioForm form)
        {
            SetAuth();
            var r = await http.PutAsJsonAsync($"api/usuarios/{id}", form);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<UsuarioDto>(JsonOpts) : null;
        }

        // ── Permisos ─────────────────────────────────────────
        public async Task<List<ModuloPermisoDetalle>> GetModulesByUserAsync(uint userId)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<ModuloPermisoDetalle>>($"api/permisos/usuarios/{userId}/modulos", JsonOpts);
            return r ?? [];
        }

        public async Task<List<ModuloInfo>> GetAllModulesAsync()
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<ModuloInfo>>("api/permisos/roles", JsonOpts);
            // Reutilizamos el endpoint de módulos via permisos — o usamos configuración
            var temas = await http.GetFromJsonAsync<List<ModuloPermisoDetalle>>("api/permisos/usuarios/0/modulos", JsonOpts);
            return (temas ?? []).Select(m => new ModuloInfo
            {
                ModuleId = m.ModuleId, Code = m.Code, Name = m.Name, Icon = m.Icon, Route = m.Route
            }).ToList();
        }

        public async Task<List<RolDto>> GetRolesAsync()
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<RolDto>>("api/permisos/roles", JsonOpts);
            return r ?? [];
        }

        public async Task<List<UsuarioDto>> GetUsersByModuleAsync(uint moduleId)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<UsuarioDto>>($"api/permisos/modulos/{moduleId}/usuarios", JsonOpts);
            return r ?? [];
        }

        public async Task<bool> AssignModulesToUserAsync(uint userId, List<uint> moduleIds, PermisosFlags flags)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync($"api/permisos/usuarios/{userId}/modulos", new
            {
                moduleIds,
                permissions = new { view = flags.View, create = flags.Create, edit = flags.Edit, delete = flags.Delete }
            });
            return r.IsSuccessStatusCode;
        }

        // ── Asignación de módulos con vigencia ───────────────
        public async Task<List<ModuloBusquedaDto>> SearchModulesAsync(string q)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<ModuloBusquedaDto>>($"api/permisos/modulos/buscar?q={Uri.EscapeDataString(q)}", JsonOpts);
            return r ?? [];
        }

        public async Task<List<AccesoUsuarioDto>> GetUserAccessAsync(uint userId)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<AccesoUsuarioDto>>($"api/permisos/usuarios/{userId}/accesos", JsonOpts);
            return r ?? [];
        }

        public async Task<bool> AssignModuleWithValidityAsync(uint userId, uint moduleId, PermisosFlags flags, string? validFrom, string? validTo)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync($"api/permisos/usuarios/{userId}/accesos", new
            {
                moduleId,
                permissions = new { view = flags.View, create = flags.Create, edit = flags.Edit, delete = flags.Delete },
                validFrom,
                validTo
            });
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> RevokeModuleAsync(uint userId, uint moduleId)
        {
            SetAuth();
            var r = await http.DeleteAsync($"api/permisos/usuarios/{userId}/accesos/{moduleId}");
            return r.IsSuccessStatusCode;
        }

        public async Task<List<UsuarioAccesoDto>> GetUsersWithAccessAsync(uint moduleId)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<UsuarioAccesoDto>>($"api/permisos/modulos/{moduleId}/accesos", JsonOpts);
            return r ?? [];
        }

        // ── Grupos ───────────────────────────────────────────
        public async Task<List<GrupoDto>> GetGroupsAsync()
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<GrupoDto>>("api/grupos", JsonOpts);
            return r ?? [];
        }

        public async Task<List<GrupoMiembroDto>> GetGroupMembersAsync(uint groupId)
        {
            SetAuth();
            var r = await http.GetFromJsonAsync<List<GrupoMiembroDto>>($"api/grupos/{groupId}/miembros", JsonOpts);
            return r ?? [];
        }

        public async Task<GrupoDto?> CreateGroupAsync(object body)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync("api/grupos", body);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<GrupoDto>(JsonOpts) : null;
        }

        public async Task<bool> AddGroupMemberAsync(uint groupId, uint userId, string roleInGroup)
        {
            SetAuth();
            var r = await http.PostAsJsonAsync($"api/grupos/{groupId}/miembros", new { userId, roleInGroup });
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveGroupMemberAsync(uint groupId, uint userId)
        {
            SetAuth();
            var r = await http.DeleteAsync($"api/grupos/{groupId}/miembros/{userId}");
            return r.IsSuccessStatusCode;
        }

        // ── Genérico helper ──────────────────────────────────
        private async Task<List<T>> GetListAsync<T>(string url)
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<List<T>>(url, JsonOpts) ?? []; }
            catch { return []; }
        }

        private async Task<TResult?> PostAsync<TResult>(string url, object body) where TResult : class
        {
            SetAuth();
            var r = await http.PostAsJsonAsync(url, body);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<TResult>(JsonOpts) : null;
        }

        private async Task<TResult?> PutAsync<TResult>(string url, object body) where TResult : class
        {
            SetAuth();
            var r = await http.PutAsJsonAsync(url, body);
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<TResult>(JsonOpts) : null;
        }

        private async Task<bool> DeleteAsync(string url)
        {
            SetAuth();
            var r = await http.DeleteAsync(url);
            return r.IsSuccessStatusCode;
        }

        // ── Noticias ─────────────────────────────────────────
        public Task<List<NoticiaDto>> GetNewsListAsync(string q = "")
            => GetListAsync<NoticiaDto>($"api/noticias?q={Uri.EscapeDataString(q)}");
        public Task<List<CategoriaDto>> GetNewsCategoriesAsync()
            => GetListAsync<CategoriaDto>("api/noticias/categorias");
        public Task<NoticiaDto?> CreateNewsAsync(NoticiaForm f) => PostAsync<NoticiaDto>("api/noticias", f);
        public Task<NoticiaDto?> UpdateNewsAsync(uint id, NoticiaForm f) => PutAsync<NoticiaDto>($"api/noticias/{id}", f);
        public Task<bool> DeleteNewsAsync(uint id) => DeleteAsync($"api/noticias/{id}");

        // ── Calendario ───────────────────────────────────────
        public Task<List<CalendarioDto>> GetCalendarAsync(string type = "")
            => GetListAsync<CalendarioDto>($"api/calendario?type={type}");
        public Task<CalendarioDto?> CreateCalendarAsync(object f) => PostAsync<CalendarioDto>("api/calendario", f);
        public Task<CalendarioDto?> UpdateCalendarAsync(uint id, object f) => PutAsync<CalendarioDto>($"api/calendario/{id}", f);
        public Task<bool> DeleteCalendarAsync(uint id) => DeleteAsync($"api/calendario/{id}");

        // ── Sacramentos ──────────────────────────────────────
        public Task<List<SacramentoDto>> GetSacramentsAsync()
            => GetListAsync<SacramentoDto>("api/sacramentos");
        public Task<List<CategoriaDto>> GetSacramentTypesAsync()
            => GetListAsync<CategoriaDto>("api/sacramentos/tipos");
        public Task<SacramentoDto?> CreateSacramentAsync(object f) => PostAsync<SacramentoDto>("api/sacramentos", f);
        public Task<bool> DeleteSacramentAsync(uint id) => DeleteAsync($"api/sacramentos/{id}");

        // ── Donaciones ───────────────────────────────────────
        public Task<List<DonacionDto>> GetDonationsAsync()
            => GetListAsync<DonacionDto>("api/donaciones");
        public async Task<DonacionResumen?> GetDonationSummaryAsync()
        {
            SetAuth();
            try { return await http.GetFromJsonAsync<DonacionResumen>("api/donaciones/resumen", JsonOpts); }
            catch { return null; }
        }
        public Task<DonacionDto?> CreateDonationAsync(object f) => PostAsync<DonacionDto>("api/donaciones", f);
        public Task<bool> DeleteDonationAsync(uint id) => DeleteAsync($"api/donaciones/{id}");

        // ── Intenciones de Misa ──────────────────────────────
        public Task<List<IntencionDto>> GetIntentionsAsync()
            => GetListAsync<IntencionDto>("api/intenciones");
        public Task<IntencionDto?> CreateIntentionAsync(object f) => PostAsync<IntencionDto>("api/intenciones", f);
        public Task<IntencionDto?> UpdateIntentionAsync(uint id, object f) => PutAsync<IntencionDto>($"api/intenciones/{id}", f);
        public Task<bool> DeleteIntentionAsync(uint id) => DeleteAsync($"api/intenciones/{id}");

        // ── Solicitudes ──────────────────────────────────────
        public Task<List<SolicitudDto>> GetRequestsAsync()
            => GetListAsync<SolicitudDto>("api/solicitudes");
        public Task<List<CategoriaDto>> GetRequestTypesAsync()
            => GetListAsync<CategoriaDto>("api/solicitudes/tipos");
        public Task<SolicitudDto?> CreateRequestAsync(object f) => PostAsync<SolicitudDto>("api/solicitudes", f);
        public Task<SolicitudDto?> UpdateRequestAsync(uint id, object f) => PutAsync<SolicitudDto>($"api/solicitudes/{id}", f);
        public Task<bool> DeleteRequestAsync(uint id) => DeleteAsync($"api/solicitudes/{id}");

        // ── Oraciones ────────────────────────────────────────
        public Task<List<OracionDto>> GetPrayersAsync()
        {
            SetAuth();
            return GetListAsync<OracionDto>("api/oraciones");
        }
    }
}
