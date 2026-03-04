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
    public class VendorController : BaseController
    {
        private readonly ERPDbContext _context;

        public VendorController(ERPDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> vIndex(string emailFilter = "invalid")
        {
            var ls_vendor = await _context.VEN.Where(e => e.VEN_StatusCode == "Active").ToListAsync();
            List<VEN> ls_valid_vendor = new List<VEN>();
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

                if (isValidEmail)
                {
                    ls_valid_vendor.Add(obj_vendor);
                }
                else
                {
                    ls_invalid_vendor.Add(obj_vendor);
                }
            }

            // Pass filter value to view for maintaining selected state
            ViewBag.EmailFilter = emailFilter;
            ViewBag.ValidCount = ls_valid_vendor.Count;
            ViewBag.InvalidCount = ls_invalid_vendor.Count;
            ViewBag.TotalCount = ls_vendor.Count;

            // Return filtered list based on emailFilter parameter
            List<VEN> result;
            switch (emailFilter?.ToLower())
            {
                case "valid":
                    result = ls_valid_vendor;
                    break;
                case "all":
                    result = ls_vendor;
                    break;
                case "invalid":
                default:
                    result = ls_invalid_vendor;
                    break;
            }

            return View(result);
        }
    }
}
