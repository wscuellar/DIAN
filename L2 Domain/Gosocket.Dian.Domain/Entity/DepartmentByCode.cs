using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class DepartmentByCode : TableEntity
    {
        public DepartmentByCode()
        {

        }

        public DepartmentByCode(string pk, string rk) : base(pk, rk)
        {

        }

        public string Code { get; set; }
        public string Name { get; set; }
    }
}
