using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.DTOs;
using Products.Models;

namespace Products.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var user = await GetUser();
            return await _context.Products.Where(p => p.User == user).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var user = await GetUser();
            var product = await _context.Products.Where(p => p.User == user && p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductUpdateDTO product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            var user = await GetUser();
            var productDb = await _context.Products.Where(p => p.User == user && p.Id == id).FirstOrDefaultAsync();
            if (productDb == null) 
            {
                return NotFound();
            }

            productDb.Name = product.Name;
            productDb.Description = product.Description;
            productDb.Price = product.Price;

            _context.Entry(productDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO product)
        {
            var user = await GetUser();
            var productDb = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                User = user
            };
            _context.Products.Add(productDb);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = productDb.Id }, product);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private async Task<User> GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var user = await _context.Users.FindAsync(int.Parse(userId));
            return user ?? throw new Exception("User not found");
        }
    }
}
