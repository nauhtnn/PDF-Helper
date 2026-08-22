using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public sealed partial class NER_LeaveSlip : DocumentProcessor
    {
        public void ExportToXlsx(string filePath)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "LeaveSlips"
                };
                sheets.Append(sheet);

                // Define headers
                string[] headers = new[]
                {
                "FilePath",
                "CommittingDate",
                "StartLeaveDate",
                "EndLeaveDate",
                "UndefinedDates",
                "NumberOfLeaveDays",
                "EmployeeNames"
            };

                // Add header row
                Row headerRow = new Row();
                foreach (string header in headers)
                {
                    Cell cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(header)
                    };
                    headerRow.Append(cell);
                }
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var kvp in LeaveSlips)
                {
                    LeaveSlip slip = kvp.Value;
                    Row row = new Row();

                    row.Append(
                        CreateTextCell(slip.FilePath),
                        CreateTextCell(slip.CommittingDate),
                        CreateTextCell(slip.StartLeaveDate),
                        CreateTextCell(slip.EndLeaveDate),
                        CreateTextCell(string.Join(", ", slip.UndefinedDates ?? new List<string>())),
                        CreateNumberCell(slip.NumberOfLeaveDays),
                        CreateTextCell(string.Join(", ", slip.EmployeeNames ?? new List<string>()))
                    );

                    sheetData.Append(row);
                }

                workbookPart.Workbook.Save();
            }
        }

        private Cell CreateTextCell(string text)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(text ?? string.Empty)
            };
        }

        private Cell CreateNumberCell(int number)
        {
            return new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(number.ToString())
            };
        }
    }
}
