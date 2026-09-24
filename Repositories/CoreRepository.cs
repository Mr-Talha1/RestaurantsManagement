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

        public async Task<List<DiningTables>> GetDiningTables(long merchantId, string UserID,int BranchId)
        {
            try
            {
                return await _appDbContext.DiningTables
                    .Where(x => x.MerchantId == merchantId && x.BranchId == BranchId && x.IsDeleted==false)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetDiningTables", ex.Message, "CoreRepository:GetDiningTables", UserID ?? "System");

                return new List<DiningTables>();
            }
        }
        public async Task<bool> GetDiningTableByNameAsync(string name, long merchantId,int BranchId)
        {
            return await _appDbContext.DiningTables.AnyAsync(p => p.MerchantId == merchantId
                                                    && p.Name == name
                                                    && p.BranchId == BranchId
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
        public async Task<bool> UpdateDiningTableAsync(DiningTableDto model,long MerchantId,int BranchId)
        {
            try
            {
                var existing = await _appDbContext.DiningTables
                                    .FirstOrDefaultAsync(x => x.Id == model.Id && x.MerchantId == MerchantId && x.BranchId == BranchId && x.IsDeleted == false);

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

        public async Task<bool> DeleteDiningTableAsync(int id, long merchantId,int BranchId)
        {
            try
            {
                var table = await _appDbContext.DiningTables
                    .FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId && x.BranchId == BranchId && x.IsDeleted == false);

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
                    BranchId= o.BranchId,
                    Customer = customer,   // 👈 customer only when exists
                    Items = items
                });
            }

            return result;
        }
        public async Task<List<OrderHistoryResponse>> GetOrderHistoryAsync(long merchantId,DateTime fromDate,DateTime toDate,int? branchId)
        {
            var orders = await _appDbContext.Orders
                .Where(x => x.MerchantId == merchantId &&
                            x.OrderDate >= fromDate &&
                            x.OrderDate <= toDate &&
                            (branchId == null || x.BranchId == branchId))
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var result = new List<OrderHistoryResponse>();

            foreach (var o in orders)
            {
                // ---------- Order Items ----------
                var items = await (from i in _appDbContext.OrderItems
                                   join p in _appDbContext.Products
                                       on i.ProductId equals p.Id
                                   where i.OrderId == o.Id
                                   select new OrderItemResponse
                                   {
                                       ProductId = p.Id,
                                       ProductName = p.ProductName,
                                       Qty = i.Qty,
                                       UnitPrice = i.UnitPrice,
                                       GrossTotal = i.GrossTotal,
                                       TotalPrice = i.TotalPrice,
                                       DiscountType = i.DiscountType,
                                       DiscountValue = i.DiscountValue
                                   }).ToListAsync();

                // ---------- Customer ----------
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

                    OrderDiscountType = o.OrderDiscountType,
                    OrderDiscountValue = o.OrderDiscountValue,

                    TaxType = o.TaxType,
                    TaxValue = o.TaxValue,
                    TaxAmount = o.TaxAmount,

                    BranchId = o.BranchId,

                    Customer = customer,
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

        public async Task<KpiDto> GetKpiDataAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                             !o.IsRefunded &&
            (branchId == null || o.BranchId == branchId))
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
        public async Task<List<ProductStatDto>> GetProductStatsAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
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
                                  !p.IsDeleted &&
                                  (branchId == null || o.BranchId == branchId)
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

        public async Task<TimeDataDto> GetTimeDataAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
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
                                    !o.IsRefunded &&
            (branchId == null || o.BranchId == branchId))
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
                                        !o.IsRefunded &&
                 (branchId == null || o.BranchId == branchId))
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

        public async Task<TaxSummaryDto> GetTaxSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded &&
            (branchId == null || o.BranchId == branchId))
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

        public async Task<DiscountSummaryDto> GetDiscountSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                               !o.IsRefunded &&
            (branchId == null || o.BranchId == branchId))
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


        public async Task<List<PaymentMethodDto>> GetPaymentMethodStatsAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                              !o.IsRefunded &&
            (branchId == null || o.BranchId == branchId))
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

        public async Task<OrderStatsDto> GetOrderStatsAsync(long merchantId, DateTime fromDate, DateTime toDate, int? branchId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1).AddTicks(-1);

                var orders = await _appDbContext.Orders
                    .Where(o => o.MerchantId == merchantId &&
                               o.OrderDate >= startDate &&
                               o.OrderDate <= endDate &&
                              (branchId == null || o.BranchId == branchId))
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
        public async Task<List<City>> GetCityList()
        {
            try
            {
                var result = await _appDbContext.City.ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new List<City>();
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetCityList", ex.Message, "CoreRepository.cs - GetCityList", "System");

                return new List<City>();
            }
        }
        public async Task<List<Branches>> GetBranchesByName(BranchDto branchDto,long MerchantId)
        {
            try
            {
                var result = await _appDbContext.Branches.Where(d => d.BranchName == branchDto.BranchName && d.MerchantId == MerchantId).ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new List<Branches>(); // Return empty list if nothing found
                }

                return result;
            }
            catch (WebException ex)
            {
                await LogWriteAsync("Error-GetBranches", " Error: " + ex.Message, "CoreRepository.cs - GetBranchesByName", MerchantId.ToString());

                return null; // Return empty list on exception
            }
        }

        public async Task<bool> AddBranch(Branches Branches)
        {
            try
            {
                await _appDbContext.Branches.AddAsync(Branches);
                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-AddBranch", ex.Message, "CoreRepository.cs - AddBranch", Branches.MerchantId.ToString());
                return false;
            }
        }

        public async Task<SystemUsers> GetUserByUserIdAsync(string UserID)
        {
            try
            {

                return await _appDbContext.SystemUsers
                    .FirstOrDefaultAsync(d => d.UserID == UserID) ?? new SystemUsers();


            }
            catch (Exception ex)
            {

                await LogWriteAsync("GetUserByUserIdAsync",  ex.Message, "CoreRepository.cs - GetUserByUserIdAsync", "System");
                return null;
            }
        }
        public async Task<List<UserRoles>> GetUserRolesList()
        {
            try
            {
                var result = await _appDbContext.UserRoles.Where(x => x.Active == true).ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new List<UserRoles>();
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetUserRolesList", ex.Message, "CoreRepository.cs - GetUserRolesList", "System");

                return new List<UserRoles>();
            }
        }

        public async Task<bool> AddUserAsync(SystemUsers User)
        {
            try
            {
                await _appDbContext.SystemUsers.AddAsync(User);
                return (await _appDbContext.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<List<Branches>> GetBranchesListbkold(long MerchantId)
        {
            try
            {
                var result = await _appDbContext.Branches.Where(x => x.MerchantId == MerchantId)
                    .ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new List<Branches>();
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetBranchesList",  ex.Message, "CoreRepository.cs - GetBranchesList", "System");

                return new List<Branches>();
            }
        }

        public async Task<UserRoles> GetRoleById(int Id)
        {
            try
            {
                var result = await _appDbContext.UserRoles.FirstOrDefaultAsync(x => x.Id == Id) ?? new UserRoles();
           
                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetRoleById", ex.Message, "CoreRepository.cs - GetRoleById", "System");

                return new UserRoles();
            }
        }

        public async Task<Branches> GetBranchById(int Id)
        {
            try
            {
                var result = await _appDbContext.Branches.FirstOrDefaultAsync(x => x.Id == Id) ?? new Branches();

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetBranchById", ex.Message, "CoreRepository.cs - GetBranchById", "System");

                return new Branches();
            }
        }
        public async Task<Branches> GetBranchByIdMerchantIdAsync(int BracnhId, long merchantId)
        {
            try
            {
                var result = await _appDbContext.Branches.FirstOrDefaultAsync(x => x.Id == BracnhId && x.MerchantId==merchantId);

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetBranchById", ex.Message, "CoreRepository.cs - GetBranchById", "System");

                throw;
            }
        }
        public async Task<List<Branches>> GetBranchesListByIdbkold(int Id)
        {
            try
            {
                var result = await _appDbContext.Branches.Where(x => x.Id == Id)
                    .ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new List<Branches>();
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetBranchesListById", ex.Message, "CoreRepository.cs - GetBranchesListById", "System");

                return new List<Branches>();
            }
        }

        public async Task<List<Branches>> GetLocationsByMerchantAsync(long MerchantId)
        {
            return await _appDbContext.Branches
                .Where(l => l.MerchantId == MerchantId)
                .ToListAsync();
        }
        public async Task<List<Branches>> GetLocationsByBranchCodesAsync(List<int> branchIds)
        {
            try
            {
                return await _appDbContext.Branches
                                     .Where(l => branchIds.Contains(l.Id))
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("GetLocationsByBranchCodesAsync", " Error: " + ex.Message,
                                   "CoreRepository.cs - GetLocationsByBranchCodesAsync", "");
                throw;
            }
        }

        public async Task<List<SystemUsers>> GetUsersByMerchantAsync(long merchantId, string role, int? userLocationId)
        {
            try
            {
                // Base query: MerchantID match karna hai
                var query = _appDbContext.SystemUsers
                    .Where(u => u.MerchantId == merchantId && u.BranchId != null);

                // Role-based filter:
                if (role != "BusinessAdmin" && userLocationId.HasValue)
                {
                    // LocationAdmin/User: sirf apni assigned LocationID
                    query = query.Where(u => u.BranchId == userLocationId.Value);
                }

                // Sirf LocationAdmin/User or all for BusinessAdmin
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("GetUsersByMerchantAsync"," Error: " + ex.Message, "CoreRepository.cs - GetUsersByMerchantAsync", merchantId.ToString());
                throw;
            }
        }

        public async Task<List<BranchListDto>> GetBranchesList(long merchantId, string role,int branchId)
        {
            try
            {
                var query = _appDbContext.Branches.AsNoTracking();

                if (role == "BusinessAdmin")
                {
                    return await query
                        .Where(x => x.MerchantId == merchantId)
                        .Select(x => new BranchListDto
                        {
                            Id = x.Id,
                            BranchName = x.BranchName
                        })
                        .ToListAsync();
                }

                return await query
                    .Where(x => x.Id == branchId && x.MerchantId == merchantId)
                    .Select(x => new BranchListDto
                    {
                        Id = x.Id,
                        BranchName = x.BranchName
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetBranchesList",ex.Message,"CoreRepository.cs - GetBranchesList", "System");

                return new List<BranchListDto>();
            }
        }
        public async Task<Branches> GetBranchesByNameandMerchantId(string BranchName, long MerchantId)
        {
            try
            {
                var result = await _appDbContext.Branches.FirstOrDefaultAsync(d => d.BranchName == BranchName && d.MerchantId == MerchantId);


                return result;
            }
            catch (WebException ex)
            {
                await LogWriteAsync("Error-GetBranchesByNameandMerchantId", " Error: " + ex.Message, "CoreRepository.cs - GetBranchesByNameandMerchantId", MerchantId.ToString());

                return null; // Return empty list on exception
            }
        }

        public async Task<bool> UpdateBranchAsync(Branches branches)
        {
            try
            {
                _appDbContext.Branches.Update(branches);
                int result = await _appDbContext.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
        
        // Dashboard 
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(long merchantId,int? branchId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                var orders = await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate &&
                        !o.IsRefunded)
                    .ToListAsync();

                var revenue = orders.Sum(x => x.TotalAmount);

                var orderCount = orders.Count;

                var avgOrder = orderCount > 0
                    ? revenue / orderCount
                    : 0;

                //var customerCount = _appDbContext.orders
                //    .Where(x => x.CustomerId.HasValue)
                //    .Select(x => x.CustomerId)
                //    .Distinct()
                //    .Count(); 
                

                var customerCount = await _appDbContext.Customers
                   .CountAsync(x =>
                       x.MerchantId == merchantId);

                var totalStaff = await _appDbContext.SystemUsers
                    .CountAsync(x =>
                        x.MerchantId == merchantId &&
                        x.BranchId == branchId);

                var activeStaff = await _appDbContext.SystemUsers
                    .CountAsync(x =>
                        x.MerchantId == merchantId &&
                        x.BranchId == branchId &&
                        x.IsActive);

                return new DashboardSummaryDto
                {
                    Revenue = revenue,
                    Orders = orderCount,
                    AvgOrder = avgOrder,
                    Customers = customerCount,
                    ActiveStaff = activeStaff,
                    TotalStaff = totalStaff
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetDashboardSummary",
                    ex.Message,
                    "CoreRepository:GetDashboardSummaryAsync",
                    merchantId.ToString());

                return new DashboardSummaryDto();
            }
        }
        public async Task<List<RevenueTrendDto>> GetRevenueTrendAsync(long merchantId,int branchId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                var orders = await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate &&
                        !o.IsRefunded)
                    .Select(o => new
                    {
                        o.OrderDate,
                        o.TotalAmount
                    })
                    .ToListAsync();

                return orders
                    .GroupBy(x => x.OrderDate.Value.Date)
                    .Select(g => new RevenueTrendDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetRevenueTrend",
                    ex.Message,
                    "CoreRepository:GetRevenueTrendAsync",
                    merchantId.ToString());

                return new List<RevenueTrendDto>();
            }
        }

        public async Task<List<PaymentMethodDashboardDto>> GetDashboardPaymentMethodsAsync(long merchantId,int branchId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                var orders = await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate &&
                        !o.IsRefunded)
                    .Select(o => new
                    {
                        o.PaymentType,
                        o.TotalAmount
                    })
                    .ToListAsync();

                var totalAmount = orders.Sum(x => x.TotalAmount);

                return orders
                    .GroupBy(x => x.PaymentType)
                    .Select(g => new PaymentMethodDashboardDto
                    {
                        PaymentMethod = g.Key,
                        Amount = g.Sum(x => x.TotalAmount),
                        Percentage = totalAmount > 0
                            ? (g.Sum(x => x.TotalAmount) / totalAmount) * 100
                            : 0
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToList();
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetDashboardPaymentMethods",
                    ex.Message,
                    "CoreRepository:GetDashboardPaymentMethodsAsync",
                    merchantId.ToString());

                return new List<PaymentMethodDashboardDto>();
            }
        }

        //public async Task<List<HourlyOrderDto>> GetHourlyOrdersAsync(long merchantId,int branchId,DateTime fromDate, DateTime toDate)
        //{
        //    try
        //    {
        //        var startDate = fromDate.Date;
        //        var endDate = toDate.Date.AddDays(1);

        //        var orders = await _appDbContext.Orders
        //            .Where(o =>
        //                o.MerchantId == merchantId &&
        //                o.BranchId == branchId &&
        //                o.OrderDate >= startDate &&
        //                o.OrderDate < endDate &&
        //                !o.IsRefunded)
        //            .Select(o => new
        //            {
        //                o.OrderDate,
        //                o.TotalAmount
        //            })
        //            .ToListAsync();

        //        var result = new List<HourlyOrderDto>();

        //        for (int hour = 0; hour < 24; hour++)
        //        {
        //            var hourlyOrders = orders
        //                .Where(x => x.OrderDate.Hour == hour)
        //                .ToList();

        //            result.Add(new HourlyOrderDto
        //            {
        //                Hour = hour,
        //                Label = $"{hour}:00",
        //                Revenue = hourlyOrders.Sum(x => x.TotalAmount),
        //                OrderCount = hourlyOrders.Count
        //            });
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogWriteAsync(
        //            "Error-GetHourlyOrders",
        //            ex.Message,
        //            "CoreRepository:GetHourlyOrdersAsync",
        //            merchantId.ToString());

        //        return new List<HourlyOrderDto>();
        //    }
        //}

        public async Task<BranchPerformanceDto> GetBranchPerformanceAsync(long merchantId,int branchId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                var branch = await _appDbContext.Branches
                    .Where(x =>
                        x.Id == branchId &&
                        x.MerchantId == merchantId)
                    .Select(x => new
                    {
                        x.Id,
                        x.BranchName
                    })
                    .FirstOrDefaultAsync();

                if (branch == null)
                {
                    return new BranchPerformanceDto();
                }

                var orders = await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate &&
                        !o.IsRefunded)
                    .ToListAsync();

                return new BranchPerformanceDto
                {
                    BranchName = branch.BranchName,
                    Revenue = orders.Sum(x => x.TotalAmount),
                    Orders = orders.Count
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetBranchPerformance",
                    ex.Message,
                    "CoreRepository:GetBranchPerformanceAsync",
                    merchantId.ToString());

                return new BranchPerformanceDto();
            }
        }
        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(long merchantId, int branchId,DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                return await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate)
                    .OrderByDescending(o => o.Id)
                    .Take(10)
                    .Select(o => new RecentOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        OrderType = o.OrderType,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        PaymentType = o.PaymentType
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetRecentOrders",
                    ex.Message,
                    "CoreRepository:GetRecentOrdersAsync",
                    merchantId.ToString());

                return new List<RecentOrderDto>();
            }
        }
        //public async Task<List<TopProductDto>> GetTopProductsAsync(long merchantId,int branchId,DateTime fromDate, DateTime toDate)
        //{
        //    try
        //    {
        //        var startDate = fromDate.Date;
        //        var endDate = toDate.Date.AddDays(1);

        //        var result = await (
        //            from o in _appDbContext.Orders
        //            join oi in _appDbContext.OrderItems
        //                on o.Id equals oi.OrderId
        //            join p in _appDbContext.Products
        //                on oi.ProductId equals p.Id
        //            where o.MerchantId == merchantId &&
        //                  o.BranchId == branchId &&
        //                  o.OrderDate >= startDate &&
        //                  o.OrderDate < endDate &&
        //                  !o.IsRefunded &&
        //                  !p.IsDeleted
        //            group oi by new
        //            {
        //                p.Id,
        //                p.ProductName
        //            }
        //            into g
        //            select new TopProductDto
        //            {
        //                ProductId = g.Key.Id,
        //                ProductName = g.Key.ProductName,
        //                Orders = g.Count(),
        //                Revenue = g.Sum(x => x.TotalPrice)
        //            })
        //            .OrderByDescending(x => x.Revenue)
        //            .Take(5)
        //            .ToListAsync();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogWriteAsync(
        //            "Error-GetTopProducts",
        //            ex.Message,
        //            "CoreRepository:GetTopProductsAsync",
        //            merchantId.ToString());

        //        return new List<TopProductDto>();
        //    }
        //}
        public async Task<DashboardTimeDataDto> GetDashboardTimeDataAsync(long merchantId,int branchId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date.AddDays(1);

                var orders = await _appDbContext.Orders
                    .Where(o =>
                        o.MerchantId == merchantId &&
                        o.BranchId == branchId &&
                        o.OrderDate.HasValue &&
                        o.OrderDate >= startDate &&
                        o.OrderDate < endDate &&
                        !o.IsRefunded)
                    .Select(o => new
                    {
                        OrderDate = o.OrderDate.Value,
                        o.TotalAmount
                    })
                    .ToListAsync();

                // ==============================
                // SAME DAY -> HOURLY
                // ==============================
                if (fromDate.Date == toDate.Date)
                {
                    var result = new List<DashboardTimeItemDto>();

                    for (int hour = 0; hour < 24; hour++)
                    {
                        var hourlyOrders = orders
                            .Where(x => x.OrderDate.Hour == hour)
                            .ToList();

                        result.Add(new DashboardTimeItemDto
                        {
                            Label = DateTime.Today
                                .AddHours(hour)
                                .ToString("htt"),

                            Revenue = hourlyOrders.Sum(x => x.TotalAmount),

                            OrderCount = hourlyOrders.Count
                        });
                    }

                    return new DashboardTimeDataDto
                    {
                        Type = "Hourly",
                        Data = result
                    };
                }

                // ==============================
                // MULTIPLE DAYS -> DAILY
                // ==============================

                var dailyResult = new List<DashboardTimeItemDto>();

                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    var dailyOrders = orders
                        .Where(x => x.OrderDate.Date == date.Date)
                        .ToList();

                    dailyResult.Add(new DashboardTimeItemDto
                    {
                        Label = date.ToString("dd MMM"),

                        Revenue = dailyOrders.Sum(x => x.TotalAmount),

                        OrderCount = dailyOrders.Count
                    });
                }

                return new DashboardTimeDataDto
                {
                    Type = "Daily",
                    Data = dailyResult
                };
            }
            catch (Exception ex)
            {
                await LogWriteAsync(
                    "Error-GetDashboardTimeData",
                    ex.Message,
                    "CoreRepository:GetDashboardTimeDataAsync",
                    merchantId.ToString());

                return new DashboardTimeDataDto
                {
                    Type = "Daily",
                    Data = new List<DashboardTimeItemDto>()
                };
            }
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(long merchantId, int branchId, DateTime fromDate, DateTime toDate)
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
                                  !p.IsDeleted &&
                                  (branchId == null || o.BranchId == branchId)
                            select new { o, oi, p };

                var results = await query.ToListAsync();

                // Group by product manually to calculate stats
                var productGroups = results
                    .GroupBy(x => new { x.p.Id, x.p.ProductName, x.p.ProductPrice })
                    .Select(g => new TopProductDto
                    {
                        ProductId = g.Key.Id,
                        ProductName = g.Key.ProductName,
                        Quantity = g.Sum(x => x.oi.Qty),
                        Revenue = g.Sum(x => x.oi.TotalPrice),
                        OriginalRevenue = g.Sum(x => x.oi.GrossTotal ?? 0),
                        //AveragePrice = g.Key.ProductPrice,

                        //// Calculate discount amount properly
                        //DiscountAmount = g.Sum(x =>
                        //    (x.oi.DiscountAmount ?? 0) + // Item-level discounts
                        //    (GetProportionalOrderDiscount(x.o, x.oi) ?? 0) // Order-level discounts
                        //),

                        //// Count items that had any discount
                        //ItemsDiscounted = g.Count(x =>
                        //    x.oi.DiscountAmount > 0 ||
                        //    (x.o.TotalDiscount > 0 && x.oi.GrossTotal > 0)
                        //)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .ToList();

                return productGroups;
            }
            catch (Exception ex)
            {
                await LogWriteAsync("Error-GetTopProductsAsync", ex.Message, "CoreRepository:GetTopProductsAsync", merchantId.ToString());
                return new List<TopProductDto>();
            }
        }
    }
}