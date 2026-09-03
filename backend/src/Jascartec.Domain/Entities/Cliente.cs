using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty; // DNI (8) o RUC (11)
    public TipoCliente Tipo { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
