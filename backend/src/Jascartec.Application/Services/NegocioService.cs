using Jascartec.Application.Common;
using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public class NegocioService(IUnitOfWork unitOfWork) : INegocioService
{
    public async Task<NegocioDto> ObtenerAsync(CancellationToken ct = default)
    {
        var negocio = await unitOfWork.Negocios.GetAsync(ct)
            ?? throw new NotFoundException("Negocio", 1);
        return new NegocioDto(negocio.RazonSocial, negocio.Ruc, negocio.Direccion, negocio.Telefono, negocio.Email, negocio.Web);
    }

    public async Task<NegocioDto> ActualizarAsync(ActualizarNegocioRequest request, CancellationToken ct = default)
    {
        var negocio = await unitOfWork.Negocios.GetAsync(ct)
            ?? throw new NotFoundException("Negocio", 1);

        negocio.RazonSocial = request.RazonSocial;
        negocio.Ruc = request.Ruc;
        negocio.Direccion = request.Direccion;
        negocio.Telefono = request.Telefono;
        negocio.Email = request.Email;
        negocio.Web = request.Web;

        unitOfWork.Negocios.Update(negocio);
        await unitOfWork.SaveChangesAsync(ct);

        return new NegocioDto(negocio.RazonSocial, negocio.Ruc, negocio.Direccion, negocio.Telefono, negocio.Email, negocio.Web);
    }
}
