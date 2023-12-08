using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace SefimV2.Helper.ExcelHelper
{
    public class ExcelOptions
    {
        public int MaksimumHataSayisi { get; set; }
    }
    public class ExcelHelper
    {
        public static byte[] GetTempFile(string fileName, string folderPath)
        {
            string filePath = Path.Combine(folderPath, fileName);

            return File.ReadAllBytes(filePath);
        }

        public ExcelHelper()
        {
            HataMesaji = new List<string>();
            ExcelSatir = new List<Dictionary<string, object>>();
        }

        #region Property

        public bool HataVarmi
        {
            get
            {
                return (HataMesaji != null && HataMesaji.Count() > 0);
            }
        }
        public List<string> HataMesaji { get; private set; }

        private string SheetName { get; set; }

        public List<Dictionary<string, object>> ExcelSatir { get; set; }

        private XmlDocument _ExcelMetaDataSablon;
        private XmlDocument ExcelMetaDataSablon
        {
            get
            {
                return _ExcelMetaDataSablon ?? ExcelMetaDataSablonOku();

            }
            set
            {
                _ExcelMetaDataSablon = value;
            }
        }
        #endregion

        #region XMLSablon işlemleri
        private XmlDocument ExcelMetaDataSablonOku()
        {
            XmlDocument doc = new XmlDocument();
            string path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"), WebConfigurationManager.AppSettings["ExcelMetaDataSablonAdresi"].ToString());
            doc.Load(path);
            ExcelMetaDataSablon = doc;
            return doc;
        }
        //private XmlNode GetHeaderNameAndAttributes(ExcelDokumanTipi excelDokumanTipi)
        //{
        //    foreach (XmlNode node in ExcelMetaDataSablon.DocumentElement.SelectNodes("/ExcelDosyalari/ExcelDosya"))
        //    {
        //        if (node.Attributes["Name"].Value == excelDokumanTipi.ToString())
        //        {
        //            SheetName = node.Attributes["SheetName"].Value;
        //            return node;
        //        }
        //    }
        //    throw new ApplicationException(ConstantMessages.ExcelDokumaniAyariBulunamadi);
        //}
        //private List<ExcelSablonKolon> GetColumns(ExcelDokumanTipi excelDokumanTipi)
        //{

        //    XmlNode node = GetHeaderNameAndAttributes(excelDokumanTipi);

        //    List<ExcelSablonKolon> list = new List<ExcelSablonKolon>();

        //    int i = 1;
        //    foreach (XmlNode item in node.ChildNodes)
        //    {
        //        if (item.NodeType != XmlNodeType.Comment)
        //        {
        //            var eskt = new ExcelSablonKolon();

        //            eskt.SiraNumarasi = i;
        //            eskt.Adi = item.Attributes["Adi"].Value;
        //            eskt.VeriTipi = item.Attributes["VeriTipi"].Value;
        //            eskt.DinamikPropertyName = item.Attributes["DynamicPropName"].Value;
        //            eskt.Zorunlu = item.Attributes["Zorunlumu"].Value == "Evet";
        //            eskt.ExcelColumnName = GetExcelColumnName(eskt.SiraNumarasi);

        //            i++;

        //            list.Add(eskt);
        //        }
        //    }
        //    return list;
        //}
        #endregion

        #region Import İşlemleri
        ////public void ReadFromExcel(byte[] excelContent, ExcelDokumanTipi excelDokumanTipi, ExcelOptions excelOptions = null)
        ////{
        ////    if (excelOptions == null)
        ////    {
        ////        excelOptions = new ExcelOptions();
        ////        excelOptions.MaksimumHataSayisi = 100;
        ////    }

        ////    //List<ExcelSablonKolon> kolonlar = GetColumns(excelDokumanTipi);

        ////    using (Stream fs = new MemoryStream(excelContent))
        ////    {
        ////        try
        ////        {
        ////            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fs, false))
        ////            {
        ////                WorkbookPart wbPart = document.WorkbookPart;
        ////                SharedStringTablePart sstpart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
        ////                SharedStringTable sst = sstpart.SharedStringTable;
        ////                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name.Value.Trim() == SheetName.Trim()).FirstOrDefault();

        ////                if (theSheet == null)
        ////                {
        ////                    HataMesajiYonet("ExcelSheetBulunamadi", SheetName);
        ////                }
        ////                else
        ////                {
        ////                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
        ////                    Worksheet workSheet = wsPart.Worksheet;
        ////                    var rows = workSheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(x => !string.IsNullOrEmpty(x.InnerText) && !string.IsNullOrEmpty(x.InnerXml));
        ////                    if (rows.LongCount() == 1)
        ////                    {
        ////                        HataMesajiYonet("ExcelDosyaKayitBulunamadi", string.Empty);
        ////                    }
        ////                    // öncelikle başlıkların doğru ve sıralı olup olmadığı kontrol edilir.
        ////                    // Eğer başlıklar doğru değilse devamı kontrol edilmez.
        ////                    if (rows.LongCount() > 0)
        ////                    {
        ////                        var header = rows.First();
        ////                        BaslikSiralariveAdlarininKontrolEt(header, ref kolonlar, ref sst);

        ////                        if (HataVarmi == false)
        ////                        {
        ////                            //ilk satır başlık.
        ////                            rows.Skip(1).ToList().ForEach(row => KolondaBulunanVerilerSablonaUyuyorMu(row, ref kolonlar, ref sst, excelOptions));
        ////                        }
        ////                    }
        ////                    else
        ////                    {
        ////                        HataMesajiYonet("ExcelDosyaKayitBulunamadi", string.Empty);
        ////                    }
        ////                }
        ////            }
        ////        }
        ////        catch (FileFormatException)
        ////        {
        ////            HataMesaji.Add("ExcelFormatiUygunDegil");
        ////        }
        ////    }
        ////}

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private void KolondaBulunanVerilerSablonaUyuyorMu(DocumentFormat.OpenXml.Spreadsheet.Row row, ref List<ExcelSablonKolon> kolonlar, ref SharedStringTable sst, ExcelOptions excelOptions)
        {
            if (HataMesaji != null && HataMesaji.Count() > excelOptions.MaksimumHataSayisi)
            {
                return;
            }

            var excelSatir = new Dictionary<string, object>();
            var cells = row.Descendants<Cell>();

            for (int i = 0; i < kolonlar.Count(); i++)
            {
                var excelSablonKolon = kolonlar[i];
                var cellReferenceValue = excelSablonKolon.ExcelColumnName + row.RowIndex;
                var cell = cells.Where(c => c.CellReference.Value == cellReferenceValue).FirstOrDefault();
                object deger = null;

                if (cell == null || cell.CellValue == null)
                {
                    if (excelSablonKolon.Zorunlu)
                    {
                        HataMesajiYonet(string.Format("{0} hücresinde ({1}) veri bekleniyor", cellReferenceValue, excelSablonKolon.Adi), "");
                    }
                }
                else
                {
                    deger = GetValueFromCell(cell, excelSablonKolon, ref sst);
                }

                excelSatir.Add(excelSablonKolon.DinamikPropertyName, deger);
            }

            ExcelSatir.Add(excelSatir);
        }

        private object GetValueFromCell(Cell cell, ExcelSablonKolon excelSablonKolon, ref SharedStringTable sst)
        {
            object deger = null;
            var textDegeri = string.Empty;

            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                textDegeri = sst.ChildElements[int.Parse(cell.CellValue.Text)].InnerText.Trim();
            }
            else if (cell.CellValue != null)
            {
                textDegeri = cell.CellValue.Text.Trim();
            }

            if (excelSablonKolon.VeriTipi == "string")
            {
                if (!string.IsNullOrWhiteSpace(textDegeri))
                {
                    deger = textDegeri;
                }
            }
            else if (excelSablonKolon.VeriTipi == "integer")
            {
                deger = CheckForInteger(textDegeri, cell.CellReference, excelSablonKolon);
            }
            else if (excelSablonKolon.VeriTipi == "date")
            {
                deger = CheckForDate(textDegeri, cell.CellReference, excelSablonKolon);
            }
            else if (excelSablonKolon.VeriTipi == "decimal")
            {
                deger = CheckForDecimal(textDegeri, cell.CellReference, excelSablonKolon);
            }
            else if (excelSablonKolon.VeriTipi == "long")
            {
                deger = CheckForLong(textDegeri, cell.CellReference, excelSablonKolon);
            }
            else
            {
                HataMesaji.Add("Veri tipi " + excelSablonKolon.VeriTipi + " için uyarlama yapılmamış.");
            }

            return deger;
        }

        private int? CheckForInteger(string value, string cellReference, ExcelSablonKolon excelSablonKolon)
        {
            int? result = null;

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (int.TryParse(value, out int intValue))
                {
                    result = intValue;
                }
                else
                {
                    HataMesajiYonet("TamSayiGecerliDegil", cellReference);
                }
            }
            else
            {
                if (excelSablonKolon.Zorunlu)
                {
                    HataMesajiYonet(string.Format("{0} hücresinde ({1}) veri bekleniyor", cellReference, excelSablonKolon.Adi), "");
                }
            }

            return result;
        }
        private static Regex regexOnlyNumbers = new Regex("^[0-9]*$", RegexOptions.Compiled);
        private DateTime? CheckForDate(string value, string cellReference, ExcelSablonKolon excelSablonKolon)
        {
            DateTime? result = null;

            if (!string.IsNullOrWhiteSpace(value))
            {
                var isValid = false;

                if (value.Contains(".")) //String tipindeki tarih
                {
                    isValid = DateTime.TryParseExact(value.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                    if (isValid)
                    {
                        result = date;
                    }
                }
                else if (regexOnlyNumbers.IsMatch(value))
                {
                    isValid = true;
                    result = DateTime.FromOADate(Convert.ToInt64(value));
                }

                if (!isValid)
                {
                    HataMesajiYonet("TarihGecerliDegil", cellReference);
                }
            }
            else
            {
                if (excelSablonKolon.Zorunlu)
                {
                    HataMesajiYonet(string.Format("{0} hücresinde ({1}) veri bekleniyor", cellReference, excelSablonKolon.Adi), "");
                }
            }

            return result;
        }
        private decimal? CheckForDecimal(string value, string cellReference, ExcelSablonKolon excelSablonKolon)
        {
            decimal? result = null;

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (decimal.TryParse(value.Replace(".", ","), out decimal dbl))
                {
                    result = dbl;
                }
                else
                {
                    HataMesajiYonet("DecimalGecerliDegil", cellReference);
                }
            }
            else
            {
                if (excelSablonKolon.Zorunlu)
                {
                    HataMesajiYonet(string.Format("{0} hücresinde ({1}) veri bekleniyor", cellReference, excelSablonKolon.Adi), "");
                }
            }

            return result;
        }
        private long? CheckForLong(string value, string cellReference, ExcelSablonKolon excelSablonKolon)
        {
            long? result = null;

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (long.TryParse(value, NumberStyles.Any, null, out long longValue))
                {
                    result = longValue;
                }
                else
                {
                    HataMesajiYonet("Decimal değer değil", cellReference);
                }
            }
            else
            {
                if (excelSablonKolon.Zorunlu)
                {
                    HataMesajiYonet(string.Format("{0} hücresinde ({1}) veri bekleniyor", cellReference, excelSablonKolon.Adi), "");
                }
            }

            return result;
        }

        private void BaslikSiralariveAdlarininKontrolEt(DocumentFormat.OpenXml.Spreadsheet.Row header, ref List<ExcelSablonKolon> kolonlar, ref SharedStringTable sst)
        {
            var cells = header.Elements<Cell>();
            foreach (var excelSablonKolon in kolonlar)
            {
                var cellReferenceValue = excelSablonKolon.ExcelColumnName + 1; //A1,B1,C1,...
                var cell = cells.Where(c => c.CellReference == cellReferenceValue).FirstOrDefault();
                var cellText = string.Empty;

                if (cell == null)
                {
                    HataMesajiYonet("{0} hücresi bulunamadı.", cellReferenceValue);
                }
                else
                {
                    if (cell.DataType != null)
                    {
                        if (cell.DataType.HasValue)
                        {
                            if (cell.DataType.Value == CellValues.SharedString)
                            {
                                cellText = sst.ChildElements[int.Parse(cell.CellValue.Text)].InnerText.Trim();
                            }
                            else if (cell.DataType.Value == CellValues.InlineString)
                            {
                                cellText = sst.ChildElements[int.Parse(cell.CellValue.Text)].InnerText.Trim();
                            }
                        }
                        else
                        {
                            cellText = Convert.ToString(cell.CellValue).Trim();
                        }
                    }
                    else
                    {
                        cellText = Convert.ToString(cell.CellValue).Trim();
                    }
                }

                if (excelSablonKolon.Adi != cellText)
                {
                    HataMesajiYonet(string.Format("Dosyada baslık hatası vardır", excelSablonKolon.Adi, cellReferenceValue), ""); ;
                }
            }
        }

        private void HataMesajiYonet(string eklenecekHataMesaji, string hücreAdresi)
        {
            HataMesaji.Add(string.Format(eklenecekHataMesaji, hücreAdresi));
        }

        #endregion

        #region Export İşlemleri
        public static DataTable ToDataTable<T>(List<T> items, List<string> includedColumns, List<string> excludedColumns, bool allTable = false)
        {
            var dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod().IsVirtual == false).ToArray();
            if (allTable)
            {
                Props = Props.Where(x => x.Name.ToLower().EndsWith("id") == false && x.Name.ToLower().EndsWith("ıd") == false).ToArray();
            }
            if (includedColumns != null && includedColumns.Count > 0)
            {
                Props = Props.Where(x => includedColumns.Contains(x.Name)).ToArray();
            }
            if (excludedColumns != null && excludedColumns.Count > 0)
            {
                Props = Props.Where(x => !excludedColumns.Contains(x.Name)).ToArray();
            }

            foreach (PropertyInfo prop in Props)
            {
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)prop.GetCustomAttribute(typeof(DisplayNameAttribute), false);
                if (displayNameAttribute == null)
                {
                    dataTable.Columns.Add(prop.Name, typeof(string));
                }
                else
                {
                    if (prop.PropertyType.IsEnum)
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else if (Convert.ToBoolean(Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum))
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(bool))
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else if (prop.PropertyType == typeof(IEnumerable<string>))
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, typeof(string));
                    }
                    else
                    {
                        dataTable.Columns.Add(displayNameAttribute.DisplayName, type);
                    }
                }
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                    if (values[i] != null)
                    {
                        if (values[i].GetType().IsEnum == true)
                        {
                            //values[i] = ((Enum)values[i]).GetDisplayName();
                        }
                        else if (values[i].GetType() == typeof(DateTime))
                        {
                            values[i] = ((DateTime)values[i]).ToShortDateString();
                        }
                        else if (values[i].GetType() == typeof(bool))
                        {
                            //values[i] = ((bool)values[i]).GetBoolToEnumValueHelper<EvetHayir>();
                        }
                        else if (values[i].GetType() == typeof(string[]))
                        {
                            values[i] = string.Join(",", (string[])values[i]);
                        }
                    }
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        private static object GetColumnObject<T>(string fieldName, T rowObj)
        {
            // is the object a dictionary?
            if (rowObj is IDictionary<string, object> dict)
            {
                return dict.TryGetValue(fieldName, out object value) ? value : null;
            }

            var myf = rowObj.GetType().GetProperty(fieldName);
            if (myf == null)
                return null;

            var obj = myf.GetValue(rowObj, null);
            return obj;
        }

        public byte[] WriteExcelFile<T>(List<T> items, List<string> includedPropertyNames = null, string sheetName = "Sayfa 1")
        {
            var headerNames = new List<string>();
            if (includedPropertyNames == null)
            {
                includedPropertyNames = new List<string>();
                var Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod().IsVirtual == false)
                    .Where(x => !x.Name.EndsWith("Id"))
                    .ToArray();

                foreach (PropertyInfo prop in Props)
                {
                    includedPropertyNames.Add(prop.Name);
                }
            }

            {
                foreach (var includedPropertyName in includedPropertyNames)
                {
                    var prop = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod().IsVirtual == false)
                        .Where(x => string.Equals(x.Name, includedPropertyName, StringComparison.CurrentCultureIgnoreCase))
                        .FirstOrDefault();

                    var displayNameAttribute = (DisplayNameAttribute)prop.GetCustomAttribute(typeof(DisplayNameAttribute), false);

                    if (displayNameAttribute == null)
                    {
                        headerNames.Add(prop.Name);
                    }
                    else
                    {
                        headerNames.Add(displayNameAttribute.DisplayName);
                    }
                }
            }

            using (var mem = new MemoryStream())
            {
                var workbook = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook
                {
                    Sheets = new Sheets()
                };

                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                var relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                var sheet = new Sheet()
                {
                    Id = relationshipId,
                    SheetId = sheetId,
                    Name = sheetName.Substring(0, Math.Min(sheetName.Length, 31))
                };

                sheets.Append(sheet);

                var headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                foreach (var headerName in headerNames)
                {
                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(headerName)
                    };

                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);

                for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
                {
                    var dsrow = items[rowIndex];

                    //Başlık eklendiği için 2 satırdan başlarız
                    var newRow = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = (uint)(rowIndex + 2) };

                    foreach (var propertyName in includedPropertyNames)
                    {

                        var cellValue = GetColumnObject(propertyName, dsrow);

                        if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        {
                            var cell = new Cell
                            {
                                CellReference = new DocumentFormat.OpenXml.StringValue()
                                {
                                    Value = GetExcelColumnName(includedPropertyNames.IndexOf(propertyName) + 1) + (newRow.RowIndex.Value)
                                }
                            };

                            if (cellValue is Enum)
                            {
                                cell.DataType = CellValues.String;
                                //cell.CellValue = new CellValue(((Enum)cellValue).GetDisplayName());
                            }
                            else
                            {
                                cell.DataType = CellValues.String;
                                cell.CellValue = new CellValue(cellValue.ToString());
                            }

                            newRow.Append(cell);
                        }
                    }

                    sheetData.AppendChild(newRow);
                }

                workbookPart.Workbook.Save();
                workbook.Close();
                return mem.ToArray();

            }
        }

        public byte[] WriteExcelFile(DataTable table, string sheetName = "Sayfa 1", string headerBilgiMesaji = "")
        {
            using (MemoryStream mem = new MemoryStream())
            {
                var workbook = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                var sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                sheets.Append(sheet);

                if (!string.IsNullOrWhiteSpace(headerBilgiMesaji))
                {
                    var headerMesajRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    var columnsHeaderMesaj = new List<string>();
                    columnsHeaderMesaj.Add(headerBilgiMesaji);

                    var cellHeaderMesaj = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(headerBilgiMesaji)
                    };
                    headerMesajRow.AppendChild(cellHeaderMesaj);
                    sheetData.AppendChild(headerMesajRow);
                }

                var headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                var columns = new List<string>();

                foreach (DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);

                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(column.ColumnName)
                    };
                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);

                for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    var dsrow = table.Rows[rowIndex];

                    //Başlık eklendiği için 2 satırdan başlarız (Eğer bilgi mesajı varsa 3. satırda başlar. Bilgi Mesajı tek satırlık alan kaplamalıdır.)
                    var newRow = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = (uint)(rowIndex + (!string.IsNullOrWhiteSpace(headerBilgiMesaji) ? 3 : 2)) };

                    for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                    {
                        var cellValue = dsrow[columnIndex];

                        if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        {
                            var cell = new Cell
                            {
                                DataType = GetCellValueDataType(cellValue),
                                CellValue = new CellValue(dsrow[columnIndex].ToString()),
                                CellReference = new DocumentFormat.OpenXml.StringValue()
                                {
                                    Value = GetExcelColumnName(columnIndex + 1) + (newRow.RowIndex.Value)
                                }
                            };

                            newRow.Append(cell);
                        }
                    }

                    sheetData.AppendChild(newRow);
                }

                workbookPart.Workbook.Save();
                workbook.Close();
                return mem.ToArray();

            }
        }

        private static CellValues GetCellValueDataType(object value)
        {
            var result = CellValues.String;

            //if (value is DateTime || value is DateTime?)
            //{
            //    result = CellValues.Date;
            //}
            //else if (value is int || value is int?)
            //{
            //    result = CellValues.Number;
            //}
            //else 

            //if (value is decimal || value is decimal?)
            //{
            //    result = CellValues.Number;
            //}

            return result;
        }

        public byte[] WriteExcelFileSheetList(IEnumerable<ExcelSheet> excelSheet)
        {
            using (var mem = new MemoryStream())
            {
                var workbook = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook
                {
                    Sheets = new Sheets()
                };

                foreach (var sheetItem in excelSheet)
                {
                    var items = sheetItem.Data.ToList();

                    if (items == null || !items.Any())
                    {
                        continue;
                    }

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    sheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();

                    uint sheetId = 1;

                    if (sheets.Elements<Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    var sheet = new Sheet()
                    {
                        Id = workbook.WorkbookPart.GetIdOfPart(sheetPart),
                        SheetId = sheetId,
                        Name = sheetItem.Name.Substring(0, Math.Min(sheetItem.Name.Length, 31))
                    };

                    sheets.Append(sheet);

                    var itemType = sheetItem.Data.FirstOrDefault().GetType();
                    var headers = new List<ExcelHeader>();
                    if (sheetItem.IncludedPropertyNames == null)
                    {
                        sheetItem.IncludedPropertyNames = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod().IsVirtual == false)
                              .Where(x => !x.Name.EndsWith("Id"))
                              .Select(x => x.Name)
                              .ToList();
                    }

                    if (sheetItem.ExcludedPropertyNames != null)
                    {
                        sheetItem.IncludedPropertyNames = sheetItem.IncludedPropertyNames.Except(sheetItem.ExcludedPropertyNames).ToList();
                    }
                    {
                        foreach (var includedPropertyName in sheetItem.IncludedPropertyNames)
                        {
                            var prop = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetGetMethod().IsVirtual == false)
                                .Where(x => string.Equals(x.Name, includedPropertyName, StringComparison.CurrentCultureIgnoreCase))
                                .FirstOrDefault();

                            var header = new ExcelHeader
                            {
                                PropertyName = prop.Name
                            };

                            var displayNameAttribute = (DisplayNameAttribute)prop.GetCustomAttribute(typeof(DisplayNameAttribute), false);
                            header.HeaderName = displayNameAttribute == null ? prop.Name : displayNameAttribute.DisplayName;

                            var displayFormatAttribute = (DisplayFormatAttribute)prop.GetCustomAttribute(typeof(DisplayFormatAttribute), false);
                            header.Format = displayFormatAttribute == null ? string.Empty : displayFormatAttribute.DataFormatString;

                            headers.Add(header);
                        }
                    }

                    var headerRow = new Row();
                    foreach (var header in headers)
                    {
                        var cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(header.HeaderName)
                        };

                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
                    {
                        var dsrow = items[rowIndex];

                        //Başlık eklendiği için 2. satırdan başla
                        var newRow = new Row() { RowIndex = (uint)(rowIndex + 2) };

                        foreach (var header in headers)
                        {
                            var cellValue = GetColumnObject(header.PropertyName, dsrow);

                            if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                            {
                                var cell = new Cell
                                {
                                    CellReference = new DocumentFormat.OpenXml.StringValue()
                                    {
                                        Value = GetExcelColumnName(sheetItem.IncludedPropertyNames.IndexOf(header.PropertyName) + 1) + (newRow.RowIndex.Value)
                                    }
                                };

                                if (cellValue is Enum @enum)
                                {
                                    cell.DataType = CellValues.String;
                                    //cell.CellValue = new CellValue(@enum.GetDisplayName());
                                }
                                else
                                {
                                    cell.DataType = CellValues.String;
                                    cell.CellValue = new CellValue(string.IsNullOrEmpty(header.Format) ? cellValue.ToString() : string.Format(header.Format, cellValue));
                                }

                                newRow.Append(cell);
                            }
                        }

                        sheetData.AppendChild(newRow);
                    }

                    sheetItem.IncludedPropertyNames = null;
                }

                workbookPart.Workbook.Save();
                workbook.Close();
                return mem.ToArray();
            }
        }

        #endregion
    }

    public class ExcelSheet
    {
        public string Name { get; set; } = "Sayfa 1";
        public IEnumerable<object> Data { get; set; }
        public List<string> IncludedPropertyNames { get; set; }
        public List<string> ExcludedPropertyNames { get; set; }
    }
    public class ExcelHeader
    {
        public string HeaderName { get; set; }
        public string Format { get; set; }
        public string PropertyName { get; set; }
    }

    internal class ExcelSablonKolon
    {
        public int SiraNumarasi { get; set; }
        public string Adi { get; set; }
        public string DinamikPropertyName { get; set; }
        public string VeriTipi { get; set; }
        public bool Zorunlu { get; set; }
        public string ExcelColumnName { get; set; }
    }


    public class FileExt : ValidationAttribute
    {
        public string Allow;
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string extension = ((System.Web.HttpPostedFileBase)value).FileName.Split('.')[1];
                if (Allow.Contains(extension))
                    return ValidationResult.Success;
                else
                    return new ValidationResult(ErrorMessage);
            }
            else
                return ValidationResult.Success;
        }
    }


}