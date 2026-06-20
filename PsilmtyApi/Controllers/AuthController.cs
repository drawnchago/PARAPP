using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IDatabaseRepository repository) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return response is null
            ? Unauthorized(new { message = "Invalid credentials or inactive account." })
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> Register(RegisterRequest request) =>
        Ok(await authService.RegisterAsync(request));

    [AllowAnonymous]
    [HttpGet("parroquias")]
    public async Task<IActionResult> GetPublicParishes() =>
        Ok(await repository.QueryAsync<object>("""
            SELECT p.id Id,p.name Name,p.city City,p.state State,p.logo_url LogoUrl,
                   p.state_id StateId,p.neighborhood_id NeighborhoodId,s.country_id CountryId,
                   NULL PrimaryColor, NULL SecondaryColor
            FROM parishes p
            LEFT JOIN states s ON s.id=p.state_id
            WHERE p.status = 1 ORDER BY p.name
            """));
}
