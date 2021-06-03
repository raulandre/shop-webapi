using Microsoft.AspNetCore.Mvc;
using Shop.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shop.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Shop.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Controllers
{
    [Route("v1/users")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get(
            [FromServices] DataContext context
        )
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post(
            [FromServices] DataContext context,
            [FromBody] User model
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Users.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar o usuário. " });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put(
            int id,
            [FromBody] User model,
            [FromServices] DataContext context
        )
        {
            if (id != model.Id)
                return NotFound(new { message = "Usuário não encontrado. " });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<User>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível alterar esse usuário. " });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound(new { message = $"Não foi possível encontrar o usuário de id {id}. " });

            try
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível deletar esse usuário. " });
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromServices] DataContext context,
            [FromBody] User model
        )
        {
            var user = await context.Users.AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválidos. " });

            var token = TokenService.GenerateToken(user);
            return new
            {
                user = user,
                token = token
            };
        }
    }
}