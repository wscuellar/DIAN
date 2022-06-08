using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Web.Services.Filters;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

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
        Task<ExchangeEmailResponse>  GetExchangeEmails();

        /// <summary>
        /// Obtener status de validadación de un documento mediante trackId.
        /// </summary>
        /// <param name="trackId" type="string"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        Task<DianResponse> GetStatus(string trackId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        Task<List<DianResponse>> GetStatusZip(string trackId);

        /// <summary>
        /// Obtener los eventos asociados a una factura por medio del trackId.
        /// </summary>
        /// <param name="trackId" type="string"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        DianResponse GetStatusEvent(string trackId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract(Name = "SendBillAsync")]
        [CustomOperation]
        Task<UploadDocumentResponse> SendBillAsync(string fileName, byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="testSetId"></param>
        /// <returns></returns>
        [OperationContract(Name = "SendTestSetAsync")]
        [CustomOperation]
        Task<UploadDocumentResponse> SendTestSetAsync(string fileName, byte[] contentFile, string testSetId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]        
        [CustomOperation]
        Task<DianResponse> SendBillSync(string fileName, byte[] contentFile);

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
        Task<DianResponse> SendEventUpdateStatus(byte[] contentFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        Task<DianResponse> SendNominaSync(byte[] contentFile);

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
        Task<EventResponse> GetXmlByDocumentKey(string trackId);

        [OperationContract]
        [CustomOperation]
        DocIdentifierWithEventsResponse GetDocIdentifierWithEvents(string contributorCode, string dateNumber);

        /// <summary>
        /// Genera la solicitud de descarga de documentos emitidos/recibidos de facturas electrónicas
        /// </summary>
        /// <param name="nit" type="string"></param>
        /// <param name="startDate" type="DateTime"></param>
        /// <param name="endDate" type="DateTime"></param>
        /// <param name="documentGroup" type="string"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]   
        DianResponse BulkDocumentDownloadAsync(string nit, string startDate, string endDate, string documentGroup);

        /// <summary>
        /// Consulta el estado de una solicitud de descarga de documentos emitidos/recibidos de facturas electrónicas
        /// </summary>
        /// <param name="trackId" type="string"></param>
        /// <returns></returns>
        [OperationContract]
        [CustomOperation]
        DianResponseBulkDocumentDownload GetStatusBulkDocumentDownload(string trackId);
    }
}