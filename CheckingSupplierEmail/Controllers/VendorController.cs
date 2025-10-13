using CheckingSupplierEmail.Data;
using CheckingSupplierEmail.Models.DbModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly ERPDbContext _context;

        public VendorController(ERPDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> vIndex()
        {
            var ls_vendor = await _context.VEN.ToListAsync();
            List<VEN> ls_invalid_vendor = new List<VEN>();
            bool isValidEmail;

            foreach (var obj_vendor in ls_vendor)
            {
                isValidEmail = true;
                obj_vendor.Reason = null; // Clear previous reason

                if (string.IsNullOrEmpty(obj_vendor.VEN_POEmail))
                {
                    isValidEmail = false;
                    obj_vendor.Reason = "ไม่ได้กำหนดอีเมล";
                }
                else
                {
                    string[] emails = obj_vendor.VEN_POEmail.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    if (emails.Length == 0) // Case for " ; " or other empty string after split
                    {
                        isValidEmail = false;
                        obj_vendor.Reason = "ไม่มีอีเมลที่ถูกต้องถูกกำหนดไว้";
                    }
                    else
                    {
                        // Regex for basic validation, can be more complex for a stricter check
                        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                        foreach (var email in emails)
                        {
                            string trimmedEmail = email.Trim();
                            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedEmail, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                isValidEmail = false;
                                obj_vendor.Reason = $"อีเมล '{trimmedEmail}' มีรูปแบบไม่ถูกต้อง";
                                break; // Exit the inner loop as soon as an invalid email is found
                            }
                        }
                    }
                }

                if (!isValidEmail)
                {
                    ls_invalid_vendor.Add(obj_vendor);
                }
            }
            return View(ls_invalid_vendor);
        }
    }
}
