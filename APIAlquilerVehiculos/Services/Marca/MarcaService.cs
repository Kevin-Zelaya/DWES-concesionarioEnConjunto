using System.Net;
using APIAlquilerVehiculos.DTOs.Marcas;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Repositories.Marcas;
using CSharpFunctionalExtensions;
using APIAlquilerVehiculos.Extensions;

namespace APIAlquilerVehiculos.Services.Marcas;

/// <summary>
/// Servicio de Marcas. Se encarga de gestionar la lógica de negocio
/// y comunicarse con el repositorio para realizar las operaciones.
/// </summary>
public class MarcaService(IMarcaRepository repository) : IMarcaService
{
    /// <summary>
    /// Repositorio encargado de acceder a los datos de las marcas.
    /// </summary>
    private readonly IMarcaRepository _repository = repository;

    /// <inheritdoc cref="GetAllAsync"/>
    public async Task<Result<List<MarcaDto>, DomainError>> GetAllAsync()
    {
        // Obtenemos todas las marcas activas desde el repositorio.
        var marcas = await _repository.GetAllAsync();

        // Convertimos las entidades Marca en objetos MarcaDto.
        var marcasDto = marcas.Select(m => new MarcaDto
        {
            Id = m.Id,
            Nombre = m.Nombre,
            Pais = m.Pais,
            Wb = m.Wb
        }).ToList();

        // Devolvemos una operación correcta con la lista de marcas.
        return Result.Success<List<MarcaDto>, DomainError>(marcasDto);
    }

    /// <inheritdoc cref="GetPaginatedMarcasAsync"/>
    public async Task<Result<PaginatedResouce<MarcaDto>, DomainError>>
        GetPaginatedMarcasAsync(int pageNumber, int pageSize)
    {
        // Comprobamos que los parámetros sean válidos.
        if (pageNumber < 1 || pageSize < 1)
            return Result.Failure<
                PaginatedResouce<MarcaDto>, DomainError>(
                new DomainError(
                    "Los parámetros de paginación no son válidos.",
                    HttpStatusCode.BadRequest));

        // Obtenemos las marcas paginadas del repositorio.
        var response = await _repository.GetPaginatedMarcasAsync(
            pageNumber, pageSize);

        // Convertimos las entidades a DTOs.
        var marcasDto = response.items.Select(m => new MarcaDto
        {
            Id = m.Id,
            Nombre = m.Nombre,
            Pais = m.Pais,
            Wb = m.Wb
        }).ToList();

        // Construimos el objeto de paginación.
        var toResponse = new PaginatedResouce<MarcaDto>(
            marcasDto,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage);

        return Result.Success<PaginatedResouce<MarcaDto>, DomainError>(
            toResponse);
    }

    /// <inheritdoc cref="GetByIdAsync"/>
    public async Task<Result<MarcaDto, DomainError>> GetByIdAsync(long id)
    {
        // Buscamos una marca por su identificador.
        var marca = await _repository.GetByIdAsync(id);

        // Si no existe, devolvemos un error 404 (Not Found).
        if (marca == null)
            return Result.Failure<MarcaDto, DomainError>(
                new DomainError(
                    "La marca no existe.",
                    HttpStatusCode.NotFound));

        // Si existe, convertimos la entidad a DTO y devolvemos el resultado.
        return Result.Success<MarcaDto, DomainError>(new MarcaDto
        {
            Id = marca.Id,
            Nombre = marca.Nombre,
            Pais = marca.Pais,
            Wb = marca.Wb
        });
    }

    /// <inheritdoc cref="GetByNombreAsync"/>
    public async Task<Result<MarcaDto, DomainError>> GetByNombreAsync(string nombre)
    {
        // Buscamos una marca por su nombre.
        var marca = await _repository.GetByNombreAsync(nombre);

        // Si no encontramos ninguna marca, devolvemos un error 404.
        if (marca == null)
            return Result.Failure<MarcaDto, DomainError>(
                new DomainError(
                    "La marca no existe.",
                    HttpStatusCode.NotFound));

        // Convertimos la marca encontrada a DTO.
        return Result.Success<MarcaDto, DomainError>(new MarcaDto
        {
            Id = marca.Id,
            Nombre = marca.Nombre,
            Pais = marca.Pais,
            Wb = marca.Wb
        });
    }

    /// <inheritdoc cref="CreateAsync"/>
    public async Task<Result<MarcaDto, DomainError>> CreateAsync(CreateMarcaDto dto)
    {
        // Creamos una nueva entidad Marca a partir de los datos recibidos.
        var marca = new Marca
        {
            Nombre = dto.Nombre,
            Pais = dto.Pais,
            Wb = dto.Wb
        };

        // Enviamos la nueva marca al repositorio para guardarla en la base de datos.
        var creada = await _repository.CreateAsync(marca);

        // Convertimos la marca creada a DTO para devolverla.
        return Result.Success<MarcaDto, DomainError>(new MarcaDto
        {
            Id = creada.Id,
            Nombre = creada.Nombre,
            Pais = creada.Pais,
            Wb = creada.Wb
        });
    }

    /// <inheritdoc cref="UpdateAsync"/>
    public async Task<Result<MarcaDto, DomainError>> UpdateAsync(
        long id,
        UpdateMarcaDto dto)
    {
        // Creamos una entidad con el identificador recibido
        // y los nuevos datos proporcionados por el usuario.
        var marca = new Marca
        {
            Id = id,
            Nombre = dto.Nombre,
            Pais = dto.Pais,
            Wb = dto.Wb
        };

        // Solicitamos al repositorio que actualice la marca.
        var actualizada = await _repository.UpdateAsync(marca);

        // Si no existe una marca activa con ese identificador,
        // devolvemos un error 404 (Not Found).
        if (actualizada == null)
            return Result.Failure<MarcaDto, DomainError>(
                new DomainError(
                    "La marca no existe.",
                    HttpStatusCode.NotFound));

        // Si se ha actualizado correctamente, devolvemos sus datos como DTO.
        return Result.Success<MarcaDto, DomainError>(new MarcaDto
        {
            Id = actualizada.Id,
            Nombre = actualizada.Nombre,
            Pais = actualizada.Pais,
            Wb = actualizada.Wb
        });
    }

    /// <inheritdoc cref="DeleteAsync"/>
    public async Task<Result<bool, DomainError>> DeleteAsync(long id)
    {
        // Solicitamos al repositorio que elimine lógicamente la marca.
        // El repositorio la desactiva, pero no la borra físicamente.
        var eliminada = await _repository.DeleteAsync(id);

        // Si no se ha encontrado la marca, devolvemos un error 404.
        if (!eliminada)
            return Result.Failure<bool, DomainError>(
                new DomainError(
                    "La marca no existe.",
                    HttpStatusCode.NotFound));

        // Si se ha eliminado correctamente, devolvemos true.
        return Result.Success<bool, DomainError>(true);
    }
}