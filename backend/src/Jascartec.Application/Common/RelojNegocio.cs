namespace Jascartec.Application.Common;

/// <summary>
/// "Hoy" para el negocio, en la zona horaria de Perú (UTC-5, sin horario de
/// verano) — NUNCA usar DateTime.UtcNow directo para calcular una fecha de
/// negocio: de noche en Perú, UTC ya está en el día siguiente, y una venta
/// hecha a las 9pm terminaba fechada "mañana".
/// </summary>
public static class RelojNegocio
{
    private static readonly TimeSpan OffsetPeru = TimeSpan.FromHours(-5);

    public static DateTimeOffset AhoraPeru() => DateTimeOffset.UtcNow.ToOffset(OffsetPeru);
    public static DateOnly HoyPeru() => DateOnly.FromDateTime(AhoraPeru().DateTime);
}
