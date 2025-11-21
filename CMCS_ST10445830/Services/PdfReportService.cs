using iTextSharp.text;
using iTextSharp.text.pdf;
using CMCS_ST10445830.Models;
using System.IO;

namespace CMCS_ST10445830.Services
{
    public class PdfReportService
    {
        public byte[] GenerateClaimsReport(List<Claim> claims, DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.Black);
                var title = new Paragraph("CLAIMS REPORT", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var dateRangeText = "Report Period: ";
                if (startDate.HasValue && endDate.HasValue)
                {
                    dateRangeText += $"{startDate.Value:MMM dd, yyyy} to {endDate.Value:MMM dd, yyyy}";
                }
                else
                {
                    dateRangeText += "All Time";
                }

                var dateRange = new Paragraph(dateRangeText, normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                document.Add(dateRange);

                var generatedDate = new Paragraph($"Generated: {DateTime.Now:MMM dd, yyyy at hh:mm tt}", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(generatedDate);

                var table = new PdfPTable(6)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };

                float[] columnWidths = { 2f, 3f, 2f, 2f, 2f, 3f };
                table.SetWidths(columnWidths);

                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.White);
                AddTableHeader(table, "Claim ID", headerFont, new BaseColor(51, 51, 51)); // Dark gray
                AddTableHeader(table, "Lecturer", headerFont, new BaseColor(51, 51, 51));
                AddTableHeader(table, "Hours", headerFont, new BaseColor(51, 51, 51));
                AddTableHeader(table, "Rate", headerFont, new BaseColor(51, 51, 51));
                AddTableHeader(table, "Total", headerFont, new BaseColor(51, 51, 51));
                AddTableHeader(table, "Submitted", headerFont, new BaseColor(51, 51, 51));

                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var claim in claims)
                {
                    AddTableCell(table, claim.Id.ToString(), cellFont);

                    var lecturerName = claim.Lecturer?.UserProfile != null
                        ? $"{claim.Lecturer.UserProfile.FirstName} {claim.Lecturer.UserProfile.LastName}"
                        : $"User{claim.LecturerId}";
                    AddTableCell(table, lecturerName, cellFont);

                    AddTableCell(table, claim.HoursWorked.ToString("0.0"), cellFont);
                    AddTableCell(table, claim.HourlyRateAtSubmission.ToString("C"), cellFont);
                    AddTableCell(table, claim.TotalAmount.ToString("C"), cellFont);
                    AddTableCell(table, claim.SubmittedAt.ToString("MMM dd, yyyy"), cellFont);
                }

                document.Add(table);

                document.Add(new Chunk("\n"));

                var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                var summary = new Paragraph("SUMMARY", summaryFont)
                {
                    SpacingBefore = 20f,
                    SpacingAfter = 10f
                };
                document.Add(summary);


                var totalClaims = claims.Count;
                var totalHours = claims.Sum(c => c.HoursWorked);
                var totalAmount = claims.Sum(c => c.TotalAmount);
                var averageClaim = totalClaims > 0 ? totalAmount / totalClaims : 0;

                var summaryDetails = new Paragraph();
                summaryDetails.Add(new Chunk($"Total Claims: ", summaryFont));
                summaryDetails.Add(new Chunk($"{totalClaims}\n", normalFont));
                summaryDetails.Add(new Chunk($"Total Hours: ", summaryFont));
                summaryDetails.Add(new Chunk($"{totalHours:0.0}\n", normalFont));
                summaryDetails.Add(new Chunk($"Total Amount: ", summaryFont));
                summaryDetails.Add(new Chunk($"{totalAmount:C}\n", normalFont));
                summaryDetails.Add(new Chunk($"Average per Claim: ", summaryFont));
                summaryDetails.Add(new Chunk($"{averageClaim:C}", normalFont));

                document.Add(summaryDetails);

                document.Add(new Chunk("\n\n"));
                var footer = new Paragraph("Generated by Academic Claims Management System",
                    FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.LightGray))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(footer);

                document.Close();
                return memoryStream.ToArray();
            }
        }

        private void AddTableHeader(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5f
            };
            table.AddCell(cell);
        }

        private void AddTableCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f
            };
            table.AddCell(cell);
        }
    }
}