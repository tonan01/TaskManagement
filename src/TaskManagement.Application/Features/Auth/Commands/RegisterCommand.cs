using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Auth.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Repositories;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Application.Features.Auth.Commands
{
    public class RegisterCommand : IRequest<AuthResponseDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("Email", "Email is already registered")
                });
            }

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Role = UserRoles.User,
                IsActive = true,
                EmailConfirmed = true // For demo purposes, set to true
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(createdUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user with refresh token
            createdUser.RefreshToken = refreshToken;
            createdUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(createdUser);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiry(accessToken),
                User = _mapper.Map<UserDto>(createdUser)
            };
        }
    }
}
