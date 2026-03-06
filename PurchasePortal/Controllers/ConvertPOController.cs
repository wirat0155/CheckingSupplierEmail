using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchasePortal.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PurchasePortal.Controllers
{
    [Authorize]
    public class ConvertPOController : BaseController
    {
        private readonly ConvertPORepository _convertPORepository;
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;

        public ConvertPOController(
            ConvertPORepository convertPORepository,
            JWTRegen.Interfaces.IClaimsHelper claimsHelper)
        {
            _convertPORepository = convertPORepository;
            _claimsHelper = claimsHelper;
        }

        public async Task<IActionResult> vIndex()
        {
            var counts = await _convertPORepository.GetCountsAsync();
            ViewBag.TotalCount = counts.totalCount;
            ViewBag.ConvertedCount = counts.convertedCount;
            ViewBag.NotConvertedCount = counts.notConvertedCount;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCounts()
        {
            try
            {
                var counts = await _convertPORepository.GetCountsAsync();
                return Ok(new
                {
                    totalCount = counts.totalCount,
                    convertedCount = counts.convertedCount,
                    notConvertedCount = counts.notConvertedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDataTable()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var convertpoflagFilter = Request.Form["convertpoflag"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                bool? convertpoflag = null;
                if (!string.IsNullOrEmpty(convertpoflagFilter))
                {
                    convertpoflag = convertpoflagFilter == "1";
                }

                var result = await _convertPORepository.GetPURPOListAsync(
                    skip, pageSize, searchValue, sortColumn, sortDirection, convertpoflag);

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = result.recordsFiltered,
                    recordsTotal = result.recordsTotal,
                    data = result.data
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateArea([FromBody] UpdateAreaRequest request)
        {
            try
            {
                // Validate that area is not GENERAL
                if (string.IsNullOrEmpty(request.Area) || request.Area.ToUpper() == "GENERAL")
                {
                    return BadRequest(new { success = false, message = "กรุณาเลือก Area ที่ไม่ใช่ GENERAL" });
                }

                string txt_user = _claimsHelper.GetUserId(User);
                var result = await _convertPORepository.UpdateAreaAsync(request.Id, request.Area, txt_user);

                if (result)
                {
                    return Ok(new { success = true, message = "บันทึกข้อมูลสำเร็จ" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "ไม่สามารถบันทึกข้อมูลได้" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class UpdateAreaRequest
    {
        public Guid Id { get; set; }
        public string Area { get; set; }
    }
}
