using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Utils;

namespace Gosocket.Dian.Application.FreeBiller
{
    public class ClaimsDbService
    {
        SqlDBContext sqlDBContext;

        public ClaimsDbService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        /// <summary>
        /// Obtiene los Ids de los usuarios segun el tipo de Claim.
        /// Método especifico para cumplir los requerimientos de la HU 225.
        /// </summary>
        /// <param name="typeClaim">Tipo de claim. Llave de Claim</param>
        /// <returns>List<[AspNetUsers].[Id]></returns>
        public List<ClaimsDb> GetUserIdsByClaimType(string typeClaim) 
        {
            return sqlDBContext.ClaimsDbs.Where(c => c.ClaimType == typeClaim).ToList();
        }

    }
}
