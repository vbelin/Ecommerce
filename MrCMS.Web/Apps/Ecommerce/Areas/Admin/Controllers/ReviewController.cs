﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrCMS.Web.Apps.Ecommerce.ACL;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.ModelBinders;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Models;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.ProductReviews;
using MrCMS.Web.Apps.Ecommerce.Services.ProductReviews;
using MrCMS.Web.Apps.Ecommerce.Settings;
using MrCMS.Website;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;
using MrCMS.Website.Filters;
using NHibernate.Event;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers
{
    public class ReviewController : MrCMSAppAdminController<EcommerceApp>
    {
        private readonly IReviewAdminService _reviewAdminService;
        private readonly IProductReviewUIService _productReviewUIService;

        public ReviewController(IReviewAdminService reviewAdminService, IProductReviewUIService productReviewUIService)
        {
            _reviewAdminService = reviewAdminService;
            _productReviewUIService = productReviewUIService;
        }

        [HttpGet]
        [MrCMSACLRule(typeof(ProductReviewACL), ProductReviewACL.List)]
        public ViewResult Index(ProductReviewSearchQuery searchQuery)
        {
            ViewData["approval-options"] = _reviewAdminService.GetApprovalOptions();
            ViewData["results"] = _reviewAdminService.Search(searchQuery);
            return View(searchQuery);
        }

        [HttpPost]
        [MrCMSACLRule(typeof(ProductReviewACL), ProductReviewACL.Edit)]
        public RedirectToRouteResult Index([IoCModelBinder(typeof(ProductReviewUpdateModelBinder))] ReviewUpdateModel model)
        {
            _reviewAdminService.BulkAction(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [MrCMSACLRule(typeof(ProductReviewACL), ProductReviewACL.Edit)]
        public ViewResult Edit(ProductReview productReview)
        {
            return View(productReview);
        }

        [ActionName("Edit")]
        [HttpPost]
        [ForceImmediateLuceneUpdate]
        [MrCMSACLRule(typeof(ProductReviewACL), ProductReviewACL.Edit)]
        public ActionResult Edit_POST(ProductReview productReview)
        {
            if (ModelState.IsValid)
            {
                _productReviewUIService.Update(productReview);

                return RedirectToAction("Index");
            }

            return View(productReview);
        }

        [HttpGet]
        public PartialViewResult Delete(ProductReview productReview)
        {
            return PartialView(productReview);
        }

        [ActionName("Delete")]
        [HttpPost]
        [ForceImmediateLuceneUpdate]
        [MrCMSACLRule(typeof(ProductReviewACL), ProductReviewACL.Delete)]
        public RedirectToRouteResult Delete_POST(ProductReview productReview)
        {
            _productReviewUIService.Delete(productReview);
            return RedirectToAction("Index");
        }

        public ViewResult Show(ProductReview productReview)
        {
            return View(productReview);
        }

        [HttpPost]
        public RedirectToRouteResult Approval([IoCModelBinder(typeof(ProductReviewApprovalModelBinder))]ProductReview productReview)
        {
            _productReviewUIService.Update(productReview);
            return RedirectToAction("Index");
        }
    }
}