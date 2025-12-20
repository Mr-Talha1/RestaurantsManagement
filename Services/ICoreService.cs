using BIPL_RAASTP2M.DTO;
using BIPL_RAASTP2M.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace BIPL_RAASTP2M.Services
{
    public interface ICoreService 
    {
        public string Decrypt(string cipherText);
        public string Encrypt(string cipherText);
        public string CalculateMD5Hash(string cipherText);
        public Task LogWrite(string Activity, string Description, string Interface,string UserID);
        Task<dynamic> LoginServiceAsync(LoginRequestDto model);
        Task<List<DiningTables>> GetDiningTablesService(long merchantId, string UserID);
        Task<bool> AddDiningTableService(DiningTableDto req, long merchantId);
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
        Task<object> AddOrderAsync(AddOrderRequest model, long merchantId, int userId);
        Task<object> GetOrderHistoryAsync(OrderHistoryRequest model, long merchantId);
    }
}
