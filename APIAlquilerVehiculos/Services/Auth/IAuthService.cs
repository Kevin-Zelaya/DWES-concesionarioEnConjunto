using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace APIAlquilerVehiculos.Services;

public interface IAuthService
{
    Task<Result<UsuarioResponseDto, DomainError>> SignupAsync(SignupDto request);
    Task<Result<AuthResponseDto, DomainError>> LoginAsync(LoginDto request);
    Task<Result<UsuarioResponseDto, DomainError>> GetCurrentUserAsync(ClaimsPrincipal principal);
}