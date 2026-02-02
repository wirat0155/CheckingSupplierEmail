using CheckingSupplierEmail.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Controllers
{
    public class MonitorController : BaseController
    {
        private readonly POLogRepository _poLogRepository;

        public MonitorController(POLogRepository poLogRepository)
        {
            _poLogRepository = poLogRepository;
        }

        public async Task<IActionResult> vIndex(DateTime? startDate, DateTime? endDate, string status)
        {
            // Set defaults for View
            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-7);
            if (!endDate.HasValue) endDate = DateTime.Today;
            if (string.IsNullOrEmpty(status)) status = "S";

            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            var logs = await _poLogRepository.GetPOLogs(startDate, endDate, status);
            return View(logs);
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
