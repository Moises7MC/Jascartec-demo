namespace Jascartec.Application.Dtos;

public record MovimientoCajaDto(int Id, DateTimeOffset CreadoEn, string Tipo, string Concepto, decimal Monto, string Usuario);

/// <summary>VentasEfectivo ya suma ventas al contado en efectivo + abonos en efectivo (incluye
/// el inicial de créditos) del día de la caja. EfectivoEsperado = MontoInicial + VentasEfectivo +
/// MovimientosIngreso - MovimientosSalida. Diferencia (MontoContadoCierre - EfectivoEsperado) solo
/// viene con valor una vez cerrada la caja: positivo = sobró plata, negativo = faltó.</summary>
public record CajaSesionDto(
    int Id, DateOnly Fecha, string Estado, int SucursalId, string Sucursal,
    string UsuarioApertura, DateTimeOffset AbiertaEn, decimal MontoInicial, string? ObservacionesApertura,
    string? UsuarioCierre, DateTimeOffset? CerradaEn, decimal? MontoContadoCierre, string? ObservacionesCierre,
    decimal VentasEfectivo, decimal MovimientosIngreso, decimal MovimientosSalida, decimal EfectivoEsperado,
    decimal? Diferencia, IReadOnlyList<MovimientoCajaDto> Movimientos);

public record AbrirCajaRequest(int SucursalId, decimal MontoInicial, string? Observaciones = null);
public record CerrarCajaRequest(decimal MontoContadoCierre, string? Observaciones = null);
public record CrearMovimientoCajaRequest(string Tipo, string Concepto, decimal Monto);
