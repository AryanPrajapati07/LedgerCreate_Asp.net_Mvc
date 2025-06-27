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
        public ActionResult Report(string ledgerName, DateTime fromDate, DateTime toDate,string ledgerGroup)
        {
            List<LedgerReport> report = new List<LedgerReport>();


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetLedgerReportings", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@LedgerGroup", string.IsNullOrEmpty(ledgerGroup)?(object)DBNull.Value:ledgerGroup);

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
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        LedgerName = reader["LedgerName"].ToString()
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
                LogAudit(null, null, null, "Failed", null);
                return BadRequest("No report data found.");
            }

            var reportList = JsonConvert.DeserializeObject<List<LedgerReport>>(reportJson);
            string ledgerName = reportList.FirstOrDefault()?.LedgerName ?? "Unknown Ledger";
            DateTime fromDate = reportList.Min(r => r.TransactionDate);
            DateTime toDate = reportList.Max(r => r.TransactionDate);

            // ✅ Create a view model to hold both report data and chart image
            var viewModel = new LedgerReportWithChartViewModel
            {
                Reports = reportList,
                ChartImageBase64 = ChartImage
            };

            var reportHtml = await _viewRenderService.RenderToStringAsync(this,"ReportWithChart", viewModel);

            var renderer = new IronPdf.HtmlToPdf();
            var pdfDoc = renderer.RenderHtmlAsPdf(reportHtml);

            string fileName = $"LedgerReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            LogAudit(ledgerName, fromDate, toDate, "Success", fileName);

            return File(pdfDoc.BinaryData, "application/pdf", fileName);
        }


        public IActionResult ReportWithChartTest()
        {
            return View("ReportWithChart");
        }

        public IActionResult GroupSummary()
        {
            List<LedgerGroupSummary> summaryList = new List<LedgerGroupSummary>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_LedgerSummaryByGroups", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    summaryList.Add(new LedgerGroupSummary
                    {
                        LedgerGroup = reader["LedgerGroup"].ToString(),
                        TotalDebit = Convert.ToDecimal(reader["TotalDebit"]),
                        TotalCredit = Convert.ToDecimal(reader["TotalCredit"]),
                        Balance = Convert.ToDecimal(reader["Balance"])
                    });
                }
            }

            return View(summaryList);
        }

        private void LogAudit(string ledgerName, DateTime? fromDate, DateTime? toDate, string status, string fileName)
        {
            var log = new LedgerReportAuditLog
            {
                LedgerName = ledgerName ?? "Unknown",
                FromDate = fromDate ?? DateTime.MinValue,
                ToDate = toDate ?? DateTime.MinValue,
                Status = status,
                FileName = fileName,
                GeneratedAt = DateTime.Now,
                UserName = User.Identity?.Name ?? "Anonymous",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            context.LedgerReportAuditLogs.Add(log);
            context.SaveChanges();
        }
        public IActionResult AuditLogs()
        {
            var logs = context.LedgerReportAuditLogs.OrderByDescending(l => l.GeneratedAt).ToList();
            return View(logs);
        }


    }
}
