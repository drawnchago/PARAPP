using PsilmtyApp.Models;

namespace PsilmtyApp.Services
{
    public class SessionService(SessionStore store)
    {
        public LoginResponse? Session { get; private set; }
        public bool IsAuthenticated => Session is not null && Session.Expiracion > DateTime.UtcNow;
        public event Action? OnChange;

        public void SetSession(LoginResponse response)
        {
            Session = response;
            NotifyChange();
        }

        public void Logout()
        {
            Session = null;
            store.Clear();   // olvidar la sesión recordada
            NotifyChange();
        }

        public bool TieneAcceso(string moduloClave) =>
            Session?.Modulos.Any(m => m.Clave == moduloClave && m.PuedeVer) ?? false;

        public ModuloPermiso? ObtenerPermiso(string clave) =>
            Session?.Modulos.FirstOrDefault(m => m.Clave == clave);

        private void NotifyChange() => OnChange?.Invoke();
    }
}
