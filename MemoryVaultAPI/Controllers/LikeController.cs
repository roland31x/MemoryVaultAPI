using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MemoryVaultAPI.Models;

namespace MemoryVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly DataContext _ctx;
        public LikeController(DataContext ctx)
        {
            this._ctx = ctx;
        }

        [HttpPost]
        [Route("{id}")]
        [Authorize (Policy = "LoginTokens")]
        public IActionResult ToggleLikeMemory([FromRoute] int id)
        {
            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                Memory memory = _ctx.Memories.Find(id);
                if (memory == null)
                    return new ObjectResult(new PHPResponse(400, null, "Memory not found!"));

                var key = new { memory.MemoryID, user.AccountID };
                Like found = _ctx.Likes.Find(memory.MemoryID, user.AccountID);

                if (found != null)
                {
                    _ctx.Likes.Remove(found);
                    _ctx.SaveChanges();
                    return new OkObjectResult(new PHPResponse(200, null, "Memory unliked."));
                }
                else
                {
                    Like like = new Like
                    {
                        Liker = user,
                        Memory = memory
                    };

                    _ctx.Likes.Add(like);
                    _ctx.SaveChanges();
                    return new OkObjectResult(new PHPResponse(200, null, "Memory liked."));
                }       
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
        }
    }
}
