namespace Jascartec.Application.Common;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Jascartec.Api";
    public string Audience { get; set; } = "Jascartec.Frontend";
    public int MinutosExpiracion { get; set; } = 480; // 8 horas de turno
}
