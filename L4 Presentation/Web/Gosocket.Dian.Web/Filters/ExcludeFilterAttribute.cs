using System;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Filters
{
    public class ExcludeFilterAttribute : FilterAttribute
    {
        private Type filterType;

        public ExcludeFilterAttribute(Type filterType)
        {
            this.filterType = filterType;
        }

        public Type FilterType
        {
            get
            {
                return this.filterType;
            }
        }
    }
}