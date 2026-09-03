namespace Jascartec.Application.Common;

/// <summary>La entidad solicitada no existe. El controller la traduce a HTTP 404.</summary>
public class NotFoundException(string entidad, object id)
    : Exception($"{entidad} con id '{id}' no fue encontrado.");

/// <summary>Se violó una regla de negocio (ej. IMEI duplicado, equipo ya vendido). El controller la traduce a HTTP 400/409.</summary>
public class BusinessRuleException(string message) : Exception(message);

/// <summary>Credenciales de login inválidas. El controller la traduce a HTTP 401.</summary>
public class InvalidCredentialsException() : Exception("Usuario o contraseña incorrectos.");
