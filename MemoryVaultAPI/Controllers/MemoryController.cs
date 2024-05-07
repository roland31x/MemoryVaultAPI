using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MemoryVaultAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MemoryVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly DataContext _ctx;
        public MemoryController(DataContext _ctx)
        {
            this._ctx = _ctx;
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult DeleteMemory([FromRoute] int id)
        {

            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                Memory toDelete = _ctx.Memories.Find(id);
                if (toDelete == null)
                    return new ObjectResult(new PHPResponse(400, null, "Memory not found!"));

                if (!(toDelete.Owner == user || user.IsAdmin))
                    return new ObjectResult(new PHPResponse(400, null, "Memory does not belong to this account!"));

                _ctx.Memories.Remove(toDelete);
                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, null, "Memory deleted!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpGet]
        [Route("get")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult GetMemories()
        {

            try
            {
                Account user = _ctx.Accounts.Include(m => m.Memories).First(x => x.AccountID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                return new OkObjectResult(new PHPResponse(200, user.Memories.ToList(), "Fetched all memories for this account!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
           
        }

        [HttpGet]
        [Route("get/{id}")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult GetMemory([FromRoute] int id)
        {

            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                Memory memory = _ctx.Memories.Include(m => m.Owner).First(x => x.MemoryID == id);
                if (memory == null)
                    return new ObjectResult(new PHPResponse(400, null, "Memory not found!"));

                if (!(memory.Owner == user || memory.Public || user.IsAdmin))
                    return new ObjectResult(new PHPResponse(400, null, "Memory does not belong to this account!"));

                return new OkObjectResult(new PHPResponse(200, memory, "Fetched memory!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpPost]
        [Route("edit/{id}")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult EditMemory([FromRoute] int id, MemoryEntry memory)
        {

            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                Memory toEdit = _ctx.Memories.Find(id);
                if (toEdit == null)
                    return new ObjectResult(new PHPResponse(400, null, "Memory not found!"));

                if (toEdit.Owner != user)
                    return new ObjectResult(new PHPResponse(400, null, "Memory does not belong to this account!"));

                toEdit.Name = memory.Name;
                toEdit.Description = memory.Description;
                toEdit.Public = memory.Public == "true" ? true : false;

                //toEdit.Images.Clear();
                //foreach (byte[] img in memory.Images)
                //{
                //    toEdit.Images.Add(new Image(img));
                //}

                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, toEdit, "Memory edited!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult CreateMemory(MemoryEntry memory)
        {

            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                Memory toCreate = new Memory();

                toCreate.Name = memory.Name;
                toCreate.Description = memory.Description;
                toCreate.Public = memory.Public == "true" ? true : false;
                toCreate.Owner = user;
                toCreate.PostDate = DateTime.Now;
                foreach (byte[] img in memory.Images)
                {
                    toCreate.Images.Add(new Image(img));
                }

                _ctx.Memories.Add(toCreate);
                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, toCreate, "Memory created!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpGet]
        [Route("admin/get")]
        [Authorize(Policy = "AdminRights")]
        public IActionResult GetMemoriesAdmin()
        {

            try
            {
                return new OkObjectResult(new PHPResponse(200, _ctx.Memories.Include(m => m.Owner).ToList(), "Fetched all memories as admin!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }
    }
}