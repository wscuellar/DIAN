using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Gosocket.Dian.Functions.Office
{
    public class Excel
    {
        public string TableName { get; set; }

        public Color HeaderTextColor { get; set; }

        public Color HeaderBgColor { get; set; }

        public ExcelNamedStyleXml HyperLinkStyle { get; set; }

        public Dictionary<string, string> ColumnNames { get; set; }

        public Excel(string headerColorCode = null)
        {
            HeaderTextColor = Color.White;
            //HeaderBgColor = Color.FromArgb(91, 155, 213);
        }

        public byte[] Generate<T>(IList<T> items, List<ExcelHorizontalAlignment> aligments = null, List<bool> hiddenColumns = null, List<Tuple<int, string, string>> hyperLinkColumns = null)
        {
            using (DataSet dataSet=new DataSet())
            {
                ToDataSet(items,dataSet);

                using (ExcelPackage objExcelPackage = new ExcelPackage())
                {
                    var table = dataSet.Tables[0];
                    if (TableName != null)
                        table.TableName = TableName;

                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(table.TableName);
                    


                    objWorksheet.Cells["A1"].LoadFromDataTable(table, true);

                    if (aligments != null)
                        for (int column = 1; column <= aligments.Count; column++)
                            objWorksheet.Column(column).Style.HorizontalAlignment = aligments[column - 1];

                    for (int column = 1; column <= table.Columns.Count; column++)
                        objWorksheet.Column(column).AutoFit();

                    if (hiddenColumns != null)
                        for (int column = 1; column <= hiddenColumns.Count; column++)
                            objWorksheet.Column(column).Hidden = hiddenColumns[column - 1];

                    if (hyperLinkColumns != null)
                    {
                        var styleName = "HyperLinkStyle";
                        HyperLinkStyle = objWorksheet.Workbook.Styles.CreateNamedStyle(styleName);
                        HyperLinkStyle.Style.Font.UnderLine = true;
                        HyperLinkStyle.Style.Font.Color.SetColor(Color.Blue);

                        int row = 1;
                        foreach (var item in hyperLinkColumns)
                        {

                            //var cell = objWorksheet.Cells[row + 1, item.Item1];
                            //cell.Hyperlink = new Uri(item.Item2);
                            //cell.Value = item.Item3;
                            using (ExcelRange rng = objWorksheet.Cells[row + 1, item.Item1, row + 1, item.Item1])
                            {
                                rng.Formula = "HYPERLINK(\"" + item.Item2 + "\" , \"" + item.Item3 + "\")";
                                rng.StyleName = styleName;
                            }
                            row++;
                        }
                    }

                    objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));

                    var reference = $"A1:{ExcelColumnIndexToName((uint)table.Columns.Count - 1)}1";
                    using (ExcelRange objRange = objWorksheet.Cells[reference])
                    {
                        objRange.Style.Font.Bold = true;
                        objRange.Style.Font.Color.SetColor(HeaderTextColor);
                        //objRange.Style.Font.Size = 11;
                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        objRange.Style.Fill.BackgroundColor.SetColor(HeaderBgColor);

                    }
                    
                    return objExcelPackage.GetAsByteArray();
                    
                }
            }
        }

        private DataSet ToDataSet<T>(IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                t.Columns.Add(GetDisplayName<T>(propInfo), ColType);
            }

            foreach (T item in list)
            {
                DataRow row = t.NewRow();
                foreach (var propInfo in elementType.GetProperties())
                    row[GetDisplayName<T>(propInfo)] = propInfo.GetValue(item, null) ?? DBNull.Value;
                t.Rows.Add(row);
            }
            return ds;
        }

        private string GetDisplayName<T>(PropertyInfo propInfo)
        {
            if (ColumnNames != null && ColumnNames.ContainsKey(propInfo.Name))
                return ColumnNames[propInfo.Name];

            MemberInfo property = typeof(T).GetProperty(propInfo.Name);
            var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Any()
                ? property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().FirstOrDefault() : null;

            string displayName = null;

            if (attribute != null)
            {
                displayName = attribute.DisplayName;
            }
            else
            {
                var attr = property.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().Any()
                    ? property.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault() : null;
                if (attr != null)
                {
                    ResourceManager rm = new ResourceManager(attr.ResourceType);
                    displayName = rm.GetString(attr.Name);
                }
            }

            return displayName ?? propInfo.Name;
        }

        private string ExcelColumnIndexToName(uint index)
        {
            string range = "";
            for (int i = 1; index + i > 0; i = 0)
            {
                range = (char)(65 + index % 26) + range;
                index /= 26;
            }
            if (range.Length > 1) range = (char)(range[0] - 1) + range.Substring(1);
            return range;
        }

        private void ToDataSet<T>(IList<T> list,DataSet ds)
        {
            Type elementType = typeof(T);

            using (DataTable t = new DataTable())
            {
                ds.Tables.Add(t);

                foreach (var propInfo in elementType.GetProperties())
                {
                    Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                    t.Columns.Add(GetDisplayName<T>(propInfo), ColType);
                }

                foreach (T item in list)
                {
                    DataRow row = t.NewRow();
                    foreach (var propInfo in elementType.GetProperties())
                        row[GetDisplayName<T>(propInfo)] = propInfo.GetValue(item, null) ?? DBNull.Value;
                    t.Rows.Add(row);
                }
            }
            
        }
    }
}
