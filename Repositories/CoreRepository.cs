using BIPL_RAASTP2M.Data;
using BIPL_RAASTP2M.DTO;
using BIPL_RAASTP2M.Models;
using BIPL_RAASTP2M.Services;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using static System.Net.WebRequestMethods;

namespace BIPL_RAASTP2M.Repositories
{
    public class CoreRepository : ICoreRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public CoreRepository(AppDbContext appDbContext, IServiceProvider serviceProvider, IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _appDbContext = appDbContext;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task LogWriteAsync(string Activity, string Description, string Interface,string UserID)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Logs Loggerbody = new Logs
                {
                    UserID = string.IsNullOrWhiteSpace(UserID) ? "System" : UserID,
                    Activity = Activity.Length > 50 ? Activity.Substring(0, 48) : Activity,
                    Description = Description.Length > 500 ? Description.Substring(0, 498) : Description,
                    Interface = Interface.Length > 50 ? Interface.Substring(0, 48) : Interface,
                    Application = "BIPL_RAASTP2M",
                    eDate = DateTime.Now,
                    IPAddress = ""
                };

                try
                {
                    await scopedDbContext.logs.AddAsync(Loggerbody);
                    await scopedDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    //  _logger.LogError("BaseService:LogWriteBase: " + ex.Message);
                }
            }
        }

        public async Task<SystemUsers> GetSyestemUserByUserId(string UserID)
        {
            return await _appDbContext.SystemUsers
                .FirstOrDefaultAsync(x => x.UserID == UserID) ?? new SystemUsers();
        }

        public async Task<Merchants> GetMerchantById(long id)
        {
            return await _appDbContext.Merchants.FirstOrDefaultAsync(x => x.Id == id) ?? new Merchants();
        }

        public async Task<List<DiningTables>> GetDiningTables(long merchantId, string UserID)
        {
            try
            {
                return await _appDbContext.DiningTables
                    .Where(x => x.MerchantId == merchantId&&x.IsDeleted==false)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetDiningTables", ex.Message, "CoreRepository:GetDiningTables", UserID ?? "System");

                return new List<DiningTables>();
            }
        }

        public async Task<bool> AddDiningTableAsync(DiningTables table)
        {
            try
            {
                await _appDbContext.DiningTables.AddAsync(table);
                return (await _appDbContext.SaveChangesAsync()) > 0;

            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-AddDiningTableAsync", ex.Message, "CoreRepository:AddDiningTableAsync","System");
                return false;
            }
        }
        public async Task<bool> UpdateDiningTableAsync(DiningTableDto model,long MerchantId)
        {
            try
            {
                var existing = await _appDbContext.DiningTables
                                    .FirstOrDefaultAsync(x => x.Id == model.Id && x.MerchantId == MerchantId && x.IsDeleted == false);

                if (existing == null)
                    return false;

                // Update fields
                existing.Name = model.Name;

                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdateDiningTableAsync", ex.Message, "CoreRepository:UpdateDiningTableAsync", MerchantId.ToString()??"System");

                return false;
            }
        }

        public async Task<bool> DeleteDiningTableAsync(long id, long merchantId)
        {
            try
            {
                var table = await _appDbContext.DiningTables
                    .FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId && x.IsDeleted == false);

                if (table == null)
                    return false;

                table.IsDeleted = true;

                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-DeleteDiningTableAsync", ex.Message, "CoreRepository:DeleteDiningTableAsync", merchantId.ToString() ?? "System");
                return false;
            }
        }
        public async Task<string> AddCategoryAsync(CategoryDto model, long merchantId)
        {
            try
            {
                // Check duplicate
                var exists = await _appDbContext.Categories
                    .AnyAsync(x => x.MerchantId == merchantId
                                && x.CategoryName.ToLower() == model.CategoryName.ToLower()
                                && x.IsDeleted == false);

                if (exists)
                    return "DUPLICATE";

                var newCategory = new Categories
                {
                    MerchantId = merchantId,
                    CategoryName = model.CategoryName,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _appDbContext.Categories.AddAsync(newCategory);

                return (await _appDbContext.SaveChangesAsync()) > 0 ? "OK" : "FAIL";
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-AddCategoryAsync", ex.Message, "CoreRepository:AddCategoryAsync", merchantId.ToString() ?? "System");
                return "FAIL";
            }
        }
        public async Task<Categories> GetCategoryId(int Id, long merchantId)
        {
            try
            {
                return await _appDbContext.Categories
                    .FirstOrDefaultAsync(x => x.Id == Id && x.MerchantId == merchantId&&x.IsDeleted==false) ?? new Categories();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCategoryId", ex.Message, "CoreRepository:GetCategoryId", merchantId.ToString() ?? "System");

                throw new Exception("Error while checking Category Id: " + ex.Message);
            }
        }
        public async Task<Categories> GetCategoryByName(string CategoryName, long merchantId)
        {
            try
            {
                return await _appDbContext.Categories
                    .FirstOrDefaultAsync(x => x.CategoryName == CategoryName&& x.MerchantId== merchantId&&x.IsDeleted==false) ?? new Categories();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCategoryByName", ex.Message, "CoreRepository:GetCategoryByName", merchantId.ToString() ?? "System");

                throw new Exception("Error while checking Category name: " + ex.Message);
            }
        }
        public async Task<bool> UpdateCategoryAsync(Categories categories)
        {
            try
            {
                _appDbContext.Categories.Update(categories);
                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdatecategoryAsync", ex.Message, "CoreRepository:UpdatecategoryAsync", "System");
                throw new Exception("Error while updating category: " + ex.Message);
            }
        }
        public async Task<List<Categories>> GetCategoriesAsync(long merchantId)
        {
            try
            {
                return await _appDbContext.Categories
                    .Where(x => x.MerchantId == merchantId && x.IsDeleted == false)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCategoriesAsync", ex.Message, "CoreRepository:GetCategoriesAsync", merchantId.ToString() ?? "System");

                return new List<Categories>();
            }
        }
        public async Task<bool> DeleteCategoryAsync(long id, long merchantId)
        {
            try
            {
                var table = await _appDbContext.Categories
                    .FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId && x.IsDeleted == false);

                if (table == null)
                    return false;

                table.IsDeleted = true;

                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-DeleteCategoryAsync", ex.Message, "CoreRepository:DeleteCategoryAsync", merchantId.ToString() ?? "System");
                return false;
            }
        }
        public async Task<bool> GetProductByNameAsync(string name, long merchantId)
        {
            return await _appDbContext.Products.AnyAsync(p => p.MerchantId == merchantId
                                                    && p.ProductName == name
                                                    && p.IsDeleted==false);
        }
        public async Task<bool> AddProductAsync(Products product)
        {
            await _appDbContext.Products.AddAsync(product);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<List<Products>> GetProductsAsync(long merchantId)
        {
            try
            {
                return await _appDbContext.Products
                    .Where(x => x.MerchantId == merchantId && x.IsDeleted == false)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetProductsAsync", ex.Message, "CoreRepository:GetProductsAsync", merchantId.ToString() ?? "System");

                return new List<Products>();
            }
        }

        public async Task<Products> GetProductById(int Id, long merchantId)
        {
            try
            {
                return await _appDbContext.Products
                    .FirstOrDefaultAsync(x => x.Id == Id && x.MerchantId == merchantId && x.IsDeleted == false) ?? new Products();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetProductById", ex.Message, "CoreRepository:GetProductById", merchantId.ToString() ?? "System");

                throw new Exception("Error while checking Category Id: " + ex.Message);
            }
        }
        public async Task<Products> GetProductByName(string ProductName, long merchantId)
        {
            try
            {
                return await _appDbContext.Products
                    .FirstOrDefaultAsync(x => x.ProductName == ProductName && x.MerchantId == merchantId && x.IsDeleted == false) ?? new Products();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetProductByName", ex.Message, "CoreRepository:GetProductByName", merchantId.ToString() ?? "System");

                throw new Exception("Error while checking Product name: " + ex.Message);
            }
        }
        public async Task<bool> UpdateProductAsync(Products products)
        {
            try
            {
                _appDbContext.Products.Update(products);
                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdateProductAsync", ex.Message, "CoreRepository:UpdateProductAsync", "System");
                throw new Exception("Error while updating products: " + ex.Message);
            }
        }
        public async Task<bool> DeleteProductAsync(Products product)
        {
            product.IsDeleted = true;
            product.ImagePath = null;
            product.ImagePublicId = null;

            return (await _appDbContext.SaveChangesAsync()) > 0;
        }

        public async Task<long> AddOrderAsync(Orders order)
        {
            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();
            return order.Id;   // return newly created order id
        }
        public async Task<bool> AddOrderItemsAsync(List<OrderItems> items)
        {
            _appDbContext.OrderItems.AddRange(items);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<List<OrderHistoryResponse>> GetOrderHistoryAsync(
    long merchantId, DateTime fromDate, DateTime toDate)
        {
            var query = _appDbContext.Orders
                .Where(x => x.MerchantId == merchantId &&
                            x.OrderDate >= fromDate &&
                            x.OrderDate <= toDate);

            //if (orderType != "All")
            //{
            //    query = query.Where(x => x.OrderType == orderType);
            //}

            var orders = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var result = new List<OrderHistoryResponse>();

            foreach (var o in orders)
            {
                var items = await (from i in _appDbContext.OrderItems
                                   join p in _appDbContext.Products on i.ProductId equals p.Id
                                   where i.OrderId == o.Id
                                   select new OrderItemResponse
                                   {
                                       ProductName = p.ProductName,
                                       Qty = i.Qty,
                                       UnitPrice = i.UnitPrice,
                                       GrossTotal = i.GrossTotal,
                                       TotalPrice = i.TotalPrice
                                   }).ToListAsync();

                result.Add(new OrderHistoryResponse
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderType = o.OrderType,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    GrossTotal=o.GrossTotal,
                    ItemsCount = o.ItemsCount,
                    TableName = o.TableId.ToString(),
                    Items = items
                });
            }

            return result;
        }

        public async Task<Customers> GetCustomersbyPhoneNumber(long merchantId, string CustomerPhone)
        {
            try
            {
                return await _appDbContext.Customers
                    .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.CustomerPhone == CustomerPhone) ?? new Customers();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCustomersbyPhoneNumber", ex.Message, "CoreRepository:GetCustomersbyPhoneNumber", merchantId.ToString() ?? "System");

                return new Customers();
            }
        }
        public async Task<bool> AddCustomer(Customers customers)
        {
            try
            {
                _appDbContext.Customers.Add(customers);
                return await _appDbContext.SaveChangesAsync() > 0;

            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-AddCustomer", ex.Message, "CoreRepository:AddCustomer", "System");

                return false;
            }
        }

        public async Task<List<Customers>> SearchCustomersAsync(long merchantId, string query)
        {
            return await _appDbContext.Customers
                .Where(x => x.MerchantId == merchantId &&
                       (x.CustomerName.Contains(query) ||
                        x.CustomerPhone.Contains(query)))
                .OrderBy(x => x.CustomerName)
                .Take(20) // suggestions limit
                .ToListAsync();
        }

    }
}