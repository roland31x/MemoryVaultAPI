﻿using MemoryVaultAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        [Route("runtest")]
        public IActionResult RunTest()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Account toCreate = new Account
                    {
                        Username = "user" + i,
                        Email = "u" + i + "@fake_gmail.com",
                        Password = HashPassword("password" + i),
                    };
                    _ctx.Accounts.Add(toCreate);
                }
                _ctx.SaveChanges();
                return new OkObjectResult(new PHPResponse(200, null, "Test Successful!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        private DateTime RandomDate()
        {
            int range = (DateTime.Today - new DateTime(2020, 1, 1)).Days;
            return new DateTime(2000, 1, 1).AddDays(new Random().Next(range));
        }

        private string HashPassword(string password)
        {
            string hashed = BCrypt.Net.BCrypt.HashPassword(password);
            return hashed;
        }
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        [HttpGet]
        [Route("profile/get/{id}")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult GetProfile([FromRoute] int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            try
            {
                Account account = _ctx.Accounts.Include(a => a.Memories).ThenInclude(m => m.Likes).First(x => x.AccountID == id);
                if (account == null)
                    return new ObjectResult(new PHPResponse(400, null, "Account not found!"));

                AccProfile profile = new AccProfile(account, userId);

                return new OkObjectResult(new PHPResponse(200, profile, "Fetched account!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpGet]
        [Route("admin/get")]
        [Authorize(Policy = "AdminRights")]
        public IActionResult GetAccounts()
        {
            try
            {
                List<Account> accounts = _ctx.Accounts.ToList();
                return new OkObjectResult(new PHPResponse(200, accounts, "Fetched all accounts!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }

        [HttpPost]
        [Route("edit")]
        [Authorize(Policy = "LoginTokens")]
        public IActionResult EditAccount(AccountRegistration Account)
        {
            try
            {
                Account user = _ctx.Accounts.Find(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                if (Account.Password.Length < 4)
                    return new ObjectResult(new PHPResponse(400, null, "Password must be at least 4 characters long!"));
                if (Account.Username.Length < 4)
                    return new ObjectResult(new PHPResponse(400, null, "Username must be at least 4 characters long!"));
                if (!Account.Email.Contains('@'))
                    return new ObjectResult(new PHPResponse(400, null, "Invalid Email!"));

                List<Account> accounts = _ctx.Accounts.Where(x => x != user).ToList();
                foreach (Account a in accounts)
                {
                    if (a.Username == Account.Username)
                        return new ObjectResult(new PHPResponse(400, null, "Username already exists!"));
                    if (a.Email == Account.Email)
                        return new ObjectResult(new PHPResponse(400, null, "Email already exists!"));
                }

                user.Username = Account.Username;
                user.Email = Account.Email;
                user.Password = HashPassword(Account.Password);

                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, null, "Account updated!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
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
                if (Account.Username.Length > 16)
                    return new ObjectResult(new PHPResponse(400, null, "Username must be at most 16 characters long!"));
                if (!Account.Email.Contains('@'))
                    return new ObjectResult(new PHPResponse(400, null, "Invalid Email!"));


                Account toCreate = new Account
                {
                    Username = Account.Username,
                    Email = Account.Email,
                    Password = HashPassword(Account.Password),
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
                Account? account = _ctx.Accounts.Where(a => a.Username.ToLower() == login.Credential.ToLower() || a.Email.ToLower() == login.Credential.ToLower()).FirstOrDefault();
                if(account == null)
                    return new ObjectResult(new PHPResponse(400, null, "Account not found!"));

                if (!VerifyPassword(login.Password, account.Password))
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
            try
            {
                Account user = _ctx.Accounts.Where(a => a.AccountID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)).FirstOrDefault()!;
                return new OkObjectResult(new PHPResponse(200, user, "Token is valid!"));
            }
            catch(Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
        }

        [HttpDelete]
        [Route("admin/delete/{id}")]
        [Authorize(Policy = "AdminRights")]
        public IActionResult DeleteAccount([FromRoute] int id)
        {
            try
            {
                Account user = _ctx.Accounts.Find(id);
                if (user == null)
                    return new ObjectResult(new PHPResponse(400, null, "User not found!"));

                _ctx.Accounts.Remove(user);
                _ctx.SaveChanges();

                return new OkObjectResult(new PHPResponse(200, null, "Account deleted!"));
            }
            catch (Exception e)
            {
                return new ObjectResult(new PHPResponse(500, null, e.Message));
            }
            
        }
    }
}
