using StrideGuard.Application.DTOs;
using StrideGuard.Application.Interfaces;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.Application.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUsuarioRepository usuarioRepo, IPasswordHasher hasher, ITokenService tokenService)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegistrarAsync(RegistrarUsuarioRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Senha) || req.Senha.Length < 6)
            throw new DomainException("A senha deve ter no mínimo 6 caracteres.");

        var existente = await _usuarioRepo.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant());
        if (existente != null)
            throw new DomainException("Já existe um usuário cadastrado com este e-mail.");

        var hash = _hasher.Hash(req.Senha);
        var usuario = new Usuario(req.Nome, req.Email, hash);
        await _usuarioRepo.AdicionarAsync(usuario);

        var token = _tokenService.GerarToken(usuario);
        return new AuthResponse(token, usuario.Nome, usuario.Email, usuario.Id);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var usuario = await _usuarioRepo.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant());
        if (usuario == null || !_hasher.Verify(req.Senha, usuario.PasswordHash))
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var token = _tokenService.GerarToken(usuario);
        return new AuthResponse(token, usuario.Nome, usuario.Email, usuario.Id);
    }
}
