﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain;
using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Gosocket.Dian.Web.Common;
using System.Linq;
using System;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using System.Diagnostics;
using System.Data.Entity;
using Gosocket.Dian.Application.Managers;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        private readonly IContributorService _ContributorService;
        private readonly IRadianContributorService _RadianContributorService;
        private readonly UserService userService = new UserService();
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private readonly RadianTestSetResultManager radianTestSetManager = new RadianTestSetResultManager();


        public RadianController(IContributorService contributorService, IRadianContributorService radianContributorService)
        {

            _ContributorService = contributorService;
            _RadianContributorService = radianContributorService;
        }


        private void SetContributorInfo()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = _ContributorService.GetByCode(userCode);
            if (contributor == null) return;

            ViewBag.ContributorId = contributor.Id;
            ViewBag.ContributorTypeId = contributor.ContributorTypeId;
            ViewBag.Active = contributor.Status;
            ViewBag.WithSoft = contributor.Softwares?.Count > 0;

            List<Domain.RadianContributor> radianContributor = _RadianContributorService.List(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado");
            string rcontributorTypes = radianContributor?.Aggregate("", (current, next) => current + ", " + next.RadianContributorTypeId.ToString());
            ViewBag.ExistInRadian = rcontributorTypes;
        }
        
        // GET: Radian
        public ActionResult Index()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult AdminRadianView()
        {
            var radianContributors = _RadianContributorService.List(t => true);
            var radianContributorType = _RadianContributorService.GetRadianContributorTypes(t => true);
            var model = new AdminRadianViewModel();
            model.RadianContributors = radianContributors.Select(c =>
            new RadianContributorsViewModel()
            {
                Id = c.Contributor.Id,
                Code = c.Contributor.Code,
                TradeName = c.Contributor.Name,
                BusinessName = c.Contributor.BusinessName,
                AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name

            }).ToList();

            model.RadianType = radianContributorType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name

            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult AdminRadianView(AdminRadianViewModel model)
        {
            var radianContributorType = _RadianContributorService.GetRadianContributorTypes(t => true);
            DateTime? startDate = string.IsNullOrEmpty(model.StartDate) ? null : (DateTime?)Convert.ToDateTime(model.StartDate).Date;
            DateTime? endDate = string.IsNullOrEmpty(model.EndDate) ? null : (DateTime?)Convert.ToDateTime(model.EndDate).Date;
            var radianContributors = _RadianContributorService.List(t => 
            (t.Contributor.Code == model.Code || model.Code == null) && 
            (t.RadianContributorTypeId == model.Type || model.Type == 0) && 
            ( t.RadianState == model.RadianState.ToString() || model.RadianState == null) &&
            (DbFunctions.TruncateTime(t.CreatedDate) >= startDate || !startDate.HasValue) && 
            (DbFunctions.TruncateTime(t.CreatedDate) <= endDate || !endDate.HasValue), 
            model.Page, model.Length);
            
            model.RadianType = radianContributorType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name

            }).ToList();

            model.RadianContributors = radianContributors.Select(c => new RadianContributorsViewModel
            {
                Id = c.Contributor.Id,
                Code = c.Contributor.Code,
                TradeName = c.Contributor.Name,
                BusinessName = c.Contributor.BusinessName,
                AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name

            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        public ActionResult ViewDetails(int id)
        {

            var radianContributor = _RadianContributorService.List(t => t.ContributorId == id);
            var userIds = _ContributorService.GetUserContributors(id).Select(u => u.UserId);
            var testSet = radianTestSetManager.GetAllTestSetResultByContributor(id);
            //RadianContributor contributor = contributorService.ObsoleteGet(id);

            //var userIds = contributorService.GetUserContributors(id).Select(u => u.UserId);
            //var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
            ////var result = contributorOperationsService.GetContributorOperations(id);
            //var contributorOperations = contributorOperationsService.GetContributorOperations(id).Where(co => !co.Deleted).Select(co => new ContributorOperationsViewModel
            //{
            //    OperationModeId = co.OperationModeId,
            //    Software = co.Software == null ? new SoftwareViewModel() : new SoftwareViewModel { AcceptanceStatusSoftwareId = co.Software.AcceptanceStatusSoftwareId }
            //}).ToList();


            var model = new RadianContributorsViewModel
            {
                Id = radianContributor[0].Id,
                Code = radianContributor[0].Contributor.Code,
                TradeName = radianContributor[0].Contributor.Name,
                BusinessName = radianContributor[0].Contributor.BusinessName,
                Email = radianContributor[0].Contributor.Email,
                ContributorTypeName = radianContributor[0].Contributor.ContributorTypeId != null ? radianContributor[0].Contributor.ContributorType.Name : "",
                AcceptanceStatusId = radianContributor[0].Contributor.AcceptanceStatusId,
                AcceptanceStatusName = radianContributor[0].Contributor.AcceptanceStatus.Name,
                CreatedDate = radianContributor[0].CreatedDate,
                UpdatedDate = radianContributor[0].Update,
                RadianContributorFiles = radianContributor[0].RadianContributorFile.Count > 0 ? radianContributor[0].RadianContributorFile.Select(f => new RadianContributorFileViewModel
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    ContributorFileStatus = new RadianContributorFileStatusViewModel
                    {
                        Id = f.RadianContributorFileStatus.Id,
                        Name = f.RadianContributorFileStatus.Name,
                    },
                    ContributorFileType = new ContributorFileTypeViewModel
                    {
                        Id = f.RadianContributorFileType.Id,
                        Mandatory = f.RadianContributorFileType.Mandatory,
                        Name = f.RadianContributorFileType.Name,
                        Timestamp = f.RadianContributorFileType.Timestamp,
                        Updated = f.RadianContributorFileType.Updated
                    },
                    CreatedBy = f.CreatedBy,
                    Deleted = f.Deleted,
                    FileName = f.FileName,
                    Timestamp = f.Timestamp,
                    Updated = f.Updated

                }).ToList() : null,
                Users = userService.GetUsers(userIds.ToList()).Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Email = u.Email
                }).ToList(),

            };


            //model.RadianContributorTestSetResults = testsetresults.where(t => !t.deleted).select(t => new testsetresultviewmodel
            //{
            //    id = t.id,
            //    operationmodename = t.operationmodename,
            //    softwareid = t.softwareid,
            //    status = t.status,
            //    statusdescription = domain.common.enumhelper.getenumdescription((testsetstatus)t.status),
            //    totalinvoicesacceptedrequired = t.totalinvoicesacceptedrequired,
            //    totalinvoicesaccepted = t.totalinvoicesaccepted,
            //    totalinvoicesrejected = t.totalinvoicesrejected,
            //    totalcreditnotesacceptedrequired = t.totalcreditnotesacceptedrequired,
            //    totalcreditnotesaccepted = t.totalcreditnotesaccepted,
            //    totalcreditnotesrejected = t.totalcreditnotesrejected,
            //    totaldebitnotesacceptedrequired = t.totaldebitnotesacceptedrequired,
            //    totaldebitnotesaccepted = t.totaldebitnotesaccepted,
            //    totaldebitnotesrejected = t.totaldebitnotesrejected
            //}).tolist();

            //var model = new ContributorViewModel
            //{
            //    Id = contributor.Id,
            //    Code = contributor.Code,
            //    Name = contributor.Name,
            //    BusinessName = contributor.BusinessName,
            //    Email = contributor.Email,
            //    ExchangeEmail = contributor.ExchangeEmail,
            //    ProductionDate = contributor.ProductionDate.HasValue ? contributor.ProductionDate.Value.ToString("dd-MM-yyyy") : "Sin registrar",
            //    PrincipalActivityCode = contributor.PrincipalActivityCode,
            //    ContributorTypeId = contributor.ContributorTypeId != null ? contributor.ContributorTypeId.Value : 0,
            //    OperationModeId = contributor.OperationModeId != null ? contributor.OperationModeId.Value : 0,
            //    ProviderId = contributor.ProviderId != null ? contributor.ProviderId.Value : 0,
            //    AcceptanceStatusId = contributor.AcceptanceStatusId,
            //    AcceptanceStatusName = contributor.AcceptanceStatus.Name,
            //    AcceptanceStatuses = contributorService.GetAcceptanceStatuses().Select(s => new ContributorAcceptanceStatusViewModel
            //    {
            //        Id = s.Id,
            //        Code = s.Code,
            //        Name = s.Name
            //    }).ToList(),
            //    Softwares = contributor.Softwares?.Select(s => new SoftwareViewModel
            //    {
            //        Id = s.Id,
            //        Name = s.Name,
            //        Pin = s.Pin,
            //        SoftwarePassword = s.SoftwarePassword,
            //        SoftwareUser = s.SoftwareUser,
            //        Url = s.Url,
            //        CreatedBy = s.SoftwareUser,
            //        AcceptanceStatusSoftwareId = s.AcceptanceStatusSoftwareId
            //    }).ToList(),
            //    ContributorFiles = contributor.ContributorFiles.Count > 0 ? contributor.ContributorFiles.Select(f => new ContributorFileViewModel
            //    {
            //        Id = f.Id,
            //        Comments = f.Comments,
            //        ContributorFileStatus = new ContributorFileStatusViewModel
            //        {
            //            Id = f.ContributorFileStatus.Id,
            //            Name = f.ContributorFileStatus.Name,
            //        },
            //        ContributorFileType = new ContributorFileTypeViewModel
            //        {
            //            Id = f.ContributorFileType.Id,
            //            Mandatory = f.ContributorFileType.Mandatory,
            //            Name = f.ContributorFileType.Name,
            //            Timestamp = f.ContributorFileType.Timestamp,
            //            Updated = f.ContributorFileType.Updated
            //        },
            //        CreatedBy = f.CreatedBy,
            //        Deleted = f.Deleted,
            //        FileName = f.FileName,
            //        Timestamp = f.Timestamp,
            //        Updated = f.Updated

            //    }).ToList() : null,
            //    Users = userService.GetUsers(userIds.ToList()).Select(u => new UserViewModel
            //    {
            //        Id = u.Id,
            //        Code = u.Code,
            //        Name = u.Name,
            //        Email = u.Email
            //    }).ToList(),
            //    ContributorOperations = contributorOperations
            //};



            return View(model);
        }

        public ActionResult DownloadContributorFile(string code, string fileName)
        {
            try
            {
                string fileNameURL = code + "/" + StringTools.MakeValidFileName(fileName);
                var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
                var result = fileManager.GetBytes("radiancontributor-files", fileNameURL, out string contentType);
                return File(result, contentType, $"{fileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/pdf", $"error");
            }

        }

        //public JsonResult ChangeRadianState(AdminRadianViewModel model)
        //{

        //}

    }
}