﻿using CarRental.Business.Abstract;
using CarRental.Business.ValidationRules.FluentValidation;
using CarRental.DataAccess.Abstract;
using CarRental.DataAccess.Concrete;
using CarRental.Entities.Concrete;
using Core.Aspects.Autofac.Validation;
using Core.Entities.Concrete;
using Core.Utilities.Business;
using Core.Utilities.Results.Abstract;
using Core.Utilities.Results.Concrete;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Business.Concrete
{
    //public class CustomerManager : ManagerBase, ICustomerService
    public class CustomerManager : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        public CustomerManager(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [ValidationAspect(typeof(CustomerValidator))]
        public async Task<IDataResult<Customer>> AddAsync(Customer entity)
        {
            var userIdStatus = await _customerRepository.AnyAsync(x => x.UserId == entity.UserId);
            if (userIdStatus)
            {
                return new ErrorDataResult<Customer>("Zaten bir müşteri hesabınız bulunmaktadır.");
            }
            var addedUser = await _customerRepository.AddAsync(entity);
            return new SuccessDataResult<Customer>(entity);
        }

        public async Task<IResult> HardDeleteAsync(Customer entity)
        {
            var result = await _customerRepository.AnyAsync(x => x.Id == entity.Id);
            if (result)
            {
                await _customerRepository.DeleteAsync(entity);
                return new SuccessResult();
            }
            return new ErrorResult("Kullanıcı bulunamadı.");
        }

        public async Task<IDataResult<IList<Customer>>> GetAllAsync()
        {
            var list = await _customerRepository.GetAllAsync();
            if (list.Count >= 1)
            {
                return new SuccessDataResult<IList<Customer>>(list);
            }
            return new ErrorDataResult<IList<Customer>>("Kayıtlı müşteri bulunmamaktadır.");
        }

        public async Task<IDataResult<Customer>> GetByIdWithAddressesAsync(int customerId)
        {
            var result = BusinessRules.Run(await CheckCustomerExistById(customerId));
            if (result != null)
                return new ErrorDataResult<Customer>(result.Message);
            List<Expression<Func<Customer, object>>> includes = new();
            includes.Add(x => x.Addresses);
            var customer = await _customerRepository.GetAsync(x => x.Id == customerId, includes);
            if (customer == null)
            {
                return new ErrorDataResult<Customer>("Verilen parametrede bir müşteri bulunamadı.");
            }
            return new SuccessDataResult<Customer>(customer);
        }

        public async Task<IDataResult<Customer>> UpdateAsync(Customer entity)
        {
            var updatedCustomer = await _customerRepository.UpdateAsync(entity);
            if (updatedCustomer == null)
            {
                return new ErrorDataResult<Customer>("Bir hata oluştu.");
            }
            return new SuccessDataResult<Customer>(updatedCustomer);
        }

        public async Task<IDataResult<Customer>> GetByUserIdWithAddressesAndUserAsync(int userId)
        {
            var result = BusinessRules.Run(await CheckCustomerExistByUserId(userId));
            if (result != null)
                return new ErrorDataResult<Customer>(result.Message);
            List<Expression<Func<Customer, object>>> includes = new();
            includes.Add(x => x.User);
            includes.Add(x => x.Addresses);
            var customer = await _customerRepository.GetAsync(x=>x.UserId == userId, includes);
            return new SuccessDataResult<Customer>(customer);
        }

        private async Task<IResult> CheckCustomerExistByUserId(int userId)
        {
            var result = await _customerRepository.AnyAsync(x=>x.UserId == userId);
            if (!result)
                return new ErrorResult($"Verilen parametrede {userId} değer bulunamadı!");

            return new SuccessResult();
        }

        private async Task<IResult> CheckCustomerExistById(int customerId)
        {
            var result = await _customerRepository.AnyAsync(x => x.Id == customerId);
            if (!result)
                return new ErrorResult($"Verilen parametrede {customerId} değer bulunamadı!");

            return new SuccessResult();
        }

        #region UnitOfWork
        //public CustomerManager(IUnitOfWork unitOfWork) : base(unitOfWork)
        //{
        //}

        //public async Task<IDataResult<Customer>> AddAsync(Customer entity)
        //{
        //    var userIdStatus = await UnitOfWork.Users.AnyAsync(x => x.Id == entity.UserId);
        //    if (userIdStatus)
        //    {
        //        return new ErrorDataResult<Customer>("Zaten bir müşteri hesabınız bulunmaktadır.");
        //    }
        //    var addedUser = await UnitOfWork.Customers.AddAsync(entity);
        //    return new SuccessDataResult<Customer>(entity);
        //}

        //public async Task<IResult> HardDeleteAsync(Customer entity)
        //{
        //    var result = await UnitOfWork.Customers.AnyAsync(x => x.Id == entity.Id);
        //    if (result)
        //    {
        //        await UnitOfWork.Customers.DeleteAsync(entity);
        //        return new SuccessResult();
        //    }
        //    return new ErrorResult("Kullanıcı bulunamadı.");
        //}

        //public async Task<IDataResult<IList<Customer>>> GetAllAsync()
        //{
        //    var list = await UnitOfWork.Customers.GetAllAsync();
        //    if (list.Count >= 1)
        //    {
        //        return new SuccessDataResult<IList<Customer>>(list);
        //    }
        //    return new ErrorDataResult<IList<Customer>>("Kayıtlı müşteri bulunmamaktadır.");
        //}

        //public async Task<IDataResult<Customer>> GetByIdAsync(int customerId)
        //{
        //    var customer = await UnitOfWork.Customers.GetAsync(x => x.Id == customerId);
        //    if (customer == null)
        //    {
        //        return new ErrorDataResult<Customer>("Verilen parametrede bir müşteri bulunamadı.");
        //    }
        //    return new SuccessDataResult<Customer>(customer);
        //}

        //public async Task<IDataResult<Customer>> UpdateAsync(Customer entity)
        //{
        //    var updatedCustomer = await UnitOfWork.Customers.UpdateAsync(entity);
        //    if (updatedCustomer == null)
        //    {
        //        return new ErrorDataResult<Customer>("Bir hata oluştu.");
        //    }
        //    return new SuccessDataResult<Customer>(updatedCustomer);
        //}
        #endregion
    }
}
