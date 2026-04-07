using TBAppBackend.Data;
using TBAppBackend.DTO;
using TBAppBackend.Models;
using TBAppBackend.Services;
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

namespace TBAppBackend.Repositories
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
                    Application = "TBAppBackend",
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
        public async Task<bool> GetDiningTableByNameAsync(string name, long merchantId)
        {
            return await _appDbContext.DiningTables.AnyAsync(p => p.MerchantId == merchantId
                                                    && p.Name == name
                                                    && p.IsDeleted == false);
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
        public async Task<List<OrderHistoryResponse>> GetOrderHistoryAsyncbk(long merchantId, DateTime fromDate, DateTime toDate)
        {
            var query = _appDbContext.Orders
                .Where(x => x.MerchantId == merchantId &&
                            x.OrderDate >= fromDate &&
                            x.OrderDate <= toDate);

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
        public async Task<List<OrderHistoryResponse>> GetOrderHistoryAsync(
    long merchantId, DateTime fromDate, DateTime toDate)
        {
            var orders = await _appDbContext.Orders
                .Where(x => x.MerchantId == merchantId &&
                            x.OrderDate >= fromDate &&
                            x.OrderDate <= toDate)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var result = new List<OrderHistoryResponse>();

            foreach (var o in orders)
            {
                // ---------- Order Items ----------
                var items = await (from i in _appDbContext.OrderItems
                                   join p in _appDbContext.Products on i.ProductId equals p.Id
                                   where i.OrderId == o.Id
                                   select new OrderItemResponse
                                   {
                                       ProductId= p.Id,
                                       ProductName = p.ProductName,
                                       Qty = i.Qty,
                                       UnitPrice = i.UnitPrice,
                                       GrossTotal = i.GrossTotal,
                                       TotalPrice = i.TotalPrice,

                                       // ADD THESE TWO LINES
                                       DiscountType = i.DiscountType,
                                       DiscountValue = i.DiscountValue
                                   }).ToListAsync();

                // ---------- Customer (ONLY if exists) ----------
                CustomerResponse customer = null;

                if (o.CustomerId.HasValue && o.CustomerId > 0)
                {
                    customer = await _appDbContext.Customers
                        .Where(c => c.CustomerId == o.CustomerId.Value)
                        .Select(c => new CustomerResponse
                        {
                            CustomerId = c.CustomerId,
                            CustomerName = c.CustomerName,
                            CustomerPhone = c.CustomerPhone,
                            DeliveryAddress = c.DeliveryAddress
                        })
                        .FirstOrDefaultAsync();
                }
                // ---------- Dining Table Name ----------
                string tableName = null;
                if (o.TableId.HasValue && o.TableId > 0)
                {
                    tableName = await _appDbContext.DiningTables
                        .Where(t => t.Id == o.TableId.Value)
                        .Select(t => t.Name)
                        .FirstOrDefaultAsync();
                }
                // ---------- Final Response ----------
                result.Add(new OrderHistoryResponse
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    InvoiceId = o.InvoiceId,
                    OrderType = o.OrderType,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    GrossTotal = o.GrossTotal,
                    ItemsCount = o.ItemsCount,
                    TableName = tableName,
                    UserId = o.UserId,
                    IsRefunded = o.IsRefunded,
                    RefundedBy = o.RefundedBy,
                    RefundedAt = o.RefundedAt,

                    PaymentType = o.PaymentType,

                    // Order level discounts (already there)
                    OrderDiscountType = o.OrderDiscountType,
                    OrderDiscountValue = o.OrderDiscountValue,

                    TaxType = o.TaxType,
                    TaxValue = o.TaxValue,
                    TaxAmount = o.TaxAmount,

                    Customer = customer,   // 👈 customer only when exists
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
        public async Task<Customers> GetCustomersbyCustomerId(long merchantId, long CustomerId)
        {
            try
            {
                return await _appDbContext.Customers
                    .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.CustomerId == CustomerId) ?? new Customers();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCustomersbyCustomerId", ex.Message, "CoreRepository:GetCustomersbyCustomerId", merchantId.ToString() ?? "System");

                return new Customers();
            }
        }
        public async Task<bool> UpdatCustomersAsync(Customers customers)
        {
            try
            {
                _appDbContext.Customers.Update(customers);
                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdatCustomersAsync", ex.Message, "CoreRepository:UpdatCustomersAsync", "System");

                return false;
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
        public async Task<Orders> GetOrderByIdAsync(long orderId, long merchantId)
        {
            try { 
            return await _appDbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId && x.MerchantId == merchantId) ?? new Orders();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetOrderByIdAsync", ex.Message, "CoreRepository:GetOrderByIdAsync", merchantId.ToString() ?? "System");

                throw new Exception("Error while checking GetOrderById: " + ex.Message);
            }
        }
        public async Task<bool> UpdateOrderAsync(Orders order)
        {
            try { 
            _appDbContext.Orders.Update(order);
            return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdateOrderAsync", ex.Message, "CoreRepository:UpdateOrderAsync", "System");

                return false;
            }
        }


        // reports/summary start =====

        public async Task<KpiDto> GetKpiDataAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return new KpiDto
                    {
                        TotalOrders = 0,
                        TotalItemsSold = 0,
                        TotalRevenue = 0,
                        TotalOriginalRevenue = 0,
                        TotalDiscount = 0
                    };
                }

                var orderIds = orders.Select(o => o.Id).ToList();

                var totalItems = await _appDbContext.OrderItems
                    .Where(oi => orderIds.Contains(oi.OrderId))
                    .SumAsync(oi => oi.Qty);

                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOriginalRevenue = orders.Sum(o => o.GrossTotal ?? 0);

                // Calculate total discounts
                var itemDiscounts = await _appDbContext.OrderItems
                    .Where(oi => orderIds.Contains(oi.OrderId))
                    .SumAsync(oi => oi.DiscountAmount ?? 0);
                var orderDiscounts = orders.Sum(o => o.TotalDiscount ?? 0);
                var totalDiscount = itemDiscounts + orderDiscounts;

                return new KpiDto
                {
                    TotalOrders = orders.Count,
                    TotalItemsSold = totalItems,
                    TotalRevenue = totalRevenue,
                    TotalOriginalRevenue = totalOriginalRevenue,
                    TotalDiscount = totalDiscount
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetKpiData", ex.Message, "CoreRepository:GetKpiDataAsync", merchantId.ToString());
                return new KpiDto { TotalOrders = 0, TotalItemsSold = 0, TotalRevenue = 0, TotalOriginalRevenue = 0, TotalDiscount = 0 };
            }
        }
        public async Task<List<ProductStatDto>> GetProductStatsAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                // First, get all order items with their products
                var query = from o in _appDbContext.Orders
                            join oi in _appDbContext.OrderItems on o.Id equals oi.OrderId
                            join p in _appDbContext.Products on oi.ProductId equals p.Id
                            where o.MerchantId == merchantId &&
                                  o.OrderDate >= startDate &&
                                  o.OrderDate <= endDate &&
                                  !o.IsRefunded &&
                                  !p.IsDeleted
                            select new { o, oi, p };

                var results = await query.ToListAsync();

                // Group by product manually to calculate stats
                var productGroups = results
                    .GroupBy(x => new { x.p.Id, x.p.ProductName, x.p.ProductPrice })
                    .Select(g => new ProductStatDto
                    {
                        ProductId = g.Key.Id,
                        ProductName = g.Key.ProductName,
                        Quantity = g.Sum(x => x.oi.Qty),
                        Revenue = g.Sum(x => x.oi.TotalPrice),
                        OriginalRevenue = g.Sum(x => x.oi.GrossTotal ?? 0),
                        AveragePrice = g.Key.ProductPrice,

                        // Calculate discount amount properly
                        DiscountAmount = g.Sum(x =>
                            (x.oi.DiscountAmount ?? 0) + // Item-level discounts
                            (GetProportionalOrderDiscount(x.o, x.oi) ?? 0) // Order-level discounts
                        ),

                        // Count items that had any discount
                        ItemsDiscounted = g.Count(x =>
                            x.oi.DiscountAmount > 0 ||
                            (x.o.TotalDiscount > 0 && x.oi.GrossTotal > 0)
                        )
                    })
                    .OrderByDescending(x => x.Quantity)
                    .ToList();

                return productGroups;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetProductStats", ex.Message, "CoreRepository:GetProductStatsAsync", merchantId.ToString());
                return new List<ProductStatDto>();
            }
        }

        // Helper method to calculate proportional order discount for an item
        private decimal? GetProportionalOrderDiscount(Orders order, OrderItems item)
        {
            if (order.TotalDiscount > 0 && order.GrossTotal > 0 && item.GrossTotal > 0)
            {
                var itemGross = item.GrossTotal ?? 0;
                var itemShare = itemGross / order.GrossTotal.Value;
                return Math.Round(order.TotalDiscount.Value * itemShare, 2);
            }
            return 0;
        }

        public async Task<TimeDataDto> GetTimeDataAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1); // End of the selected day (11:59:59.999)

                var totalDays = (toDate.Date - fromDate.Date).Days + 1;
                var isSingleDay = totalDays == 1;

                if (isSingleDay)
                {
                    // Hourly data for single day
                    var orders = await _appDbContext.Orders
                        .Where(o => o.MerchantId == merchantId &&
                                   o.OrderDate >= startDate &&
                                   o.OrderDate <= endDate &&
                                   !o.IsRefunded)
                        .ToListAsync();

                    var hourlyData = new List<TimePointDto>();

                    for (int hour = 0; hour < 24; hour++)
                    {
                        var hourStart = startDate.AddHours(hour);
                        var hourEnd = hourStart.AddHours(1);

                        var hourRevenue = orders
                            .Where(o => o.OrderDate >= hourStart && o.OrderDate < hourEnd)
                            .Sum(o => o.TotalAmount);

                        hourlyData.Add(new TimePointDto
                        {
                            Label = $"{hour}:00",
                            Hour = hour,
                            Value = hourRevenue
                        });
                    }

                    return new TimeDataDto
                    {
                        Type = "hourly",
                        Points = hourlyData
                    };
                }
                else
                {
                    // Daily data for multiple days
                    var dailyData = new List<TimePointDto>();

                    for (int day = 0; day < totalDays; day++)
                    {
                        var currentDate = startDate.AddDays(day);
                        var nextDate = currentDate.AddDays(1);

                        var dayRevenue = await _appDbContext.Orders
                            .Where(o => o.MerchantId == merchantId &&
                                       o.OrderDate >= currentDate &&
                                       o.OrderDate < nextDate &&
                                       !o.IsRefunded)
                            .SumAsync(o => o.TotalAmount);

                        dailyData.Add(new TimePointDto
                        {
                            Label = currentDate.ToString("yyyy-MM-dd"),
                            Date = currentDate,
                            Value = dayRevenue
                        });
                    }

                    return new TimeDataDto
                    {
                        Type = "daily",
                        Points = dailyData
                    };
                }
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetTimeData", ex.Message, "CoreRepository:GetTimeDataAsync", merchantId.ToString());
                return new TimeDataDto { Type = "daily", Points = new List<TimePointDto>() };
            }
        }

        // ==================== NEW REPORT METHODS FOR UPDATED UI ====================

        public async Task<TaxSummaryDto> GetTaxSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded)
                    .ToListAsync();

                var totalTax = orders.Sum(o => o.TaxAmount ?? 0); // Handle null with ?? 0

                var taxByPaymentMethod = orders
                    .Where(o => o.TaxAmount.HasValue && o.TaxAmount > 0)
                    .GroupBy(o => o.PaymentType)
                    .Select(g => new TaxByPaymentMethodDto
                    {
                        PaymentMethod = g.Key,
                        TaxAmount = g.Sum(o => o.TaxAmount ?? 0)
                    })
                    .ToList();

                return new TaxSummaryDto
                {
                    TotalTaxCollected = totalTax,
                    TaxByPaymentMethod = taxByPaymentMethod
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetTaxSummary", ex.Message, "CoreRepository:GetTaxSummaryAsync", merchantId.ToString());
                return new TaxSummaryDto { TotalTaxCollected = 0, TaxByPaymentMethod = new List<TaxByPaymentMethodDto>() };
            }
        }

        public async Task<DiscountSummaryDto> GetDiscountSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded)
                    .ToListAsync();

                var orderIds = orders.Select(o => o.Id).ToList();

                var orderItems = await _appDbContext.OrderItems
                    .Where(oi => orderIds.Contains(oi.OrderId))
                    .ToListAsync();

                var products = await _appDbContext.Products
                    .Where(p => p.MerchantId == merchantId && !p.IsDeleted)
                    .ToDictionaryAsync(p => p.Id);

                // Dictionary to accumulate discounts per product
                var productDiscountDict = new Dictionary<int, ProductDiscountDto>();

                // 1. Handle item-level discounts
                foreach (var item in orderItems.Where(oi => oi.DiscountAmount > 0))
                {
                    var productId = item.ProductId;

                    if (!productDiscountDict.ContainsKey(productId))
                    {
                        productDiscountDict[productId] = new ProductDiscountDto
                        {
                            ProductId = productId,
                            ProductName = products.ContainsKey(productId) ? products[productId].ProductName : $"Product {productId}",
                            DiscountAmount = 0,
                            ItemsDiscounted = 0,
                            AverageDiscountRate = 0
                        };
                    }

                    productDiscountDict[productId].DiscountAmount += item.DiscountAmount ?? 0;
                    productDiscountDict[productId].ItemsDiscounted += item.Qty; // FIXED: Add quantity here
                }

                // 2. Handle order-level discounts (proportionally distribute to items)
                foreach (var order in orders.Where(o => o.TotalDiscount > 0))
                {
                    var items = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                    var orderGrossTotal = order.GrossTotal ?? 1; // Avoid division by zero

                    foreach (var item in items)
                    {
                        if (item.GrossTotal > 0)
                        {
                            var itemShare = item.GrossTotal.Value / orderGrossTotal;
                            var itemOrderDiscount = Math.Round(order.TotalDiscount.Value * itemShare, 2);

                            if (!productDiscountDict.ContainsKey(item.ProductId))
                            {
                                productDiscountDict[item.ProductId] = new ProductDiscountDto
                                {
                                    ProductId = item.ProductId,
                                    ProductName = products.ContainsKey(item.ProductId) ? products[item.ProductId].ProductName : $"Product {item.ProductId}",
                                    DiscountAmount = 0,
                                    ItemsDiscounted = 0,
                                    AverageDiscountRate = 0
                                };
                            }

                            productDiscountDict[item.ProductId].DiscountAmount += itemOrderDiscount;
                            // Only count items for order discounts if they didn't already have item-level discounts
                            var existingItem = orderItems.FirstOrDefault(oi => oi.Id == item.Id);
                            if (existingItem == null || existingItem.DiscountAmount == 0)
                            {
                                productDiscountDict[item.ProductId].ItemsDiscounted += item.Qty;
                            }
                        }
                    }
                }

                // Calculate average discount rates
                foreach (var product in productDiscountDict.Values)
                {
                    var productItems = orderItems.Where(oi => oi.ProductId == product.ProductId && oi.DiscountValue > 0);
                    if (productItems.Any())
                    {
                        product.AverageDiscountRate = Math.Round(productItems.Average(oi => oi.DiscountValue ?? 0), 2);
                    }
                }

                var totalItemDiscounts = orderItems.Sum(oi => oi.DiscountAmount ?? 0);
                var totalOrderDiscounts = orders.Sum(o => o.TotalDiscount ?? 0);
                var totalDiscount = totalItemDiscounts + totalOrderDiscounts;

                return new DiscountSummaryDto
                {
                    TotalDiscount = totalDiscount,
                    ProductDiscounts = productDiscountDict.Values.ToList()
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetDiscountSummary", ex.Message, "CoreRepository:GetDiscountSummaryAsync", merchantId.ToString());
                return new DiscountSummaryDto { TotalDiscount = 0, ProductDiscounts = new List<ProductDiscountDto>() };
            }
        }


        public async Task<List<PaymentMethodDto>> GetPaymentMethodStatsAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded)
                    .ToListAsync();

                var totalAmount = orders.Sum(o => o.TotalAmount);

                var stats = orders
                    .GroupBy(o => o.PaymentType)
                    .Select(g => new PaymentMethodDto
                    {
                        Method = g.Key,
                        OrderCount = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount),
                        Percentage = totalAmount > 0 ? (g.Sum(o => o.TotalAmount) / totalAmount) * 100 : 0
                    })
                    .ToList();

                return stats;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetPaymentMethodStats", ex.Message, "CoreRepository:GetPaymentMethodStatsAsync", merchantId.ToString());
                return new List<PaymentMethodDto>();
            }
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync(long merchantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate)
                    .ToListAsync();

                var activeOrders = orders.Where(o => !o.IsRefunded).ToList();
                var refundedOrders = orders.Where(o => o.IsRefunded).ToList();

                var totalOrders = activeOrders.Count;
                var totalRevenue = activeOrders.Sum(o => o.TotalAmount);
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Find best selling time
                string bestSellingTime = "-";
                if (activeOrders.Any())
                {
                    var timeGroups = activeOrders
                        .GroupBy(o => o.OrderDate?.Hour ?? 0)
                        .Select(g => new { Hour = g.Key, Count = g.Count() })
                        .OrderByDescending(g => g.Count)
                        .FirstOrDefault();

                    if (timeGroups != null)
                    {
                        bestSellingTime = $"{timeGroups.Hour}:00";
                    }
                }

                return new OrderStatsDto
                {
                    TotalOrders = totalOrders,
                    AverageOrderValue = avgOrderValue,
                    RefundedOrders = refundedOrders.Count,
                    RefundedAmount = refundedOrders.Sum(o => o.TotalAmount),
                    BestSellingTime = bestSellingTime
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetOrderStats", ex.Message, "CoreRepository:GetOrderStatsAsync", merchantId.ToString());
                return new OrderStatsDto();
            }
        }
        // reports/summary end 

        public async Task<Orders> GetOrderForEditAsync(long orderId, long merchantId)
        {
            try
            {
                return await _appDbContext.Orders
                    .Include(o => o.OrderItems) // Include items for edit
                    .FirstOrDefaultAsync(x => x.Id == orderId && x.MerchantId == merchantId)
                    ?? new Orders();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetOrderForEdit", ex.Message, "CoreRepository:GetOrderForEditAsync", merchantId.ToString());
                return new Orders();
            }
        }
        public async Task<bool> UpdateOrderWithItemsAsync(Orders order, List<OrderItems> items)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                // Remove old items
                var oldItems = _appDbContext.OrderItems.Where(x => x.OrderId == order.Id);
                _appDbContext.OrderItems.RemoveRange(oldItems);

                // Add new items
                await _appDbContext.OrderItems.AddRangeAsync(items);

                // Update order
                _appDbContext.Orders.Update(order);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogWriteAsync("Error-UpdateOrderWithItems", ex.Message, "CoreRepository:UpdateOrderWithItemsAsync", order.MerchantId.ToString());
                return false;
            }
        }

        //Website Work
        public async Task<WebsiteConfig?> GetWebsiteConfigBySubdomainAsync(string subdomain)
        {
            try
            {
                return await _appDbContext.WebsiteConfigs
                    .Include(w => w.Merchant)
                    .FirstOrDefaultAsync(w => w.Subdomain.ToLower() == subdomain.ToLower() && w.IsActive);
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetWebsiteConfigBySubdomain", ex.Message, "CoreRepository:GetWebsiteConfigBySubdomainAsync", "System");
                return null;
            }
        }

        public async Task<WebsiteConfig?> GetWebsiteConfigByMerchantIdAsync(long merchantId)
        {
            try
            {
                return await _appDbContext.WebsiteConfigs
                    .FirstOrDefaultAsync(w => w.MerchantId == merchantId);
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetWebsiteConfigByMerchantId", ex.Message, "CoreRepository:GetWebsiteConfigByMerchantIdAsync", merchantId.ToString());
                return null;
            }
        }

        public async Task<bool> CreateDefaultWebsiteConfigAsync(WebsiteConfig config)
        {
            try
            {
                await _appDbContext.WebsiteConfigs.AddAsync(config);
                return await _appDbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-CreateDefaultWebsiteConfig", ex.Message, "CoreRepository:CreateDefaultWebsiteConfigAsync", config.MerchantId.ToString());
                return false;
            }
        }

        public async Task<bool> UpdateWebsiteConfigAsync(WebsiteConfig config)
        {
            try
            {
                config.UpdatedAt = DateTime.Now;
                _appDbContext.WebsiteConfigs.Update(config);
                return await _appDbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-UpdateWebsiteConfig", ex.Message, "CoreRepository:UpdateWebsiteConfigAsync", config.MerchantId.ToString());
                return false;
            }
        }

        public async Task<MenuResponseDto> GetMenuBySubdomainAsync(string subdomain)
        {
            try
            {
                // First get the merchant from subdomain
                var websiteConfig = await _appDbContext.WebsiteConfigs
                    .FirstOrDefaultAsync(w => w.Subdomain.ToLower() == subdomain.ToLower() && w.IsActive);

                if (websiteConfig == null)
                    return new MenuResponseDto();

                long merchantId = websiteConfig.MerchantId;

                // Get all categories for this merchant that are NOT deleted
                var categories = await _appDbContext.Categories
                    .Where(c => c.MerchantId == merchantId && !c.IsDeleted)  // Added !c.IsDeleted
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                // Get all products for this merchant that are NOT deleted
                var products = await _appDbContext.Products
                    .Where(p => p.MerchantId == merchantId && !p.IsDeleted)  // Added !p.IsDeleted
                    .ToListAsync();

                var menuResponse = new MenuResponseDto();

                foreach (var category in categories)
                {
                    var categoryDto = new CategoryMenuDto
                    {
                        Id = category.Id,
                        Name = category.CategoryName,
                        Products = products
                            .Where(p => p.CategoryId == category.Id)
                            .Select(p => new ProductMenuDto
                            {
                                Id = p.Id,
                                Name = p.ProductName,
                                Price = p.ProductPrice,
                                ImageUrl = p.ImagePath,
                                CategoryId = p.CategoryId
                                // Removed Description and IsAvailable
                            })
                            .ToList()
                    };

                    menuResponse.Categories.Add(categoryDto);
                }

                // Optional: Add uncategorized products (categoryId = null)
                var uncategorizedProducts = products
                    .Where(p => p.CategoryId == null)
                    .Select(p => new ProductMenuDto
                    {
                        Id = p.Id,
                        Name = p.ProductName,
                        Price = p.ProductPrice,
                        ImageUrl = p.ImagePath,
                        CategoryId = p.CategoryId
                        // Removed Description and IsAvailable
                    })
                    .ToList();

                if (uncategorizedProducts.Any())
                {
                    menuResponse.Categories.Add(new CategoryMenuDto
                    {
                        Id = 0,
                        Name = "Other Items",
                        Products = uncategorizedProducts
                    });
                }

                return menuResponse;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetMenuBySubdomain", ex.Message, "CoreRepository:GetMenuBySubdomainAsync", subdomain);
                return new MenuResponseDto();
            }
        }
    }
}