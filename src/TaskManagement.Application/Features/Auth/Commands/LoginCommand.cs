using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Auth.DTOs;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public LoginCommandHandler(
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

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("", "Invalid email or password")
                });
            }

            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("", "Invalid email or password")
                });
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user with refresh token and last login
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiry(accessToken),
                User = _mapper.Map<UserDto>(user)
            };
        }
    }
}
