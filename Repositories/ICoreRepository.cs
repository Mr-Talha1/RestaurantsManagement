using BIPL_RAASTP2M.DTO;
using BIPL_RAASTP2M.Models;

namespace BIPL_RAASTP2M.Repositories
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
    }
}
