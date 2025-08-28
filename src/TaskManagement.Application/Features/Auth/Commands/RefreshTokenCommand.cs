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
    public class RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var email = _tokenService.GetEmailFromToken(request.AccessToken);
            if (string.IsNullOrEmpty(email))
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("", "Invalid token")
                });
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("", "Invalid token")
                });
            }

            if (user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("", "Invalid refresh token")
                });
            }

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user with new refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
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
