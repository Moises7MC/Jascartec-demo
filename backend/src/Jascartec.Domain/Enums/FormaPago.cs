namespace Jascartec.Domain.Enums;

public enum FormaPago
{
    Contado,
    Credito // se guarda en BD como "Crédito"; ver configuración de EF (HasConversion)
}
