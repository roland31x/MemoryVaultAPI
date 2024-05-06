using MemoryVaultAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MemoryVaultAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _ctx;
        private readonly IConfiguration _config;
        public AccountController(DataContext _ctx, IConfiguration _config)
        {
            this._ctx = _ctx;
            this._config = _config;
        }
        [Route("register")]
        [HttpPost]
        public IActionResult CreateAccount(AccountRegistration Account)
        {
            try
            {
                if(Account.Password.Length < 4)
                    return new ObjectResult(new PHPResponse(400, null, "Password must be at least 4 characters long!"));
                if (Account.Username.Length < 4)
                    return new ObjectResult(new PHPResponse(400, null, "Username must be at least 4 characters long!"));
                if (!Account.Email.Contains('@'))
                    return new ObjectResult(new PHPResponse(400, null, "Invalid Email!"));


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
                        return new ObjectResult(new PHPResponse(400, null, "Username already exists!"));
                    if(a.Email == toCreate.Email)
                        return new ObjectResult(new PHPResponse(400, null, "Email already exists!"));
                }

                _ctx.Accounts.Add(toCreate);
                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, null, "Registration Successful!"));
            }
            catch(Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login(LoginRequest login)
        {
            try
            {
                Account? account = _ctx.Accounts.Where(a => a.Username == login.Credential || a.Email == login.Credential).FirstOrDefault();
                if(account == null)
                    return new ObjectResult(new PHPResponse(400, null, "Account not found!"));

                if(account.Password != login.Password)
                    return new ObjectResult(new PHPResponse(400, null, "Invalid Password!"));

                string token = JWTToken.GenerateToken(account, _config);

                return new OkObjectResult(new PHPResponse(200, new { token = token }, "Login Successful!"));
            }
            catch(Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
        }

        [Route("lgtoken")]
        [HttpGet]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult LoginToken()
        {
            Account user = _ctx.Accounts.Where(a => a.AccountID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)).FirstOrDefault()!;
            return new OkObjectResult(new PHPResponse(200, user, "Token is valid!"));
        }
    }
}
