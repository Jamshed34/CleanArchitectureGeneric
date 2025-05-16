using Application.Commands;
using Application.Models;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiScope")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMemoryCache _cache;
        private static CancellationTokenSource _evictionTokenSource = new();

        public UserController(IMediator mediator, IMemoryCache cache)
        {
            _mediator = mediator;
            _cache = cache;
        }

        // GET api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            string cacheKey = $"user_{id}";

            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.AddExpirationToken(new CancellationChangeToken(_evictionTokenSource.Token));
                return await _mediator.Send(new GetUserByIdQuery(id));
            });

            return Ok(result);
        }

        // POST api/user
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await _mediator.Send(command);
            InvalidateCache();
            return Ok(result);
        }

        private void InvalidateCache()
        {
            _evictionTokenSource.Cancel();
            _evictionTokenSource.Dispose();
            _evictionTokenSource = new CancellationTokenSource(); // Renew token for next cache cycle
        }
    }
}
