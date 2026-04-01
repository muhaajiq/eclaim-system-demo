using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.SharePoint.Client;
using System.Globalization;
using Color = DocumentFormat.OpenXml.Spreadsheet.Color;
//Spreadsheet design
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;


using OpenXmlDrawing = DocumentFormat.OpenXml.Drawing;
using System.Drawing;




namespace MHA.TRAVELREQUEST.Framework.Helpers
{
    public class ExcelHelper
    {
        private static readonly JSONAppSettings appSettings;

        static ExcelHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Excel Style and Cell
        private static Stylesheet GenerateStylesheet()
        {
            Stylesheet styleSheet = null;

            Fonts fonts = new Fonts(
                new Font( // Index 0 - default
                    new FontSize() { Val = 10 }
                ),
                new Font( // Index 1 - header
                    new FontSize() { Val = 10 },
                    new Bold(),
                    new Color() { Rgb = "FFFFFF" }
                ),
                new Font( // Index 2 - report title
                    new FontSize() { Val = 16 },
                    new Bold(),
                    new FontName() { Val = "Arial" }
                ),
                new Font( // Index 3 - report header
                    new FontSize() { Val = 10 },
                    new Bold(),
                    new FontName() { Val = "Arial" }
                ),
                new Font( // Index 4 - report data
                    new FontSize() { Val = 10 },
                    new FontName() { Val = "Arial" }
                ),
                new Font( // Index 5 - Transmittal Import Header
                    new FontSize() { Val = 11 },
                    new Bold(),
                    new FontName() { Val = "Calibri" }
                ),
                new Font( // Index 6 - Transmittal Import Header Mandatory
                    new FontSize() { Val = 11 },
                    new Bold(),
                    new FontName() { Val = "Calibri" },
                    new Color() { Rgb = "FF0000" }
                ),
                new Font( // Index 7 - Transmittal Import data
                    new FontSize() { Val = 11 },
                    new FontName() { Val = "Calibri" }
                ));


            Fills fills = new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "66666666" } }) { PatternType = PatternValues.Solid }),// Index 2 - header
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "FFFF00" } }) { PatternType = PatternValues.Solid }), //Index 3 - deleted (yellow)
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "d3d3d3" } }) { PatternType = PatternValues.Solid }), // Index 4 - report header
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "DDEBF7" } }) { PatternType = PatternValues.Solid }) // Index 5 - report header
                    );

            Borders borders = new Borders(
                    new Border(), // index 0 default
                    new Border( // index 1 black border
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                );

            Alignment allignments = new Alignment();
            allignments.WrapText = true;
            allignments.Horizontal = HorizontalAlignmentValues.Justify;
            Alignment allignmentB = new Alignment();
            allignmentB.WrapText = true;
            allignmentB.Horizontal = HorizontalAlignmentValues.Justify;

            CellFormats cellFormats = new CellFormats(
                    new CellFormat(), // default
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }, // body
                    new CellFormat { FontId = 1, FillId = 2, BorderId = 1, ApplyFill = true }, // header
                    new CellFormat { FontId = 0, FillId = 3, BorderId = 1, ApplyBorder = true }, // highlighted item 
                    new CellFormat { FontId = 2, FillId = 0, BorderId = 0 }, // report title
                    new CellFormat { FontId = 3, FillId = 4, BorderId = 1, ApplyFill = true, ApplyBorder = true }, // report header
                    new CellFormat { FontId = 4, FillId = 0, BorderId = 1, ApplyBorder = true }, // report data
                    new CellFormat { FontId = 5, FillId = 5, BorderId = 1, ApplyBorder = true }, // Transmittal Import Header
                    new CellFormat { FontId = 6, FillId = 5, BorderId = 1, ApplyBorder = true }, // Transmittal Import Mandatory Header
                    new CellFormat { FontId = 7, FillId = 0, BorderId = 1, ApplyBorder = true }, // Transmittal Import data
                    new CellFormat { FontId = 4, FillId = 0, BorderId = 1, ApplyBorder = true, ApplyNumberFormat = true, NumberFormatId = 49 }, // Text Format cell
                    new CellFormat { FontId = 4, FillId = 0, BorderId = 1, ApplyBorder = true, ApplyNumberFormat = true, NumberFormatId = 14 }, //Date Format Cell
                    new CellFormat { ApplyNumberFormat = true, NumberFormatId = 14 }, //Date Cell Only
                    new CellFormat { FontId = 4, FillId = 0, BorderId = 1, ApplyBorder = true, Alignment = allignmentB, ApplyAlignment = true } //Wrap Cell
                );

            styleSheet = new Stylesheet(fonts, fills, borders, cellFormats, allignments);

            return styleSheet;
        }


        #endregion

        public static Cell ConstructCell(string value, CellValues dataType, uint styleIndex = 0)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = ProjectHelper.RemoveInvalidHexaCharacter(value);
            }
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
                StyleIndex = styleIndex
            };
        }

        private static Cell CreateTextCell(string text, uint styleIndex = 0)
        {
            //if (!String.IsNullOrEmpty(text))
            //{
            //    string text2 = string.Empty;
            //    List<string> totalLine = text.Split('\n').ToList();

            //    foreach (string line in totalLine)
            //    {
            //        text2 += line;
            //        text2 += "\n";
            //    }
            //    text = text2;
            //    //text = ProjectHelper.RemoveInvalidHexaCharacter(text);
            //}

            var cell = new Cell
            {
                DataType = CellValues.String,
                StyleIndex = styleIndex
            };
            cell.CellValue = new CellValue(text);

            return cell;
        }

        public static Cell ConstructDateCell(DateTime value)
        {
            uint styleIndex = 11;

            return new Cell()
            {
                CellValue = new CellValue(value.ToString("s")),
                DataType = CellValues.Date,
                StyleIndex = styleIndex
            };
        }

        public static Cell ConstructDateCell(string value)
        {
            uint styleIndex = 11;

            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = CellValues.Date,
                StyleIndex = styleIndex
            };
        }


        // Modified version, without using rootClientContext and clientContext (to retrieve logo)
    
        //Create func wtih logo
        public static Byte[] CreateExcelReportWithoutLogo(List<string> displayFields, List<string> internalFields, List<string> dateFields, List<dynamic> dynamicListItem, ReportInfo vmReport, ClientContext clientContext)
        {
            Byte[] byteArray = null;
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Report" };
                    sheets.Append(sheet);

                    WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = GenerateStylesheet();
                    stylePart.Stylesheet.Save();
                    workbookPart.Workbook.Save();

                    SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    //Merge cells
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:D3") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:E5") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());

                    sheetData.AppendChild(new Row(ConstructCell(vmReport.ProjectInfo, CellValues.String, 4)));
                    sheetData.AppendChild(new Row() { CustomHeight = true, Height = 5 });

                    sheetData.AppendChild(new Row(ConstructCell(vmReport.ReportTitle, CellValues.String, 4)));
                    sheetData.AppendChild(new Row() { CustomHeight = true, Height = 5 });

                    sheetData.AppendChild(new Row(ConstructCell($"Generated on {vmReport.GeneratedDate} by {vmReport.GeneratedBy}", CellValues.String, 4)));
                    sheetData.AppendChild(new Row() { CustomHeight = true, Height = 5 });

                    // Constructing header
                    Row row = new Row();
                    int i = 0;

                    foreach (string fieldName in displayFields)
                    {
                        row.Append(ConstructCell(fieldName, CellValues.String, 5));
                    }

                    // Insert the header row to the Sheet Data
                    sheetData.AppendChild(row);

                    //Construc Rows
                    foreach (dynamic dynRow in dynamicListItem)
                    {
                        Row excelRow = new Row();
                        IDictionary<string, object> reportClass = dynRow;

                        foreach (string fieldName in internalFields)
                        {
                            string _value = String.Empty;

                            object value = reportClass[fieldName];

                            if (value != null)
                            {
                                _value = value + "";
                            }

                            if (dateFields.Contains(fieldName))
                            {
                                if (DateTime.TryParse(_value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime fieldDateTime))
                                    excelRow.Append(ConstructDateCell(fieldDateTime));
                                else if (String.IsNullOrEmpty(_value))
                                    excelRow.Append(ConstructDateCell(_value));
                                else
                                    excelRow.Append(ConstructCell(_value, CellValues.String, 6));
                            }
                            else
                            {
                                excelRow.Append(ConstructCell(_value, CellValues.String, 6));
                            }

                        }
                        sheetData.AppendChild(excelRow);
                    }
                    worksheetPart.Worksheet.Save();

                }
                byteArray = mem.ToArray();
            }

            return byteArray;
        }

      

        public static Byte[] CreateExcelReportWithLogo(List<string> displayFields, List<string> internalFields, List<string> dateFields, List<dynamic> dynamicListItem, ReportInfo vmReport, ClientContext clientContext)
        {
            Byte[] byteArray = null;
            using (MemoryStream mem = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Report" };
                    sheets.Append(sheet);

                    WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = GenerateStylesheet();
                    stylePart.Stylesheet.Save();
                    workbookPart.Workbook.Save();

                    SheetData sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                    Columns columns = new Columns();
                    for (int i = 1; i <= 12; i++) // A–L
                    {
                        columns.Append(new Column()
                        {
                            Min = (UInt32)i,
                            Max = (UInt32)i,
                            Width = 22,
                            CustomWidth = true
                        });
                    }
                    worksheetPart.Worksheet.InsertAt(columns, 0);

                    //Merge cells
                    MergeCells mergeCells = new MergeCells();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:A3") }); // logo area
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:L5") }); // project info
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A6:L6") }); // report title
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A7:L7") });



                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());



                    //Insert image
                    string filePath = new Uri(vmReport.LogoUrl).AbsolutePath;
                    Microsoft.SharePoint.Client.File file;
                    ClientResult<Stream> data;
                   
                    file = CSOMHelper.GetFileByServerRelativePath(filePath, clientContext);
                    clientContext.Load(file, x => x.Name);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                   
                    data = file.OpenBinaryStream();
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    DrawingsPart drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();

                    if (!worksheetPart.Worksheet.ChildElements.OfType<Drawing>().Any())
                    {
                        worksheetPart.Worksheet.Append(new Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) });
                    }
                    if (drawingsPart.WorksheetDrawing == null)
                    {
                        drawingsPart.WorksheetDrawing = new OpenXmlDrawing.Spreadsheet.WorksheetDrawing();
                    }

                    OpenXmlDrawing.Spreadsheet.WorksheetDrawing worksheetDrawing = drawingsPart.WorksheetDrawing;

                    string extension = Path.GetExtension(file.Name).ToLower();
                    string contentType;

                    switch (extension)
                    {
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".jpg":
                        case ".jpeg":
                            contentType = "image/jpeg";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        default:
                            return null;
                    }

                    ImagePart imagePart = drawingsPart.AddImagePart(contentType);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        data.Value.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        imagePart.FeedData(memoryStream);
                    }

                    Bitmap bitMap = null;
                    OpenXmlDrawing.Extents extents = null;
                    using (Stream stream = data.Value)
                    {
                        bitMap = new Bitmap(stream);

                        // Target max display height in points (55 points ~ 0.77 inches)
                        const double maxHeightPoints = 55.0;

                        // Convert bitmap DPI to English Metric Units (EMU = 914400 per inch)
                        const double emusPerInch = 914400.0;

                        // Calculate actual size in EMU
                        double cx = bitMap.Width * (emusPerInch / bitMap.HorizontalResolution);
                        double cy = bitMap.Height * (emusPerInch / bitMap.VerticalResolution);

                        // Scale down proportionally if taller than max height
                        double heightPoints = bitMap.Height * 72.0 / bitMap.VerticalResolution; // pixels → points
                        double scale = heightPoints > maxHeightPoints ? maxHeightPoints / heightPoints : 1.0;

                        cx *= scale;
                        cy *= scale;

                        extents = new OpenXmlDrawing.Extents
                        {
                            Cx = (long)cx,
                            Cy = (long)cy
                        };
                    }

                    bitMap.Dispose();

                    //sheetData.AppendChild(new Row() { CustomHeight = true, Height = 30 });
                    //sheetData.AppendChild(new Row() { CustomHeight = true, Height = 22 });
                    //sheetData.AppendChild(new Row() { CustomHeight = true, Height = 22 });



                    var colNumber = 1;
                    var rowNumber = 1;
                    var colOffset = 5000;
                    var rowOffset = 1000;

                    var nonVisualDrawingProperties = worksheetDrawing.Descendants<OpenXmlDrawing.Spreadsheet.NonVisualDrawingProperties>();
                    var nonVisualDrawingPropertiesId = nonVisualDrawingProperties.Count() > 0 ? (UInt32Value)worksheetDrawing.Descendants<OpenXmlDrawing.Spreadsheet.NonVisualDrawingProperties>().Max(p => p.Id.Value) + 1 : 1U;

                    OpenXmlDrawing.Spreadsheet.OneCellAnchor oneCellAnchor = new OpenXmlDrawing.Spreadsheet.OneCellAnchor(
                        new OpenXmlDrawing.Spreadsheet.FromMarker
                        {
                            ColumnId = new OpenXmlDrawing.Spreadsheet.ColumnId((colNumber - 1).ToString()),
                            RowId = new OpenXmlDrawing.Spreadsheet.RowId((rowNumber - 1).ToString()),
                            ColumnOffset = new OpenXmlDrawing.Spreadsheet.ColumnOffset(colOffset.ToString()),
                            RowOffset = new OpenXmlDrawing.Spreadsheet.RowOffset(rowOffset.ToString())
                        },
                        new OpenXmlDrawing.Spreadsheet.Extent { Cx = extents.Cx, Cy = extents.Cy },
                        new OpenXmlDrawing.Spreadsheet.Picture(
                            new OpenXmlDrawing.Spreadsheet.NonVisualPictureProperties(
                                new OpenXmlDrawing.Spreadsheet.NonVisualDrawingProperties { Id = nonVisualDrawingPropertiesId, Name = "Picture " + nonVisualDrawingPropertiesId, Description = file.Name },
                                new OpenXmlDrawing.Spreadsheet.NonVisualPictureDrawingProperties(new OpenXmlDrawing.PictureLocks { NoChangeAspect = true })
                            ),
                            new OpenXmlDrawing.Spreadsheet.BlipFill(
                                new OpenXmlDrawing.Blip { Embed = drawingsPart.GetIdOfPart(imagePart), CompressionState = OpenXmlDrawing.BlipCompressionValues.Print },
                                new OpenXmlDrawing.Stretch(new OpenXmlDrawing.FillRectangle())
                            ),
                            new OpenXmlDrawing.Spreadsheet.ShapeProperties(
                                new OpenXmlDrawing.Transform2D(
                                    new OpenXmlDrawing.Offset { X = 0, Y = 0 },
                                    new OpenXmlDrawing.Extents { Cx = extents.Cx, Cy = extents.Cy }
                                ),
                                new OpenXmlDrawing.PresetGeometry { Preset = OpenXmlDrawing.ShapeTypeValues.Rectangle }
                            )
                        ),
                        new OpenXmlDrawing.Spreadsheet.ClientData()
                    );

                    worksheetDrawing.Append(oneCellAnchor);

                    Row row5 = new Row() { RowIndex = 5 };
                    row5.Append(ConstructCell(vmReport.ProjectInfo, CellValues.String, 4));
                    sheetData.Append(row5);

                    Row row6 = new Row() { RowIndex = 6 };
                    row6.Append(ConstructCell(vmReport.ReportTitle, CellValues.String, 4));
                    sheetData.Append(row6);

                    Row row7 = new Row() { RowIndex = 7 };
                    row7.Append(ConstructCell($"Generated on {vmReport.GeneratedDate} by {vmReport.GeneratedBy}", CellValues.String, 4));
                    sheetData.Append(row7);




                    // Constructing header
                    Row row = new Row();
                    foreach (string fieldName in displayFields)
                    {
                        row.Append(ConstructCell(fieldName, CellValues.String, 5));
                    }
                    sheetData.AppendChild(row);

                    //Construc Rows
                    foreach (dynamic dynRow in dynamicListItem)
                    {
                        Row excelRow = new Row();
                        IDictionary<string, object> reportClass = dynRow;
                        foreach (string fieldName in internalFields)
                        {
                            string _value = string.Empty;
                            object value = reportClass[fieldName];
                            //if (value != null)
                            //{
                            //    _value = value + "";

                            //}

                            //QUESTION: DO YOU WANT TO LEAVE IT BLANK OR APPLY SOME TEXT IN THE EXCEL IF NULL
                            if (value == null || (value is DateTime bt && bt == DateTime.MinValue))
                            {
                                // Write blank cell and skip to next field
                                excelRow.Append(ConstructCell(string.Empty, CellValues.String, 6));
                                continue;
                            }

                            _value = value.ToString();

                            if (dateFields.Contains(fieldName))
                            {
                                if (value == null || (value is DateTime dt && dt == DateTime.MinValue))
                                {
                                    // Empty date → write blank cell
                                    excelRow.Append(ConstructCell(string.Empty, CellValues.String, 6));
                                }
                                else if (DateTime.TryParse(_value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime fieldDateTime))
                                    excelRow.Append(ConstructDateCell(fieldDateTime));
                               
                                else if (string.IsNullOrEmpty(_value))
                                    excelRow.Append(ConstructDateCell(_value));
                                else
                                    excelRow.Append(ConstructCell(_value, CellValues.String, 6));
                            }
                            else
                            {
                                excelRow.Append(ConstructCell(_value, CellValues.String, 6));
                            }

                        }
                        sheetData.AppendChild(excelRow);
                    }
                    worksheetPart.Worksheet.Save();            
                }
                byteArray = mem.ToArray();
            }
            return byteArray;
        }

    }

}