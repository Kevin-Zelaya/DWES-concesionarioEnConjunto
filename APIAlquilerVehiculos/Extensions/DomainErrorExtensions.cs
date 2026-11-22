using APIAlquilerVehiculos.Errors.Common;
using Microsoft.AspNetCore.Mvc;

public static class DomainErrorExtensions
{
    public static ActionResult<T> ToHttpResult<T>(this DomainError error) => error switch
{
    NotFoundError => new NotFoundObjectResult(new { message = error.Message }),
    ConflictError => new ConflictObjectResult(new { message = error.Message }),
    UnauthorizedError => new UnauthorizedObjectResult(new { message = error.Message }),
    BusinessRuleError => new UnprocessableEntityObjectResult(new { message = error.Message }),

    DomainError e when e.StatusCode == System.Net.HttpStatusCode.BadRequest =>
        new BadRequestObjectResult(new { message = e.Message }),

    _ => new ObjectResult(new { message = "Error interno del servidor" })
    {
        StatusCode = 500
    }
};
}