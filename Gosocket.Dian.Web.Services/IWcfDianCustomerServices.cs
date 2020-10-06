using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Web.Services.Filters;
using System.Collections.Generic;
using System.ServiceModel;

namespace Gosocket.Dian.Web.Services
{
    /// <summary>
    /// IWcfDianCustomerServices
    /// </summary>
    [ServiceContract(Namespace = "http://wcf.dian.colombia")]
    public interface IWcfDianCustomerServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        ExchangeEmailResponse GetExchangeEmails();

        /// <summary>
        /// Obtener status de validadación de un documento mediante trackId.
        /// </summary>
        /// <param name="trackId" type="string"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        DianResponse GetStatus(string trackId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        List<DianResponse> GetStatusZip(string trackId);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        UploadDocumentResponse SendBillAsync(string fileName, byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="testSetId"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        UploadDocumentResponse SendTestSetAsync(string fileName, byte[] contentFile, string testSetId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        DianResponse SendBillSync(string fileName, byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        UploadDocumentResponse SendBillAttachmentAsync(string fileName, byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        DianResponse SendEventUpdateStatus(byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        NumberRangeResponseList GetNumberingRange(string accountCode, string accountCodeT, string softwareCode);

        [OperationContract]
        [CustomOperation]
        EventResponse GetXmlByDocumentKey(string trackId);
    }
}
