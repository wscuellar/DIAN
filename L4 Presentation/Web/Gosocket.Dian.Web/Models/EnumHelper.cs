using System;
using System.ComponentModel;
using System.Reflection;

namespace Gosocket.Dian.Web.Models
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<T>(T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
              (DescriptionAttribute[])fi.GetCustomAttributes
              (typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }

    [Flags]
    public enum Rol
    {
        Administrador = 1,
        Facturador = 2,
        Proveedor = 3,
        [Description("Administrador")]
        ADMIN = 0,
        //[Description("Facturador")]
        //BILLER = 1,
        //[Description("Proveedor")]
        //SUPPLIER = 2,
    }
}