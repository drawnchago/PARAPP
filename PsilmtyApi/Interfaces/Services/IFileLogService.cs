namespace PsilmtyApi.Interfaces.Services;

public interface IFileLogService
{
    /// <summary>Escribe el contenido en Log/yyyy/MM/dd/{success|error}/Log_yyyyMMdd.txt</summary>
    void Write(bool success, string content);
}
