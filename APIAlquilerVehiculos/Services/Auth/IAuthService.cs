using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Auth;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace APIAlquilerVehiculos.Services;

public interface IAuthService
{
    Task<Result<UsuarioResponseDto, ProblemDetails>> SignupAsync(SignupDto request);
    Task<Result<AuthResponseDto, ProblemDetails>> LoginAsync(LoginDto request);
    Task<Result<UsuarioResponseDto, ProblemDetails>> GetCurrentUserAsync(ClaimsPrincipal principal);
}