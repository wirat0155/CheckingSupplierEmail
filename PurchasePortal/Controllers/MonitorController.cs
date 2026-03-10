using PurchasePortal.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PurchasePortal.Models.DbViewModels;

namespace PurchasePortal.Controllers
{
    public class MonitorController : BaseController
    {
        private readonly POLogRepository _poLogRepository;

        public MonitorController(POLogRepository poLogRepository)
        {
            _poLogRepository = poLogRepository;
        }

        public IActionResult vIndex()
        {
            // Set defaults for View
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today;
            var status = "S";

            ViewData["StartDate"] = startDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPOLogs(
            DateTime? startDate, 
            DateTime? endDate, 
            string? status,
            int draw = 1,
            int start = 0,
            int length = 10,
            int orderColumn = 1,
            string orderDir = "desc",
            string? searchValue = null)
        {
            var result = await _poLogRepository.GetPOLogsDataTable(
                startDate, endDate, status, start, length, draw, searchValue, orderColumn, orderDir);
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var details = await _poLogRepository.GetPODetails(id);
            return PartialView("_PODetails", details);
        }
    }
}
