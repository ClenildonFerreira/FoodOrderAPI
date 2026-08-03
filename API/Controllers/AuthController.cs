using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Auth.Commands.Login;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace FoodOrderAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new {error = result.Error});

        return Ok(result.Value);
    }
}