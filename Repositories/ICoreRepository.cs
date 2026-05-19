using TBAppBackend.DTO;
using TBAppBackend.Models;

namespace TBAppBackend.Repositories
{
    public interface ICoreRepository
    {
        Task LogWriteAsync(string Activity, string Description, string Interface,string UserID);
        Task<SystemUsers> GetSyestemUserByUserId(string UserID);
        Task<Merchants> GetMerchantById(long id);
        Task<List<DiningTables>> GetDiningTables(long merchantId, string UserID);
        Task<bool> AddDiningTableAsync(DiningTables table);
        Task<bool> UpdateDiningTableAsync(DiningTableDto model, long MerchantId);
        Task<bool> DeleteDiningTableAsync(long id, long merchantId);
        Task<string> AddCategoryAsync(CategoryDto model, long merchantId);
        Task<Categories> GetCategoryId(int Id, long merchantId);
        Task<Categories> GetCategoryByName(string CategoryName, long merchantId);
        Task<bool> UpdateCategoryAsync(Categories categories);
        Task<List<Categories>> GetCategoriesAsync(long merchantId);
        Task<bool> DeleteCategoryAsync(long id, long merchantId);
        //Task<bool> ExistsByNameAsync(string name, long merchantId);
        Task<bool> AddProductAsync(Products product);
        Task<List<Products>> GetProductsAsync(long merchantId);
        Task<bool> GetProductByNameAsync(string name, long merchantId);
        Task<Products> GetProductByName(string ProductName, long merchantId);
        Task<Products> GetProductById(int Id, long merchantId);
        Task<bool> UpdateProductAsync(Products products);
        Task<bool> DeleteProductAsync(Products product);
        Task<long> AddOrderAsync(Orders order);
        Task<bool> AddOrderItemsAsync(List<OrderItems> items);
        Task<List<OrderHistoryResponse>> GetOrderHistoryAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<Customers> GetCustomersbyPhoneNumber(long merchantId, string CustomerPhone);
        Task<bool> AddCustomer(Customers customers);
        Task<List<Customers>> SearchCustomersAsync(long merchantId, string query);
        Task<Orders> GetOrderByIdAsync(long orderId, long merchantId);
        Task<bool> UpdateOrderAsync(Orders order);
        Task<KpiDto> GetKpiDataAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<List<ProductStatDto>> GetProductStatsAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<TimeDataDto> GetTimeDataAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<TaxSummaryDto> GetTaxSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<DiscountSummaryDto> GetDiscountSummaryAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<List<PaymentMethodDto>> GetPaymentMethodStatsAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<OrderStatsDto> GetOrderStatsAsync(long merchantId, DateTime fromDate, DateTime toDate);
        Task<Orders> GetOrderForEditAsync(long orderId, long merchantId);
        Task<bool> UpdateOrderWithItemsAsync(Orders order, List<OrderItems> items);
        Task<WebsiteConfig?> GetWebsiteConfigBySubdomainAsync(string subdomain);
        Task<WebsiteConfig?> GetWebsiteConfigByMerchantIdAsync(long merchantId);
        Task<bool> CreateDefaultWebsiteConfigAsync(WebsiteConfig config);
        Task<bool> UpdateWebsiteConfigAsync(WebsiteConfig config);
        Task<MenuResponseDto> GetMenuBySubdomainAsync(string subdomain);
        Task<bool> GetDiningTableByNameAsync(string name, long merchantId);
        Task<Customers> GetCustomersbyCustomerId(long merchantId, long CustomerId);
        Task<bool> UpdatCustomersAsync(Customers customers);
        Task<List<City>> GetCityList();
        Task<List<Branches>> GetBranchesByName(BranchDto branchDto, long MerchantId);
        Task<bool> AddBranch(Branches Branches);
        Task<SystemUsers> GetUserByUserIdAsync(string UserID);
        Task<List<UserRoles>> GetUserRolesList();
        Task<bool> AddUserAsync(SystemUsers User);
        Task<List<Branches>> GetBranchesList(long MerchantId);
        Task<UserRoles> GetRoleById(int Id);
        Task<Branches> GetBranchById(int Id);
        Task<List<Branches>> GetBranchesListById(int Id);
    }
}
