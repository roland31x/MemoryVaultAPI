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
                Account user = _ctx.Accounts.Include(m => m.Memories).Include(m => m.Likes).First(x => x.AccountID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                return new OkObjectResult(new PHPResponse(200, user.Memories.Select(x => new MemoryShortImage(x, x.Likes.Where(y => y.LikerID == user.AccountID).Any())), "Fetched all memories for this account!"));
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

                Memory memory = _ctx.Memories.Include(m => m.Owner).Include(m => m.Likes).First(x => x.MemoryID == id);
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
            //int pageNumber = pag.PageNumber;
            //int pageSize = pag.PageSize;
            try
            {
                var pagedMemories = _ctx.Memories
                                        .Include(m => m.Owner)
                                        .Include(m => m.Likes)
                                        //.Skip((pageNumber - 1) * pageSize)
                                        .Select(m => new MemoryShort(m))
                                        .ToList();

                return new OkObjectResult(new PHPResponse(200, pagedMemories, $"Fetched memories as admin!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }  
        }
        [HttpPost]
        [Route("admin/get/pages")]
        [Authorize(Policy = "AdminRights")]
        public IActionResult GetPagesAdmin([FromBody] Pagination pag)
        {
            try
            {
                int pages = _ctx.Memories.Count() / pag.PageSize;
                return new OkObjectResult(new PHPResponse(200, pages, "Fetched all memories as admin!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
        }

        [HttpGet]
        [Route("random")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult GetRandomMemories()
        {

            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                int amount = 5;
                List<int> usedidx = new List<int>();
                List<MemoryShortImage> memories = new List<MemoryShortImage>();
                List<Memory> pub = _ctx.Memories.Include(m => m.Owner).Include(m => m.Likes).Where(x => x.Public).ToList();
                if (pub.Count < amount)
                    amount = pub.Count;
                Random r = new Random();
                for (int i = 0; i < amount; i++)
                {
                    int idx = r.Next(0, pub.Count);
                    while (usedidx.Contains(idx))
                        idx = r.Next(0, pub.Count);
                    usedidx.Add(idx);
                    memories.Add(new MemoryShortImage(pub[idx], pub[idx].Likes.Where(y => y.LikerID == userId).Any()));
                }
                return new OkObjectResult(new PHPResponse(200, memories, "Fetched random memories!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }
    }
}