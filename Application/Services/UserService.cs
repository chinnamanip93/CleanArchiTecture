using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                       ?? throw new NotFoundException(nameof(User), id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new DomainException($"Email '{dto.Email}' is already registered.");

            var user = _mapper.Map<User>(dto);
            user.Id = Guid.NewGuid();
            user.PasswordHash = _passwordHasher.Hash(dto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.Role = "User";

            await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateAsync(UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id)
                       ?? throw new NotFoundException(nameof(User), dto.Id);

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                       ?? throw new NotFoundException(nameof(User), id);
            await _userRepository.DeleteAsync(id);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                       ?? throw new DomainException("Invalid email or password.");

            if (!user.IsActive)
                throw new DomainException("This account has been deactivated.");

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                throw new DomainException("Invalid email or password.");

            var token = _jwtTokenService.GenerateToken(user);
            var userDto = _mapper.Map<UserDto>(user);

            return new AuthResponseDto(token, "Bearer", _jwtTokenService.ExpiresInSeconds, userDto);
        }
    }
}
