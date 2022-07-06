using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Text;

namespace Functions.Common.Helpers
{
    public static class HttpErrorHelper
    {
        /// <summary>
        /// Método auxiliar para montar o resultado com código http
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>Objeto de resultado com mensagem de erro</returns>
        public static ContentResult GetExceptionResult(this Exception e)
        {
            e.GetHttpErrorStatus(out HttpStatusCode statusCode);

            string message = JsonConvert.SerializeObject(((e.InnerException ?? e).Message));
            message = message.RemoveDiacritics();

            return new ContentResult { StatusCode = (int)statusCode, Content = message };
        }

        /// <summary>
        /// Método auxiliar para retornar erros http
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="statusCode"></param>
        /// <returns>Se verdadeiro, significa que é um erro interno não tratado</returns>
        public static bool GetHttpErrorStatus(this Exception ex, out HttpStatusCode statusCode)
        {
            bool isInternal = false;

            if (ex.InnerException is KeyNotFoundException || ex is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (ex is FunctionInvocationException && ex.InnerException == null)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                isInternal = true;
            }

            return isInternal;
        }

        public static string RemoveDiacritics(this string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
