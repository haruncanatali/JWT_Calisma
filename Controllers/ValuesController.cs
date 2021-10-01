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
    [Authorize]
    public class ValuesController : ControllerBase
    {
        AppDbContext context;
        public ValuesController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult GetProducts()
        {
            return Ok(context.Tbl_Product.ToList());
        }
    }
}
