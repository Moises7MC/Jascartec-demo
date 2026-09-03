using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiraEn) Generar(Usuario usuario);
}
