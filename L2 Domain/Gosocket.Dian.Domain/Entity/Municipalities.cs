using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class Municipalities : TableEntity
    {
        public Municipalities()
        {

        }

        public Municipalities(string pk, string rk) : base(pk, rk)
        {

        }

#pragma warning disable CS0108 // 'MunicipalityByCode.Timestamp' oculta el miembro heredado 'TableEntity.Timestamp'. Use la palabra clave new si su intención era ocultarlo.
        public DateTime Timestamp { get; set; }
#pragma warning restore CS0108 // 'MunicipalityByCode.Timestamp' oculta el miembro heredado 'TableEntity.Timestamp'. Use la palabra clave new si su intención era ocultarlo.
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
