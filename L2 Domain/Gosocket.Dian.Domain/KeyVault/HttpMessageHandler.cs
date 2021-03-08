using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.KeyVault
{
    public class HttpMessageHandler : DelegatingHandler
    {
        /// <inheritdoc />
        /// <summary>
        /// Adds the Host header to every request if the "KmsNetworkUrl" configuration setting is specified.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(response => response.Result, cancellationToken);
        }
    }
}
