using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;
using Metrics.Application.DTOs.UserGroup;
using Metrics.Application.Interfaces.IServices;
using System.Globalization;

namespace Metrics.Infrastructure.Services;

public class KeyKpiReportService : IKeyKpiReportService
{
    public Task<MemoryStream> ExportExcel_KeyKpiSummaryReport(List<UserGroupDto> userGroupDtoList, List<KeyKpi_SummaryReportDto> summaryReportDto)
    {
        var memoryStream = new MemoryStream();

        // Prepare Data
        List<Dictionary<string, object>> excelData = [];

        // SummaryReport_Multiple
        var colPeriod = "Period Name";
        var colKeyDepartment = "Key Departments";
        var colKpiKey = "KPI Key";
        var colCandidateDepartmentCount = "No. of Candidate Departments";
        var colReceivedSubmission = "Submission Received";
        var colReceivedScore = "Score Received";
        var colTotal = "Score/Submission";
        var colTotalScore = "Total Score";
        var colFinalScore = "Final Score";

        // Build list of all headers in order
        var headersList = new List<string>
        {
            colPeriod,
            colKeyDepartment,
            colKpiKey
        };

        // Add user group columns
        foreach (var g in userGroupDtoList)
        {
            headersList.Add($"{g.GroupName}_Submissions");
        }

        // Add remaining columns
        headersList.AddRange(
        [
            colCandidateDepartmentCount,
            colReceivedSubmission,
            colReceivedScore,
            colTotal,
            colTotalScore,
            colFinalScore
        ]);

        // Track merge ranges: key = columnName, value = (startRow, endRow)
        var mergeRanges = new Dictionary<string, (int startRow, int endRow)>();

        foreach (var parent in summaryReportDto)
        {
            var firstRow = true;
            var totalScoreSum = parent.SummaryReportItems.Sum(i => i.AverageScore);
            int parentStartRow = excelData.Count + 2; // +2 because header is row 1, data starts at row 2

            foreach (var item in parent.SummaryReportItems)
            {
                var excelRow = new Dictionary<string, object>();

                excelRow[colPeriod] = item.PeriodName;
                excelRow[colKeyDepartment] = item.KeyIssueDepartment.DepartmentName;
                excelRow[colKpiKey] = item.KeyMetric.MetricTitle;
                // user group columns (group name as column name)
                foreach (var g in userGroupDtoList)
                {
                    // item => period, list of SummaryReport_CandidateDepartmentScore
                    // item.SummaryReport_CandidateDepartmentScoreList => canidate department, score total, list of submissions details
                    // ---to get number of submission by candidate group---
                    // from::CandidateDepartmentScoreList, get::list of submission details
                    var submissionDetails = item.Submissions
                        .SelectMany(e => e.SubmissionDetails)
                        .Count(d => d.CandidateGroup.Equals(g.GroupName, StringComparison.OrdinalIgnoreCase));
                    excelRow[$"{g.GroupName}_Submissions"] = submissionDetails;
                }
                excelRow[colCandidateDepartmentCount] = item.Submissions.Count;
                excelRow[colReceivedSubmission] = item.ReceivedSubmissions;
                excelRow[colReceivedScore] = item.ReceivedScore;
                excelRow[colTotal] = item.AverageScore;

                if (firstRow)
                {
                    excelRow[colTotalScore] = totalScoreSum;
                    excelRow[colFinalScore] = parent.FinalScore;
                    firstRow = false;
                }
                else
                {
                    excelRow[colTotalScore] = string.Empty;
                    excelRow[colFinalScore] = string.Empty;
                }
                excelData.Add(excelRow);
            }

            // Track merge for Total Score and Final Score columns
            int parentEndRow = excelData.Count + 1; // +1 because header is row 1
            if (parentStartRow < parentEndRow)
            {
                int totalScoreColIndex = headersList.IndexOf(colTotalScore) + 1;
                int finalScoreColIndex = headersList.IndexOf(colFinalScore) + 1;

                mergeRanges[$"{GetColumnName(totalScoreColIndex)}{parentStartRow}:{GetColumnName(totalScoreColIndex)}{parentEndRow}"] = (parentStartRow, parentEndRow);
                mergeRanges[$"{GetColumnName(finalScoreColIndex)}{parentStartRow}:{GetColumnName(finalScoreColIndex)}{parentEndRow}"] = (parentStartRow, parentEndRow);
            }
        }



        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart() ?? throw new InvalidOperationException("Failed to create workbook part.");
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            // ------------------------------------------------
            // LOW LEVEL WRITER
            // ------------------------------------------------
            using (OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                // --- Write Header Row (Row 1) ---
                writer.WriteStartElement(new Row() { RowIndex = 1 });

                int colIndex = 1;
                foreach (var header in headersList)
                {
                    Cell cell = new Cell()
                    {
                        CellReference = GetColumnName(colIndex) + "1",
                        DataType = CellValues.String,
                        CellValue = new CellValue(header) // Header name
                    };
                    writer.WriteElement(cell);
                    colIndex++;
                }
                writer.WriteEndElement(); // End Row

                // --- Write Data Rows (Row 2 onwards) ---
                int rowIndex = 2;
                foreach (var rowData in excelData)
                {
                    writer.WriteStartElement(new Row() { RowIndex = (uint)rowIndex });

                    colIndex = 1;
                    foreach (var header in headersList)
                    {
                        rowData.TryGetValue(header, out var value);

                        // Determine Type and Value
                        var (cellType, cellValue) = GetCellValue(value);

                        Cell cell = new Cell()
                        {
                            CellReference = GetColumnName(colIndex) + rowIndex,
                            DataType = cellType,
                            CellValue = new CellValue(cellValue)
                        };

                        writer.WriteElement(cell);
                        colIndex++;
                    }
                    writer.WriteEndElement(); // End Row
                    rowIndex++;
                }

                writer.WriteEndElement(); // End SheetData

                // Write MergeCells element after SheetData
                if (mergeRanges.Count > 0)
                {
                    writer.WriteStartElement(new MergeCells() { Count = (uint)mergeRanges.Count });
                    foreach (var range in mergeRanges.Keys)
                    {
                        writer.WriteElement(new MergeCell() { Reference = range });
                    }
                    writer.WriteEndElement(); // End MergeCells
                }

                writer.WriteEndElement(); // End Worksheet
            }

            // ------------------------------------------------
            // FINALIZE WORKBOOK
            // ------------------------------------------------
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            Sheet sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Data"
            };
            sheets.Append(sheet);
            workbookPart.Workbook.Save();
        }

        // Reset position to beginning so caller can read the stream
        memoryStream.Position = 0;

        return Task.FromResult(memoryStream);
    }


    // ==========HELPERs==================================================
    // Helper to convert generic object to Excel cell value
    private static (CellValues DataType, string Value) GetCellValue(object? value)
    {
        if (value == null || value == DBNull.Value)
            return (CellValues.String, "");

        Type type = value.GetType();

        // Handle Numbers (Int, Double, Decimal, etc.)
        if (type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) || type == typeof(long))
        {
            // Must use InvariantCulture to ensure dots (.) are used for decimals regardless of local PC settings
            return (CellValues.Number, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
        }

        // Handle Booleans
        if (type == typeof(bool))
        {
            // Excel stores booleans as 1 or 0, but expects the DataType to be Boolean
            return (CellValues.Boolean, (bool)value ? "1" : "0");
        }

        // Handle Dates
        if (type == typeof(DateTime))
        {
            // NOTE: Low-level Excel date handling is complex (requires Styles/Formats).
            // For simplicity here, we export Dates as ISO Strings (Sortable).
            // Excel will treat this as text, but it is human-readable.
            return (CellValues.String, ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
        }

        // Default to String
        return (CellValues.String, value.ToString() ?? string.Empty);
    }

    // Helper to convert column index (1) to Excel Letter (A)
    private string GetColumnName(int columnIndex)
    {
        int dividend = columnIndex;
        string columnName = string.Empty;

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
            dividend = (int)((dividend - modulo) / 26);
        }

        return columnName;
    }
























    // public void ExportData(string filePath, string[][] data)
    // {
    //     // 1. Create the SpreadsheetDocument
    //     // This creates the .xlsx file structure (which is essentially a zip file)
    //     using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
    //     {
    //         // 2. Add the main WorkbookPart
    //         WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
    //         workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

    //         // 3. Add a WorksheetPart
    //         WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

    //         // 4. Use OpenXmlWriter (The "Low Level" tool)
    //         // This writes XML directly to the stream without creating heavy objects for every cell
    //         using (OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart))
    //         {
    //             writer.WriteStartElement(new Worksheet()); // <Worksheet>
    //             writer.WriteStartElement(new SheetData()); // <SheetData>

    //             int rowIndex = 1;

    //             // 5. Iterate your data
    //             foreach (var rowData in data)
    //             {
    //                 // Create the Row element
    //                 Row row = new Row() { RowIndex = (uint)rowIndex };
    //                 writer.WriteElement(row);

    //                 int colIndex = 1;

    //                 foreach (var cellValue in rowData)
    //                 {
    //                     // Create the Cell element
    //                     // Note: For production code, you usually convert column index to letters (e.g., 1 = A, 2 = B)
    //                     Cell cell = new Cell()
    //                     {
    //                         CellReference = GetCellReference(rowIndex, colIndex),
    //                         DataType = CellValues.String, // Simple string type
    //                         CellValue = new CellValue(cellValue)
    //                     };

    //                     writer.WriteElement(cell);
    //                     colIndex++;
    //                 }

    //                 rowIndex++;
    //             }

    //             writer.WriteEndElement(); // </SheetData>
    //             writer.WriteEndElement(); // </Worksheet>
    //         }

    //         // 6. Finalize the Workbook (Save)
    //         // We need to link the WorksheetPart to the Workbook
    //         spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

    //         Sheet sheet = new Sheet()
    //         {
    //             Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
    //             SheetId = 1,
    //             Name = "Export"
    //         };

    //         spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(sheet);

    //         workbookPart.Workbook.Save();
    //     }
    // }



    // // Helper to convert Row/Col numbers (1,1) to Excel strings (A1)
    // private string GetCellReference(int row, int col)
    // {
    //     // // Simplified for example. A robust solution handles A-Z, AA-AZ, etc.
    //     // return "A" + row; // This only works for Column 1. 
    //     // // See helper below for full implementation.

    //     // Excel columns are 1-based
    //     col--; // Adjust to 0-based for calculation

    //     string columnName = "";

    //     while (col >= 0)
    //     {
    //         int remainder = col % 26;
    //         columnName = Convert.ToChar('A' + remainder) + columnName;
    //         col = (col / 26) - 1;
    //     }

    //     return columnName + row;
    // }
}
