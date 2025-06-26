using Microsoft.Extensions.Configuration;
using IronPdf;
using LedgerCreate.Models;
using LedgerCreate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

using Newtonsoft.Json;

namespace LedgerCreate.Controllers
{
    public class LedgerController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly string connectionString;
        private readonly IViewRenderService _viewRenderService;


        public LedgerController(ApplicationDbContext context, IConfiguration configuration, IViewRenderService viewRenderService)
        {
            this.context = context;
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _viewRenderService = viewRenderService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Report(string ledgerName, DateTime fromDate, DateTime toDate)
        {
            List<LedgerReport> report = new List<LedgerReport>();


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetLedgerReports", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    report.Add(new LedgerReport
                    {
                        TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                        Particulars = reader["Particulars"].ToString(),
                        VoucherNo = reader["VoucherNo"].ToString(),
                        DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                        CreditAmount = Convert.ToDecimal(reader["CreditAmount"]),
                        Balance = Convert.ToDecimal(reader["Balance"])
                    });
                }
            }
            HttpContext.Session.SetString("ReportData", JsonConvert.SerializeObject(report));

            return View("Report", report); // ✅ Must pass the list to the view
        }


        [HttpPost]
        public async Task<IActionResult> ExportToPdf(string ChartImage)
        {
            TempData.Keep("ReportData");

            var reportJson = HttpContext.Session.GetString("ReportData");
            if (string.IsNullOrEmpty(reportJson))
            {
                return BadRequest("No report data found.");
            }

            var reportList = JsonConvert.DeserializeObject<List<LedgerReport>>(reportJson);

            // ✅ Create a view model to hold both report data and chart image
            var viewModel = new LedgerReportWithChartViewModel
            {
                Reports = reportList,
                ChartImageBase64 = ChartImage
            };

            var reportHtml = await _viewRenderService.RenderToStringAsync(this,"ReportWithChart", viewModel);

            var renderer = new IronPdf.HtmlToPdf();
            var pdfDoc = renderer.RenderHtmlAsPdf(reportHtml);

            return File(pdfDoc.BinaryData, "application/pdf", "LedgerReport.pdf");
        }


        public IActionResult ReportWithChartTest()
        {
            return View("ReportWithChart");
        }



    }
}
