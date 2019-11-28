using System;
using System.Linq;
using System.Data.Entity.Validation;

namespace Gosocket.Dian.Services.Utils.Common
{
    public static class ExceptionExtensions
    {
        public static string ToStringMessage(this Exception exception)
        {
            if (!(exception is DbEntityValidationException))
                return ExceptionExtensions.Details(exception);
            string seed = ExceptionExtensions.Details(exception);
            foreach (DbEntityValidationResult entityValidationError in ((DbEntityValidationException)exception).EntityValidationErrors)
            {
                seed = seed + " --> " + string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", (object)entityValidationError.Entry.Entity.GetType().Name, (object)entityValidationError.Entry.State);
                seed = entityValidationError.ValidationErrors.Aggregate<DbValidationError, string>(seed, (Func<string, DbValidationError, string>)((current, ve) => current + " --> " + string.Format("- Property: \"{0}\", Error: \"{1}\"", (object)ve.PropertyName, (object)ve.ErrorMessage)));
            }
            return seed;
        }

        private static string Details(Exception exception)
        {
            string str = "";
            try
            {
                int num = 0;
                while (exception != null)
                {
                    str = str + "*** Exception Level " + (object)num + " ***\n";
                    str = str + "Message: " + exception.Message + "\n";
                    str = str + "Source: " + exception.Source + "\n";
                    str = str + "Target Site: " + (object)exception.TargetSite + "\n";
                    str = str + "Stack Trace: " + exception.StackTrace + "\n";
                    str += "***\n\n";
                    exception = exception.InnerException;
                    ++num;
                }
            }
            catch (Exception ex)
            {
            }
            return str;
        }
    }
}
