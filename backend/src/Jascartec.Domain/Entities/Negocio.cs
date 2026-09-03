namespace Jascartec.Domain.Entities;

/// <summary>Datos del negocio, fila única usada para imprimir boletas.</summary>
public class Negocio
{
    public short Id { get; set; } = 1;
    public string RazonSocial { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Web { get; set; }
}
