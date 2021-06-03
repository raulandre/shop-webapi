using Microsoft.AspNetCore.Mvc;
using Shop.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shop.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Controllers
{
    [Route("products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get(
            [FromServices] DataContext context
        )
        {
            var products = await context.Products.Include(x => x.Category)
                .AsNoTracking().ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(
            int id,
            [FromServices] DataContext context
        )
        {
            var product = await context.Products.Include(p => p.Category).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
                return Ok(product);
            else
                return NotFound(new { message = $"O produto de id {id} não foi encontrado. " });
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(
            int id,
            [FromServices] DataContext context
        )
        {
            var products = await context.Products.Include(p => p.Category)
                .AsNoTracking().Where(p => p.categoryId == id).ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post(
            [FromBody] Product product,
            [FromServices] DataContext context
        )
        {
            if (ModelState.IsValid)
            {
                context.Products.Add(product);
                await context.SaveChangesAsync();
                return Ok(product);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Put(
            int id,
            [FromBody] Product product,
            [FromServices] DataContext context
        )
        {
            if (id != product.Id)
                return NotFound(new { message = $"Não foi possível encontrar o produto de id {id}. " });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Product>(product).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(product);
            }
            catch
            {
                return BadRequest(new { message = $"Não foi possível atualizar o produto de id {id}. " });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            try
            {
                var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
                context.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new { message = $"Produto de id {id} removido com sucesso. " });
            }
            catch
            {
                return BadRequest("Não foi possível deletar o produto de id {id}. ");
            }
        }
    }
}