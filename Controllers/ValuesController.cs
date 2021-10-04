using JWT_Calisma.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWT_Calisma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        AppDbContext context;
        UserManager<Kullanici> userManager;

        public ValuesController(AppDbContext context,UserManager<Kullanici> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [Route("removeCard")]
        public async Task<IActionResult> RemoveBasket(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user!=null)
            {
                Sepet card = context.Tbl_Card.Where(c => c.UserId == userId).Include(c => c.ProductCard).First();
                context.Tbl_ProductCard.RemoveRange(card.ProductCard);
                context.SaveChanges();
                return Ok(new string[]
                {
                    "The deletion was successfull!"
                });
            }
            else
            {
                return BadRequest(new string[]
                {
                    "User not found!"
                });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("getUserCard")]
        public async Task<IActionResult> GetUserCard(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user!=null)
            {
                Sepet card = context.Tbl_Card.Where(c => c.UserId == userId).Include(c=>c.ProductCard).First();
                if (card.ProductCard.Count()>0)
                {
                    List<Product> productList = new List<Product>();
                    foreach (var item in card.ProductCard)
                    {
                        productList.Add(context.Tbl_Product.FirstOrDefault(c => c.Id == item.ProductId));
                    }
                    return Ok(productList);
                }
                else
                {
                    return BadRequest(new string[] 
                    { 
                        "Cart is empty!"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "User not found!"
                });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("addToBasket")]
        public async Task<IActionResult> AddToBasket([FromBody]ProductCardDto productList)
        {
            var user = await userManager.FindByIdAsync(productList.UserId);
            var card = context.Tbl_Card.Where(c=>c.UserId == user.Id).First();
            if (user != null)
            {
                try
                {
                    foreach (var item in productList.ProductCard)
                    {
                        context.Tbl_ProductCard.Add(new ProductCard
                        {
                            CardId = card.Id,
                            ProductId = item.ProductId
                        });
                        context.SaveChanges();
                    }

                    return Ok(new string[]
                    {
                        "Adding was successfull!"
                    });
                }
                catch (Exception exception)
                {
                    return BadRequest(new string[]
                    {
                        "Error :"+exception.ToString()
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "User not found!"
                });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("products")]
        public IActionResult GetProducts()
        {
            return Ok(context.Tbl_Product.Where(c=>c.UrunAdi!="AdminProduct").ToList());
        }

        [HttpGet]
        [Route("productForAdmin")]
        [Authorize(Roles ="admin")]
        public IActionResult ProductForAdmin()
        {
            return Ok(context.Tbl_Product.Where(c => c.UrunAdi == "AdminProduct"));
        }

        [HttpGet]
        [Route("complexAuthForProduct")]
        [Authorize(Roles ="admin,Normal")]
        public IActionResult ComplexAuthForProduct()
        {
            return Ok(context.Tbl_Product.ToList());
        }
    }
}
