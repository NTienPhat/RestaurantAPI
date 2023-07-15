using API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<string>> Get()
        {
            return "You are authenticated";
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles =SD.Role_Admin)]
        public async Task<ActionResult<string>> Get(IIntellisenseBuilder someIntValue)
        {
            return "You are authorized with Amin Role";
        }
    }
}
