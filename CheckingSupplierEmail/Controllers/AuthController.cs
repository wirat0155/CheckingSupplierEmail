using CheckingSupplierEmail.Repositories;
using JWTRegen.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Controllers
{
    public class AuthController : BaseController
    {
        private readonly EmployeeRepository _emp;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            EmployeeRepository emp,
            IJwtTokenService jwtTokenService)
        {
            _emp = emp;
            _jwtTokenService = jwtTokenService;
        }
        public IActionResult vLogin()
        {
            return View();
        }

        public class LoginVM
        {
            public string txt_empno { get; set; }
            public string txt_password { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            try
            {
                await _emp.Login(model.txt_empno, model.txt_password);
                var token = _jwtTokenService.GenerateToken(model.txt_empno, "user");

                Response.Cookies.Append("purvenportal_jwt", token, new CookieOptions
                {
                    HttpOnly = false,
                    //Secure = true, disable when use http
                    SameSite = SameSiteMode.Strict,
                    Path = "/", // Set cookie available across the entire site
                    Expires = DateTime.UtcNow.AddHours(24)
                });
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, text = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Append("purvenportal_jwt", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Path = "/", // Set cookie available across the entire site
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return RedirectToAction(nameof(vLogin));
        }
    }
}
