using AutoMapper;
using QAStudio.Application.Environments.DTOs;
using QAStudio.Application.Environments.Interfaces;
using QAStudio.Application.Common.Exceptions;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;

namespace QAStudio.Infrastructure.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public EnvironmentService(
        IEnvironmentRepository environmentRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _environmentRepository = environmentRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EnvironmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        var environments = await _environmentRepository.GetAllAsync(ct);
        return _mapper.Map<List<EnvironmentDto>>(environments);
    }

    public async Task<EnvironmentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Environment", id);

        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid createdById, CancellationToken ct = default)
    {
        if (await _environmentRepository.ExistsByNameAsync(dto.Name, ct: ct))
        {
            throw new BadRequestException($"An environment with name '{dto.Name}' already exists.");
        }

        var user = await _userRepository.GetByIdAsync(createdById, ct)
            ?? throw new NotFoundException("User", createdById);

        var environment = _mapper.Map<TargetEnvironment>(dto);
        environment.CreatedBy = createdById;

        var created = await _environmentRepository.AddAsync(environment, ct);
        return _mapper.Map<EnvironmentDto>(created);
    }

    public async Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto dto, CancellationToken ct = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Environment", id);

        if (await _environmentRepository.ExistsByNameAsync(dto.Name, id, ct))
        {
            throw new BadRequestException($"An environment with name '{dto.Name}' already exists.");
        }

        _mapper.Map(dto, environment);
        environment.BaseUrl = dto.BaseUrl.TrimEnd('/');

        await _environmentRepository.UpdateAsync(environment, ct);
        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!await _environmentRepository.ExistsAsync(id, ct))
        {
            throw new NotFoundException("Environment", id);
        }

        await _environmentRepository.DeleteAsync(id, ct);
    }
}
