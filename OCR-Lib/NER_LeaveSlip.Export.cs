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
            SpreadsheetDocument document = null;
            
            try
            {
                document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            }
            catch(Exception ex)
            {
                StatusMessage.GetInstance().AddMessage($"Không thể tạo file {filePath}. Lỗi: {ex.Message}");
                return;
            }

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
                "Loại văn bản",
                "Ngày nộp đơn",
                "Ngày bắt đầu nghỉ",
                "Ngày kết thúc nghỉ",
                "Các ngày khác",
                "Số ngày nghỉ",
                "Tên người",
                "Đường dẫn File",
            };

            // Add header row
            Row headerRow = new Row();
            foreach (string header in headers)
            {
                headerRow.Append(CreateTextCell(header));
            }
            sheetData.Append(headerRow);

            // Add data rows
            foreach (var kvp in LeaveSlips)
            {
                LeaveSlip slip = kvp.Value;
                Row row = new Row();

                row.Append(
                    CreateTextCell(DocumentTypeMapping.SentenceCase[DocumentType.LeaveSlip]),
                    CreateTextCell(slip.CommittingDate),
                    CreateTextCell(slip.StartLeaveDate),
                    CreateTextCell(slip.EndLeaveDate),
                    CreateTextCell(string.Join(", ", slip.UndefinedDates ?? new List<string>())),
                    CreateNumberCell(slip.NumberOfLeaveDays),
                    CreateTextCell(string.Join(", ", slip.EmployeeNames ?? new List<string>())),
                    CreateTextCell(slip.FilePath)
                );

                sheetData.Append(row);
            }

            workbookPart.Workbook.Save();

            document.Dispose();
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
