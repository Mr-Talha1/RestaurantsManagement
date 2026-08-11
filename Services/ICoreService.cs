using TBAppBackend.DTO;
using TBAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace TBAppBackend.Services
{
    public interface ICoreService 
    {
        public string Decrypt(string cipherText);
        public string Encrypt(string cipherText);
        public string CalculateMD5Hash(string cipherText);
        public Task LogWrite(string Activity, string Description, string Interface,string UserID);
        Task<dynamic> LoginServiceAsync(LoginRequestDto model);
        Task<List<DiningTables>> GetDiningTablesService(long merchantId, string UserID);
        Task<DefaultResponse> AddDiningTableService(DiningTableDto req, long merchantId);
        Task<DefaultResponse> UpdateDiningTableAsync(DiningTableDto request, long MerchantId);
        Task<DefaultResponse> DeleteDiningTableAsync(int id, long merchantId);
        Task<DefaultResponse> AddCategoryService(CategoryDto model, long merchantId);
        Task<DefaultResponse> UpdateCategoryAsync(CategoryDto categoryDto, long merchantId);
        Task<List<Categories>> GetCategoryService(long merchantId);
        Task<DefaultResponse> DeleteCategoryService(int id, long merchantId);
        Task<DefaultResponse> AddProductAsync(AddProductRequest req, long merchantId);
        Task<List<Products>> GetProductsService(long merchantId);
        Task<DefaultResponse> UpdateProductAsync(AddProductRequest model, long merchantId);
        Task<DefaultResponse> DeleteProductService(int productId, long merchantId);
        Task<object> AddOrderAsync(AddOrderRequest model, long merchantId, string userId);
        Task<object> GetOrderHistoryAsync(OrderHistoryRequest model, long merchantId);
        Task<object> SearchCustomersAsync(string query, long merchantId);
        Task<DefaultResponse> RefundOrderAsync(long OrderId, long merchantId, string userId);
        Task<ReportResponseDto> GetReportAsync(ReportRequestDto request, long merchantId);
        Task<EditOrderResponse> EditOrderAsync(EditOrderRequest request, long merchantId, string userId);
        Task<WebsiteConfigResponseDto?> GetWebsiteConfigBySubdomainAsync(string subdomain);
        Task<WebsiteConfigResponseDto?> GetWebsiteConfigByMerchantIdAsync(long merchantId);
        Task<DefaultResponse> UpdateWebsiteConfigAsync(long merchantId, UpdateWebsiteConfigDto updateDto);
        Task<MenuResponseDto> GetMenuBySubdomainAsync(string subdomain);
        Task<List<City>> GetCityListService();
        Task<List<Branches>> GetBranchesByNameService(BranchDto branchDto, long MerchantId);
        Task<bool> AddBranchService(BranchDto branchDto, long MerchantId);
        Task<SystemUsers> GetUserByUserIdService(string UserID);
        Task<List<UserRoles>> GetUserRolesListService();
        Task<bool> AddUserAsync(AddBranchUserDto addBranchUserDto, long MerchantId);
        Task<List<Branches>> GetBranchesListService(long MerchantId, string Role, int BranchId);
        Task<List<BranchUsersDto>> GetLocationsWithUsersAsync(long merchantId, string role, int? userLocationId);
    }
}

