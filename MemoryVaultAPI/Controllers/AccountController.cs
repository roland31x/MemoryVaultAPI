using MemoryVaultAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace MemoryVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _ctx;
        public AccountController(DataContext _ctx)
        {
            this._ctx = _ctx;
        }
        [Route("register")]
        [HttpPost]
        public IActionResult CreateAccount(AccountRegistration Account)
        {
            try
            {
                Account toCreate = new Account
                {
                    Username = Account.Username,
                    Email = Account.Email,
                    Password = Account.Password
                };

                List<Account> accounts = _ctx.Accounts.ToList();
                foreach(Account a in accounts)
                {
                    if(a.Username == toCreate.Username)
                    {
                        return new BadRequestObjectResult(new PHPResponse(400, null, "Username already exists!"));
                    }
                    if(a.Email == toCreate.Email)
                    {
                        return new BadRequestObjectResult(new PHPResponse(400, null, "Email already exists!"));
                    }
                }

                return new OkObjectResult(new PHPResponse(200, , "Registration Successful!"));
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(new PHPResponse(500, null, e.Message));
            }
        }
    }
}
