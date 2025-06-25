using Microsoft.Extensions.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;
using LedgerCreate.Models;
using LedgerCreate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LedgerCreate.Controllers
{
    public class LedgerController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly string connectionString;

        public LedgerController(ApplicationDbContext context, IConfiguration configuration)
        {
            this.context = context;
            connectionString = configuration.GetConnectionString("DefaultConnection");
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
                SqlCommand cmd = new SqlCommand("sp_GetLedgerReport", conn);
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

            return View("Report", report);
        }

        public IActionResult ExportToPdf()
        {
            var reportHtml = RenderViewToString("Report", TempData["ReportData"]);
            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4
        },
                Objects = {
            new ObjectSettings()
            {
                HtmlContent = (string)reportHtml
            }
        }
            };
            var file = converter.Convert(doc);
            return File(file, "application/pdf", "LedgerReport.pdf");
        }

        private object RenderViewToString(string v1, object? v2)
        {
            throw new NotImplementedException();
        }
    }

    internal class PdfTools
    {
        public PdfTools()
        {
        }
    }
}
