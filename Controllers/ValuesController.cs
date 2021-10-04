using JWT_Calisma.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ValuesController(AppDbContext context)
        {
            this.context = context;
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
