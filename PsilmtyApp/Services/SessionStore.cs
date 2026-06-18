using PsilmtyApp.Models;
using System.Text.Json;

namespace PsilmtyApp.Services
{
    /// <summary>
    /// Persiste la sesión de forma segura (SecureStorage) para "Recordar mi sesión".
    /// </summary>
    public class SessionStore
    {
        private const string Key = "psilmty_session";
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task SaveAsync(LoginResponse session)
        {
            try
            {
                var json = JsonSerializer.Serialize(session, JsonOpts);
                await SecureStorage.Default.SetAsync(Key, json);
            }
            catch { /* almacenamiento no disponible: ignorar */ }
        }

        public async Task<LoginResponse?> LoadAsync()
        {
            try
            {
                var json = await SecureStorage.Default.GetAsync(Key);
                if (string.IsNullOrEmpty(json)) return null;
                return JsonSerializer.Deserialize<LoginResponse>(json, JsonOpts);
            }
            catch { return null; }
        }

        public void Clear()
        {
            try { SecureStorage.Default.Remove(Key); } catch { }
        }
    }
}
