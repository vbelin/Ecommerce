﻿using System;
using System.Collections.Generic;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Entities.Discounts;
using NHibernate;
using System.Linq;

namespace MrCMS.Web.Apps.Ecommerce.Services.Discounts
{
    public class DiscountManager : IDiscountManager
    {
        private readonly ISession _session;

        public DiscountManager(ISession session)
        {
            _session = session;
        }

        public IList<Discount> GetAll()
        {
            return _session.QueryOver<Discount>().CacheMode(CacheMode.Refresh).List();
        }

        public Discount Get(int discountId)
        {
            return _session.QueryOver<Discount>().Where(x => x.Id == discountId).Cacheable().SingleOrDefault();
        }

        public void Add(Discount discount)
        {
            _session.Transact(
                session =>
                session.Save(discount));
        }

        public void Save(Discount discount, DiscountLimitation discountLimitation, DiscountApplication discountApplication)
        {
            Discount oldDiscount = Get(discount.Id);
            DiscountLimitation oldDiscountLimitation = oldDiscount.Limitation;
            if (discountLimitation != null && oldDiscountLimitation != null)
            {
                if (oldDiscount.Limitation.GetType() != discountLimitation.GetType())
                {
                    DeleteOldLimitationOrApplication(oldDiscount, oldDiscountLimitation);
                    oldDiscount.Limitation = discountLimitation;
                }
                else
                {
                    oldDiscount.Limitation.CopyValues(discountLimitation);
                };
            }
            else if (discountLimitation == null && oldDiscountLimitation != null)
            {
                DeleteOldLimitationOrApplication(oldDiscount, oldDiscountLimitation);
                oldDiscount.Limitation = discountLimitation;
            }
            else
            {
                oldDiscount.Limitation = discountLimitation;
            }

            DiscountApplication oldDiscountApplication = oldDiscount.Application;
            if (discountApplication != null && oldDiscountApplication != null)
            {
                if (oldDiscount.Application.GetType() != discountApplication.GetType())
                {
                    DeleteOldLimitationOrApplication(oldDiscount, oldDiscountApplication);
                    oldDiscount.Application = discountApplication;
                }
                else
                {
                    oldDiscount.Application.CopyValues(discountApplication);
                };
            }
            else if (discountApplication == null && oldDiscountApplication != null)
            {
                DeleteOldLimitationOrApplication(oldDiscount, oldDiscountApplication);
                oldDiscount.Application = discountApplication;
            }
            else
            {
                oldDiscount.Application = discountApplication;
            }

            _session.Transact(session => session.SaveOrUpdate(discount));
        }

        private void DeleteOldLimitationOrApplication(Discount oldDiscount, object oldLimitationOrApplication)
        {
            oldDiscount.Limitation = null;
            _session.Transact(session => session.SaveOrUpdate(oldDiscount));
            _session.Transact(session => session.Delete(oldLimitationOrApplication));
        }

        public void Delete(Discount discount)
        {
            _session.Transact(session => 
                {
                    session.Delete(discount);
                    if (discount.Limitation != null)
                        session.Delete(discount.Limitation);
                    if (discount.Application != null)
                        session.Delete(discount.Application);
                });
        }

        public DiscountApplication GetApplication(Discount discount, string applicationType)
        {
            var application =
                TypeHelper.GetAllConcreteMappedClassesAssignableFrom<DiscountApplication>()
                          .FirstOrDefault(type => type.FullName == applicationType);

            return discount.Application != null && discount.Application.GetType() == application
                       ? discount.Application
                       : Activator.CreateInstance(application) as DiscountApplication;
        }

        public DiscountLimitation GetLimitation(Discount discount, string limitationType)
        {
            if (string.IsNullOrWhiteSpace(limitationType))
                return null;

            var limitation =
                TypeHelper.GetAllConcreteMappedClassesAssignableFrom<DiscountLimitation>()
                          .FirstOrDefault(type => type.FullName == limitationType);

            return discount.Limitation != null && discount.Limitation.GetType() == limitation
                       ? discount.Limitation
                       : Activator.CreateInstance(limitation) as DiscountLimitation;
        }
    }
}