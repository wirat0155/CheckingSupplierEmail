using CheckingSupplierEmail.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CheckingSupplierEmail.Controllers
{
    public class MonitorPRController : BaseController
    {
        private readonly MonitorPRRepository _repository;

        public MonitorPRController(MonitorPRRepository repository)
        {
            _repository = repository;
        }

        public IActionResult vIndex(string month)
        {
            ViewData["Month"] = string.IsNullOrEmpty(month) ? DateTime.Now.ToString("yyyy-MM") : month;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var month = Request.Form["month"].FirstOrDefault();
                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                
                var data = await _repository.GetMonitorPRData(skip, pageSize, searchValue, sortColumn, sortColumnDirection, month);
                
                return Json(new { 
                    draw = draw, 
                    recordsFiltered = data.FilteredRecords, 
                    recordsTotal = data.TotalRecords, 
                    data = data.Data 
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdjustAmount(string[] prNumbers)
        {
            try
            {
                if (prNumbers == null || prNumbers.Length == 0)
                {
                    return Json(new { success = false, message = "No PR selected" });
                }

                // Get current user from claims
                // Assuming standard claim mapping. 
                // If the user is logged in via the provided JWT setup, User.Identity.Name should be populated.
                string currentUser = User.Identity?.Name ?? "Unknown";

                await _repository.UpdatePRAmountAsync(prNumbers.ToList(), currentUser);
                return Json(new { success = true, message = "Adjusted approve amount successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
