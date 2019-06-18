#region Related components
using System;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Helper class for working with Excel (with OpenXML format)
	/// </summary>
	public static partial class ExcelService
	{
		/// <summary>
		/// Creates a stream that contains Excel document from this data-set
		/// </summary>
		/// <param name="dataset">DataSet containing the data to be written to the Excel in OpenXML format</param>
		/// <returns>A stream that contains the Excel document</returns>
		/// <remarks>The stream that contains an Excel document in OpenXML format with MIME type is 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'</remarks>
		public static MemoryStream SaveAsExcel(this DataSet dataset)
		{
			// check dataset
			if (dataset == null || dataset.Tables == null || dataset.Tables.Count < 1)
				throw new InformationNotFoundException("DataSet must be not null and contains at least one table");

			// write dataset into stream
			var stream = UtilityService.CreateMemoryStream();
			using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
			{
				ExcelService.WriteExcelDocument(dataset, document);
			}
			return stream;
		}

		/// <summary>
		/// Creates a stream that contains Excel document from this data-set
		/// </summary>
		/// <param name="dataset">DataSet containing the data to be written to the Excel in OpenXML format</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A stream that contains the Excel document</returns>
		/// <remarks>The stream that contains an Excel document in OpenXML format with MIME type is 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'</remarks>
		public static Task<MemoryStream> SaveAsExcelAsync(this DataSet dataset, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => dataset.SaveAsExcel(), cancellationToken);

		#region Write a data-set to Excel document
		static void WriteExcelDocument(DataSet dataset, SpreadsheetDocument spreadsheet)
		{
			//  Create the Excel document contents.
			// This function is used when creating an Excel file either writing to a file, or writing to a MemoryStream.
			spreadsheet.AddWorkbookPart();
			spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

			//  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
			spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

			//  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
			var workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
			var stylesheet = new Stylesheet();
			workbookStylesPart.Stylesheet = stylesheet;

			//  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
			uint worksheetNumber = 1;
			foreach (DataTable dataTable in dataset.Tables)
			{
				//  For each worksheet you want to create
				var workSheetID = "rId" + worksheetNumber.ToString();
				var worksheetName = dataTable.TableName;

				var newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
				newWorksheetPart.Worksheet = new Worksheet();

				// create sheet data
				newWorksheetPart.Worksheet.AppendChild(new SheetData());

				// save worksheet
				ExcelService.WriteDataTableToExcelWorksheet(dataTable, newWorksheetPart);
				newWorksheetPart.Worksheet.Save();

				// create the worksheet to workbook relation
				if (worksheetNumber == 1)
					spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());

				spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
				{
					Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
					SheetId = (uint)worksheetNumber,
					Name = dataTable.TableName
				});

				worksheetNumber++;
			}

			spreadsheet.WorkbookPart.Workbook.Save();
		}

		static void WriteDataTableToExcelWorksheet(DataTable dataTable, WorksheetPart worksheetPart)
		{
			var worksheet = worksheetPart.Worksheet;
			var sheetData = worksheet.GetFirstChild<SheetData>();

			var cellValue = "";

			//  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
			//
			//  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
			//  cells of data, we'll know if to write Text values or Numeric cell values.
			var numberOfColumns = dataTable.Columns.Count;
			var isNumericColumn = new bool[numberOfColumns];

			var excelColumnNames = new string[numberOfColumns];
			for (var column = 0; column < numberOfColumns; column++)
				excelColumnNames[column] = ExcelService.GetExcelColumnName(column);

			//
			//  Create the Header row in our Excel Worksheet
			//
			uint rowIndex = 1;

			// add a row at the top of spreadsheet
			var headerRow = new Row
			{
				RowIndex = rowIndex
			};
			sheetData.Append(headerRow);

			for (int index = 0; index < numberOfColumns; index++)
			{
				var col = dataTable.Columns[index];
				ExcelService.AppendTextCell(excelColumnNames[index] + "1", col.ColumnName, headerRow);
				isNumericColumn[index] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
			}

			//
			//  Now, step through each row of data in our DataTable...
			//
			double cellNumericValue = 0;
			foreach (DataRow dataRow in dataTable.Rows)
			{
				// ...create a new row, and append a set of this row's data to it.
				++rowIndex;

				// add a row at the top of spreadsheet
				var newExcelRow = new Row
				{
					RowIndex = rowIndex
				};
				sheetData.Append(newExcelRow);

				for (var index = 0; index < numberOfColumns; index++)
				{
					cellValue = dataRow.ItemArray[index].ToString();

					// Create cell with data
					if (isNumericColumn[index])
					{
						//  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
						//  If this numeric value is NULL, then don't write anything to the Excel file.
						cellNumericValue = 0;
						if (double.TryParse(cellValue, out cellNumericValue))
						{
							cellValue = cellNumericValue.ToString();
							ExcelService.AppendNumericCell(excelColumnNames[index] + rowIndex.ToString(), cellValue, newExcelRow);
						}
					}
					//  For text cells, just write the input data straight out to the Excel file.
					else
						ExcelService.AppendTextCell(excelColumnNames[index] + rowIndex.ToString(), cellValue, newExcelRow);
				}
			}
		}

		static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
		{
			var cell = new Cell()
			{
				CellReference = cellReference,
				DataType = CellValues.String
			};

			cell.Append(new CellValue
			{
				Text = cellStringValue
			});

			excelRow.Append(cell);
		}

		static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
		{
			var cell = new Cell()
			{
				CellReference = cellReference
			};

			cell.Append(new CellValue
			{
				Text = cellStringValue
			});

			excelRow.Append(cell);
		}

		static string GetExcelColumnName(int columnIndex)
		{
			//  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
			//
			//		GetExcelColumnName(0) should return "A"
			//    GetExcelColumnName(1) should return "B"
			//    GetExcelColumnName(25) should return "Z"
			//    GetExcelColumnName(26) should return "AA"
			//    GetExcelColumnName(27) should return "AB"
			//    ..etc..
			//
			if (columnIndex < 26)
				return ((char)('A' + columnIndex)).ToString();

			var firstChar = (char)('A' + (columnIndex / 26) - 1);
			var secondChar = (char)('A' + (columnIndex % 26));

			return $"{firstChar}{secondChar}";
		}
		#endregion

	}
}