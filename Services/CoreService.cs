using TBAppBackend.Models;
using TBAppBackend.Repositories;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Specialized;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.Cmp;
using System.Data;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using static System.Net.WebRequestMethods;
using TBAppBackend.Security;
using System.ComponentModel.DataAnnotations;
using TBAppBackend.DTO;
using Microsoft.EntityFrameworkCore;

namespace TBAppBackend.Services
{
    public class CoreService : ICoreService
    {
        private readonly HttpClient _httpClient1;
        private readonly ICoreRepository _coreRepository;
        private readonly IConfiguration _configuration;
        private readonly IJwtFactory _jwtFactory;
        private readonly ICloudinaryService _cloudinaryService;



        public CoreService(ICoreRepository coreRepository, IConfiguration configuration, HttpClient httpClient, IJwtFactory jwtFactory, ICloudinaryService cloudinaryService)
        {
            _coreRepository = coreRepository;
            _configuration = configuration;
            _httpClient1 = httpClient;
            _jwtFactory = jwtFactory;
            _cloudinaryService = cloudinaryService;
        }


        public static string key = "u)16'#Z3,BWotF@y!o^$Aw}[+Is(-jrqd2V"; //Same as in Angular
        private object _httpClient;

        public string GenerateTransactionID()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 20)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
        public string Encrypt(string cipherText)
        {
            string password = key;
            byte[] encryptedData;
            using (Aes encryptor = Aes.Create())
            {
                var salt = Encoding.UTF8.GetBytes("1203199320052021");
                var iv = Encoding.UTF8.GetBytes("1203199320052021");
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 100);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.Padding = PaddingMode.PKCS7;
                encryptor.Mode = CipherMode.CBC;
                encryptor.IV = iv;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(cipherText);
                        }
                        encryptedData = ms.ToArray();
                    }
                }

                return Convert.ToBase64String(encryptedData);
            }

        }
        public string EncryptWMD5Hash(string strTxt)
        {
            string pass = strTxt;
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(pass);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            pass = s.ToString();
            return pass;

        }
        public string Decrypt(string cipherText)
        {
            try
            {

                string password = key;
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    var salt = Encoding.UTF8.GetBytes("1203199320052021");
                    var iv = Encoding.UTF8.GetBytes("1203199320052021");
                    var encrypted = cipherBytes;
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 100);
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.Padding = PaddingMode.PKCS7;
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.IV = iv;
                    using (MemoryStream ms = new MemoryStream(encrypted))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cs, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                return "";
            }

        }

        public string CalculateMD5Hash(string input)
        {

            // step 1, calculate MD5 hash from input
            try
            {
                MD5 md5 = System.Security.Cryptography.MD5.Create();

                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

                byte[] hash = md5.ComputeHash(inputBytes);


                // step 2, convert byte array to hex string

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)

                {

                    sb.Append(hash[i].ToString("x2"));

                }

                return sb.ToString();

            }
            catch (Exception ex)
            {

                return "";
            }


        }
        public async Task LogWrite(string Activity, string Description, string Interface,string UserID)
        {
            await _coreRepository.LogWriteAsync(Activity, Description, Interface,UserID);


        }

        public async Task<dynamic> LoginServiceAsync(LoginRequestDto model)
        {
            try
            {
                // Step-1: Get User
                var GetUser = await _coreRepository.GetSyestemUserByUserId(model.UserId);

                if (string.IsNullOrEmpty(GetUser.UserID))
                {
                    await LogWrite("LoginServiceAsync", "User does not exist", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "User does not exist"
                    };
                }
                if (GetUser.IsActive == false)
                {
                    await LogWrite("LoginServiceAsync", "InActive User", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "InActive User"
                    };
                }
                var GetMerchnat = await _coreRepository.GetMerchantById(GetUser.MerchantId);
                if (GetMerchnat.Id == 0)
                {
                    await LogWrite("LoginServiceAsync", "Merchant not found", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Merchant not found"
                    };
                }
                if (GetMerchnat.IsActive == false)
                {
                    await LogWrite("LoginServiceAsync", "InActive Merchant", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "InActive Merchant"
                    };
                }

                var GetUserRole = await _coreRepository.GetRoleById(GetUser.RoleId);
                if (GetUserRole.Id == 0)
                {
                    await LogWrite("LoginServiceAsync", "User Role Not Found", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "User Role Not Found"
                    };
                }

                var GetBranch = await _coreRepository.GetBranchById(GetUser.BranchId);
                if (GetBranch.Id == 0)
                {
                    await LogWrite("LoginServiceAsync", "Branch Not Found", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Branch Not Found"
                    };
                }

                // Step-1: User exists, PIN not entered
                if (string.IsNullOrEmpty(model.Password))
                {
                    await LogWrite("LoginServiceAsync", "User found", "CoreService:LoginServiceAsync", model.UserId ?? "System");
                    return new LoginResponseDto
                    {
                        ResponseCode = "00",
                        ResponseMessage = "User found",
                        Data = new LoginDataDto
                        {
                            UserId = GetUser.UserID,
                            FullName = GetUser.FullName,
                            MerchantId = GetMerchnat.Id,
                            BusinessName = GetMerchnat.Name,
                            LogoPath = GetMerchnat.LogoPath
                        }
                    };
                }

                // Step-2: PIN validation
                if (GetUser.PasswordHash != model.Password) // hashing later
                {
                    await LogWrite("LoginServiceAsync", "Wrong password", "CoreService:LoginServiceAsync", model.UserId ?? "System");
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Wrong password"
                    };
                }
                await LogWrite("LoginServiceAsync", "Login successful", "CoreService:LoginServiceAsync", model.UserId ?? "System");

                // Step-2: Generate Token
                return new LoginResponseDto
                {
                    ResponseCode = "00",
                    ResponseMessage = "Login successful",
                    Data = new LoginDataDto
                    {
                        UserId = GetUser.UserID,
                        FullName = GetUser.FullName,
                        Role = GetUserRole.Role,
                        MerchantId = GetMerchnat.Id,
                        BusinessName = GetMerchnat.Name,
                        BusinessAddress = GetMerchnat.Address,
                        BusinessMobileNumber = GetMerchnat.MobileNumber,
                        LogoPath = GetMerchnat.LogoPath,
                        BusinessType = GetMerchnat.BusinessType,
                        BranchName = GetBranch.BranchName,
                        BranchId = GetUser.BranchId,
                    },
                   Token = await _jwtFactory.LoginToken(GetUserRole.Role,GetUser.UserID, GetMerchnat.Id.ToString(), GetBranch.Id.ToString())
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-Login", ex.Message, "CoreService:LoginServiceAsync", model.UserId ?? "System");
                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong"
                };
            }
        }

        public async Task<List<DiningTables>> GetDiningTablesService(long merchantId,string UserID)
        {
            return await _coreRepository.GetDiningTables(merchantId, UserID);
        }
        public async Task<DefaultResponse> AddDiningTableService(DiningTableDto req, long merchantId)
        {
            try
            {
                // duplicate name check
                if (await _coreRepository.GetDiningTableByNameAsync(req.Name, merchantId))
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Table name already exists"
                    };
                }

                DiningTables table = new DiningTables
            {
                MerchantId = merchantId,
                Name = req.Name,
                IsDeleted=false,
                CreatedAt = DateTime.Now
            };

            var result = await _coreRepository.AddDiningTableAsync(table);

                if (result)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Table Added Successfully.",
                    };
                }
                else
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to Add Table.",
                    };
                }

            }
             catch (Exception ex)
            {
                await LogWrite("Error-AddDiningTableService", ex.Message, "CoreService:AddDiningTableService", merchantId.ToString() ?? "System");
                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                };

            }
        }
        public async Task<DefaultResponse> UpdateDiningTableAsync(DiningTableDto request,long MerchantId)
        {
            try
            {
                bool result = await _coreRepository.UpdateDiningTableAsync(request, MerchantId);

                if (!result)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Table Not Found."
                    };
                }

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Dining Table Updated Successfully."
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-UpdateDiningTableAsync", ex.Message, "CoreService:UpdateDiningTableAsync", MerchantId.ToString() ?? "System");

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong."
                };
            }
        }
        public async Task<DefaultResponse> DeleteDiningTableAsync(int id, long merchantId)
        {
            try
            {
                var deleted = await _coreRepository.DeleteDiningTableAsync(id, merchantId);

                if (!deleted)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Dining table not found"
                    };
                }

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "table deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-DeleteDiningTableAsync", ex.Message, "CoreService:DeleteDiningTableAsync", merchantId.ToString() ?? "System");

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong."
                };
            }
        }
        public async Task<DefaultResponse> AddCategoryService(CategoryDto model, long merchantId)
        {
            try
            {
                var result = await _coreRepository.AddCategoryAsync(model, merchantId);

                if (result == "DUPLICATE")
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Category name already exists"
                    };
                }

                if (result == "OK")
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Category added successfully"
                    };
                }

                return new DefaultResponse
                {
                    ResponseCode = "01",
                    ResponseMessage = "Category added fail try again later"
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-AddCategoryService", ex.Message, "CoreService:AddCategoryService", merchantId.ToString() ?? "System");
                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                };
            }
        }
        public async Task<DefaultResponse> UpdateCategoryAsync(CategoryDto categoryDto, long merchantId)
        {
            try
            {
                // Check existing record by Id
                var existingCategory = await _coreRepository.GetCategoryId(categoryDto.Id,merchantId);
                if (existingCategory == null || existingCategory.Id == 0)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Category not found."
                    };
                }

                var existingName = await _coreRepository.GetCategoryByName(categoryDto.CategoryName,merchantId);
                if (existingName != null && existingName.Id != 0 && existingName.Id != categoryDto.Id)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "CategoryName already exists."
                    };
                }

                // Update data
                existingCategory.CategoryName = categoryDto.CategoryName;

                bool isUpdated = await _coreRepository.UpdateCategoryAsync(existingCategory);

                if (isUpdated)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Category updated successfully."
                    };
                }
                else
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Category Update fail try again later."
                    };
                }
            }
            catch (Exception ex)
            {
                await LogWrite("UpdateCategoryAsync-Error", ex.Message, "CoreService.cs:UpdateCategoryAsync", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }
        public async Task<List<Categories>> GetCategoryService(long merchantId)
        {
            return await _coreRepository.GetCategoriesAsync(merchantId);
        }
        public async Task<DefaultResponse> DeleteCategoryService(int id, long merchantId)
        {
            try
            {
                var deleted = await _coreRepository.DeleteCategoryAsync(id, merchantId);

                if (!deleted)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Category not found"
                    };
                }

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Category deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-DeleteCategoryService", ex.Message, "CoreService:DeleteCategoryService", merchantId.ToString() ?? "System");

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong."
                };
            }
        }
        public async Task<DefaultResponse> AddProductAsync(AddProductRequest req, long merchantId)
        {
            //var resp = new ApiResponse();
            try
            {
                // duplicate name check
                if (await _coreRepository.GetProductByNameAsync(req.ProductName, merchantId))
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Product name already exists"
                    };
                }

                string? imageUrl = null;
                string? imagePublicId = null;

                if (req.Image != null)
                {
                    // validate file type & size
                    var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
                    if (!allowed.Contains(req.Image.ContentType))
                    {
                        return new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Invalid image type"
                        };
                    }

                    var upload = await _cloudinaryService.UploadImageAsync(req.Image, $"merchants/{merchantId}/products");
                    if (!upload.Success)
                    {
                        return new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Image upload failed: " + upload.Error
                        };
                    }
                    imageUrl = upload.Url;
                    imagePublicId = upload.PublicId;
                }

                var product = new Products
                {
                    MerchantId = merchantId,
                    ProductName = req.ProductName,
                    ProductPrice = req.ProductPrice,
                    CategoryId = req.CategoryId,
                    ImagePath = imageUrl,
                    ImagePublicId = imagePublicId,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                };

                var added = await _coreRepository.AddProductAsync(product);
                if (!added)
                {
                    // If DB save failed, delete image from Cloudinary to avoid orphan
                    if (!string.IsNullOrEmpty(imagePublicId))
                        await _cloudinaryService.DeleteImageAsync(imagePublicId);

                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Product not added"
                    };
                }
                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Product added successfully"
                };
                //resp.ResponseCode = "00";
                //resp.ResponseMessage = "Product added successfully";
                //resp.Data = new { product.Id, product.Name, product.Price, product.ImagePath };
                //return resp;
            }
            catch (Exception ex)
            {
                await LogWrite("Error-AddProductAsync", ex.Message, "CoreService:AddProductAsync", merchantId.ToString() ?? "System");

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong."
                };
            }
        }
        public async Task<List<Products>> GetProductsService(long merchantId)
        {
            return await _coreRepository.GetProductsAsync(merchantId);
        }

        public async Task<DefaultResponse> UpdateProductAsync(AddProductRequest model, long merchantId)
        {
            try
            {
                var product = await _coreRepository.GetProductById(model.Id??0,merchantId);
            if (product == null || product.Id == 0)
            {
                return new DefaultResponse
                {
                    ResponseCode = "01",
                    ResponseMessage = "Product not found."
                };
            }

            // Duplicate name check
            var existingName = await _coreRepository.GetProductByName(model.ProductName, merchantId);
            if (existingName != null && existingName.Id != 0 && existingName.Id != model.Id)
            {
                return new DefaultResponse
                {
                   ResponseCode = "01",
                   ResponseMessage = "Product name already exists."
                };
            }
            // Update fields
            product.ProductName = model.ProductName;
            product.ProductPrice = model.ProductPrice;
            product.CategoryId = model.CategoryId;

            // ========= IMAGE UPDATE LOGIC ==========
            if (model.Image != null)
            {
                // DELETE OLD IMAGE if exists
                if (!string.IsNullOrEmpty(product.ImagePublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
                }

                // UPLOAD NEW IMAGE
                var uploadResult = await _cloudinaryService.UploadImageAsync(model.Image, $"merchants/{merchantId}/products");

                // Save new values
                product.ImagePath = uploadResult.Url;
                product.ImagePublicId = uploadResult.PublicId;
            }

            // Save update
            bool isUpdated =await _coreRepository.UpdateProductAsync(product);

                if (isUpdated)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Product updated successfully."
                    };
                }
                else
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Product Update fail try again later."
                    };
                }
        }
            catch (Exception ex)
            {
                await LogWrite("UpdateProductAsync-Error", ex.Message, "CoreService.cs:UpdateProductAsync", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
}
        }

        public async Task<DefaultResponse> DeleteProductService(int productId, long merchantId)
        {
            try
            {
                var product = await _coreRepository.GetProductById(productId, merchantId);

                if (product == null || product.Id == 0)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Product not found."
                    };
                }

                // ========== DELETE IMAGE FROM CLOUDINARY ==========
                if (!string.IsNullOrEmpty(product.ImagePublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
                }

                // ========== SOFT DELETE + RESET IMAGE FIELDS ==========
                bool deleted = await _coreRepository.DeleteProductAsync(product);

                if (deleted)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Product deleted successfully."
                    };
                }
                else
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to delete product."
                    };
                }
            }
            catch (Exception ex)
            {
                await LogWrite("SoftDeleteProduct-Error", ex.Message,
                               "CoreService.cs:SoftDeleteProductAsync",
                               merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }

        //public async Task<object> AddOrderAsyncbk(AddOrderRequest model, long merchantId, int userId)
        //{
        //    try
        //    {
        //        // ========================
        //        // VALIDATE ITEMS
        //        // ========================
        //        if (model.Items == null || model.Items.Count == 0)
        //        {
        //            return new DefaultResponse
        //            {
        //                ResponseCode = "01",
        //                ResponseMessage = "No items found in order."
        //            };
        //        }

        //        decimal totalAmount = 0;
        //        int itemCount = 0;

        //        var orderItems = new List<OrderItems>();

        //        // ========================
        //        // VALIDATE EACH PRODUCT
        //        // ========================
        //        foreach (var item in model.Items)
        //        {
        //            var product = await _coreRepository.GetProductById(item.ProductId, merchantId);

        //            if (product == null || product.Id == 0)
        //            {
        //                return new DefaultResponse
        //                {
        //                    ResponseCode = "01",
        //                    ResponseMessage = $"Product not found: ID {item.ProductId}"
        //                };
        //            }

        //            decimal unitPrice = product.ProductPrice;
        //            decimal total = unitPrice * item.Qty;

        //            totalAmount += total;
        //            itemCount += item.Qty;

        //            orderItems.Add(new OrderItems
        //            {
        //                ProductId = item.ProductId,
        //                Qty = item.Qty,
        //                UnitPrice = unitPrice,
        //                TotalPrice = total,
        //                CreatedAt = DateTime.Now
        //            });
        //        }

        //        // ========================
        //        // INSERT ORDER
        //        // ========================
        //        var newOrder = new Orders
        //        {
        //            MerchantId = merchantId,
        //            UserId = userId,
        //            OrderType = model.OrderType,
        //            TableId = model.TableId,
        //            TotalAmount = totalAmount,
        //            ItemsCount = itemCount,
        //            OrderNumber = model.OrderNumber, 
        //            PaymentType=model.PaymentType,
        //            OrderDate=model.OrderDate,
        //            CreatedAt = DateTime.Now
        //        };

        //        long orderId = await _coreRepository.AddOrderAsync(newOrder);

        //        // ========================
        //        // SAVE ORDER ITEMS
        //        // ========================
        //        orderItems.ForEach(x => x.OrderId = orderId);

        //        await _coreRepository.AddOrderItemsAsync(orderItems);

        //        return new
        //        {
        //            ResponseCode = "00",
        //            ResponseMessage = "Order Created Successfully."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogWrite("AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

        //        return new DefaultResponse
        //        {
        //            ResponseCode = "05",
        //            ResponseMessage = "Service Failed"
        //        };
        //    }
        //}
        //public async Task<object> AddOrderAsyncbk1(AddOrderRequest model, long merchantId, int userId)
        //{
        //    try
        //    {
        //        // ========================
        //        // VALIDATE ITEMS
        //        // ========================
        //        if (model.Items == null || model.Items.Count == 0)
        //        {
        //            return new DefaultResponse
        //            {
        //                ResponseCode = "01",
        //                ResponseMessage = "No items found in order."
        //            };
        //        }

        //        decimal totalAmount = 0;
        //        decimal _grossTotal = 0;
        //        decimal _discountAmount = 0;
        //        int itemCount = 0;

        //        var orderItems = new List<OrderItems>();

        //        // ========================
        //        // VALIDATE EACH PRODUCT
        //        // ========================
        //        foreach (var item in model.Items)
        //        {
        //            var product = await _coreRepository.GetProductById(item.ProductId, merchantId);

        //            if (product == null)
        //            {
        //                return new DefaultResponse
        //                {
        //                    ResponseCode = "01",
        //                    ResponseMessage = $"Product not found: ID {item.ProductId}"
        //                };
        //            }

        //            decimal unitPrice = product.ProductPrice;
        //            decimal grossTotal = unitPrice * item.Qty;
        //            decimal discountAmount = 0;

        //            // ========== APPLY DISCOUNT ==========
        //            if (!string.IsNullOrEmpty(item.DiscountType) && item.DiscountValue.HasValue)
        //            {
        //                if (item.DiscountType.ToLower() == "percentage")
        //                {
        //                    discountAmount = (grossTotal * item.DiscountValue.Value) / 100;

        //                }
        //                else if (item.DiscountType.ToLower() == "flat")
        //                {
        //                    discountAmount = item.DiscountValue.Value;

        //                }
        //            }

        //            // TOTAL PRICE AFTER DISCOUNT
        //            decimal netTotal = grossTotal - discountAmount;

        //            totalAmount += netTotal;
        //            itemCount += item.Qty;
        //            _grossTotal += grossTotal;
        //            _discountAmount += discountAmount;

        //            orderItems.Add(new OrderItems
        //            {
        //                ProductId = item.ProductId,
        //                Qty = item.Qty,
        //                UnitPrice = unitPrice,
        //                DiscountType = item.DiscountType,
        //                DiscountValue = item.DiscountValue,
        //                DiscountAmount = discountAmount,
        //                GrossTotal=grossTotal,
        //                AmountAfterDiscount= netTotal,
        //                TotalPrice = netTotal,
        //                CreatedAt = DateTime.Now
        //            });
        //        }


        //        // ========================
        //        // INSERT ORDER
        //        // ========================
        //        decimal? overallDiscount = orderItems.Sum(x => x.DiscountAmount);

        //        var newOrder = new Orders
        //        {
        //            MerchantId = merchantId,
        //            UserId = userId,
        //            OrderType = model.OrderType,
        //            TableId = model.TableId,
        //            TotalAmount = totalAmount,
        //            GrossTotal = _grossTotal,
        //            TotalDiscount = _discountAmount,
        //            ItemsCount = itemCount,
        //            OrderNumber = model.OrderNumber,
        //            PaymentType = model.PaymentType,
        //            OrderDate = model.OrderDate,
        //            CreatedAt = DateTime.Now
        //        };

        //        long orderId = await _coreRepository.AddOrderAsync(newOrder);

        //        // ========================
        //        // SAVE ORDER ITEMS
        //        // ========================
        //        orderItems.ForEach(x => x.OrderId = orderId);

        //        await _coreRepository.AddOrderItemsAsync(orderItems);

        //        return new
        //        {
        //            ResponseCode = "00",
        //            ResponseMessage = "Order Created Successfully."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogWrite("AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

        //        return new DefaultResponse
        //        {
        //            ResponseCode = "05",
        //            ResponseMessage = "Service Failed"
        //        };
        //    }
        //}
        public async Task<object> AddOrderAsync(AddOrderRequest model, long merchantId, string userId)
        {
            try
            {

                // =============== CUSTOMER HANDLING ===============
                long customerId = 0;

                if (!string.IsNullOrEmpty(model.CustomerPhone))
                {
                    //var existingCustomer = await _db.Customers
                    //    .FirstOrDefaultAsync(x => x.CustomerPhone == model.CustomerPhone && x.MerchantId == merchantId);
                    var existingCustomer = await _coreRepository.GetCustomersbyPhoneNumber(merchantId, model.CustomerPhone);
                    if (existingCustomer == null || existingCustomer.CustomerId == 0)
                    {
                        var newCustomer = new Customers
                        {
                            MerchantId = merchantId,
                            CustomerName = model.CustomerName,
                            CustomerPhone = model.CustomerPhone,
                            DeliveryAddress = model.DeliveryAddress,
                            CreatedAt = DateTime.Now
                        };
                        //_db.Customers.Add(newCustomer);
                        //await _db.SaveChangesAsync();
                        await _coreRepository.AddCustomer(newCustomer);

                        customerId = newCustomer.CustomerId;
                    }
                    else
                    {
                        customerId = existingCustomer.CustomerId;
                    }
                }


                // ========================
                // VALIDATE ITEMS
                // ========================
                if (model.Items == null || model.Items.Count == 0)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "No items found in order."
                    };
                }

                // --------------------
                // Initialize totals
                // --------------------
                decimal subTotal = 0m;            // sum of items AFTER item-level discounts
                decimal grossTotalAll = 0m;      // sum of items BEFORE discounts
                decimal totalItemDiscounts = 0m; // sum of item-level discounts
                int itemCount = 0;

                var orderItems = new List<OrderItems>();

                DateTime? orderDate = model.OrderDate != default ? model.OrderDate : DateTime.Now;

                // ========================
                // VALIDATE EACH PRODUCT (item-level calculation)
                // ========================
                foreach (var item in model.Items)
                {
                    var product = await _coreRepository.GetProductById(item.ProductId, merchantId);

                    if (product == null)
                    {
                        return new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = $"Product not found: ID {item.ProductId}"
                        };
                    }

                    decimal unitPrice = product.ProductPrice;
                    decimal itemGross = unitPrice * item.Qty;
                    decimal itemDiscountAmount = 0m;

                    // ========== APPLY ITEM-LEVEL DISCOUNT ==========
                    if (!string.IsNullOrEmpty(item.DiscountType) && item.DiscountValue.HasValue)
                    {
                        if (item.DiscountType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                        {
                            // percentage applies to gross of item (unitPrice * qty)
                            itemDiscountAmount = Math.Round((itemGross * item.DiscountValue.Value) / 100m, 2);
                        }
                        else if (item.DiscountType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                        {
                            // flat discount applied once per line (NOT multiplied by qty)
                            // If you want flat per qty, change logic accordingly.
                            itemDiscountAmount = Math.Round(item.DiscountValue.Value, 2);
                        }
                    }

                    decimal itemNet = Math.Round(itemGross - itemDiscountAmount, 2);

                    // accumulate
                    grossTotalAll += itemGross;
                    totalItemDiscounts += itemDiscountAmount;
                    subTotal += itemNet;
                    itemCount += item.Qty;

                    orderItems.Add(new OrderItems
                    {
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        UnitPrice = unitPrice,
                        DiscountType = item.DiscountType,
                        DiscountValue = item.DiscountValue,
                        DiscountAmount = itemDiscountAmount,
                        GrossTotal = itemGross,
                        AmountAfterDiscount = itemNet,
                        TotalPrice = itemNet,
                        CreatedAt = DateTime.Now
                    });
                }

                // ========================
                // APPLY ORDER-LEVEL DISCOUNT (if any)
                // ========================
                decimal orderDiscountAmount = 0m;
                if (!string.IsNullOrEmpty(model.OrderDiscountType) && model.OrderDiscountValue.HasValue)
                {
                    if (model.OrderDiscountType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        orderDiscountAmount = Math.Round((subTotal * model.OrderDiscountValue.Value) / 100m, 2);
                    }
                    else if (model.OrderDiscountType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        orderDiscountAmount = Math.Round(model.OrderDiscountValue.Value, 2);
                    }

                    // safety: don't allow discount > subTotal
                    if (orderDiscountAmount > subTotal) orderDiscountAmount = subTotal;
                }


                // ========================
                // CALCULATE AMOUNT AFTER DISCOUNT (BEFORE TAX)
                // ========================
                decimal amountAfterDiscount = Math.Round(subTotal - orderDiscountAmount, 2);

                decimal totalDiscountAll = Math.Round(totalItemDiscounts + orderDiscountAmount, 2);

                // ========================
                // APPLY TAX AFTER DISCOUNT (ONLY if tax info is provided)
                // ========================
                decimal taxAmount = 0m;
                if (!string.IsNullOrEmpty(model.TaxType) && model.TaxValue.HasValue && model.TaxValue > 0)
                {
                    if (model.TaxType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        // Percentage tax: e.g., 15% of amount after discount
                        taxAmount = Math.Round((amountAfterDiscount * model.TaxValue.Value) / 100m, 2);
                    }
                    else if (model.TaxType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        // Flat tax: e.g., $5 fixed tax per order
                        taxAmount = Math.Round(model.TaxValue.Value, 2);
                    }
                }

                // ========================
                // FINAL TOTAL (Amount after discount + Tax)
                // ========================
                decimal finalPayable = amountAfterDiscount + taxAmount;


                // ========================
                // CREATE ORDER (save)
                // ========================
                var newOrder = new Orders
                {
                    MerchantId = merchantId,
                    UserId = userId,
                    CustomerId = customerId,
                    OrderType = model.OrderType,
                    TableId = model.TableId,
                    // TotalAmount store final payable (after all discounts)
                    TotalAmount = finalPayable,
                    GrossTotal = grossTotalAll,
                    TotalDiscount = totalDiscountAll,
                    ItemsCount = itemCount,
                    OrderNumber = model.OrderNumber,
                    InvoiceId = model.InvoiceId,
                    PaymentType = model.PaymentType,
                    OrderDate = orderDate,
                    CreatedAt = DateTime.Now,
                    OrderDiscountType=model.OrderDiscountType,
                    OrderDiscountValue=model.OrderDiscountValue,
                    OrderDiscountAmount=orderDiscountAmount,

                    // TAX FIELDS - Now optional (will be null if no tax)
                    TaxType = !string.IsNullOrEmpty(model.TaxType) ? model.TaxType : null,
                    TaxValue = model.TaxValue,
                    TaxAmount = taxAmount > 0 ? taxAmount : (decimal?)null
                };

                long orderId = await _coreRepository.AddOrderAsync(newOrder);

                // ========================
                // SAVE ORDER ITEMS
                // ========================
                orderItems.ForEach(x => x.OrderId = orderId);

                await _coreRepository.AddOrderItemsAsync(orderItems);

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Order Created Successfully."
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }

        public async Task<object> GetOrderHistoryAsync(OrderHistoryRequest model, long merchantId)
        {
            try
            {
                var FromDate = !string.IsNullOrEmpty(model.FromDate) ? model.FromDate : null;
                var ToDate = !string.IsNullOrEmpty(model.ToDate) ? model.ToDate : null;

                var fromtime = "";
                var totime = "";

                if ((FromDate == null && ToDate == null) || (FromDate == "" && ToDate == ""))
                {
                    fromtime = null;
                    totime = null;
                }
                else
                {
                    fromtime = " 00:00:00.000";
                    totime = " 23:59:59.999";
                }
                DateTime now = DateTime.Now;

                var DateFrom = fromtime != null ? Convert.ToDateTime(FromDate + fromtime) : Convert.ToDateTime("1970-01-01 00:00:00.000");
                var DateTo = totime != null ? Convert.ToDateTime(ToDate + totime) : now;

                var list = await _coreRepository
                    .GetOrderHistoryAsync(merchantId, DateFrom, DateTo);

                var TotalRevenue = list.Sum(x => x.TotalAmount);

                return new
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success",
                    Data = list,
                    OrderCount=list.Count,
                    Revenue = TotalRevenue
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-GetOrderHistoryAsync", ex.Message, "CoreService.cs", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }

        public async Task<object> SearchCustomersAsync(string query, long merchantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new DefaultResponse { ResponseCode = "01", ResponseMessage = "Query is required." };
                }

                var customers = await _coreRepository.SearchCustomersAsync(merchantId, query);

                if (customers == null || customers.Count == 0)
                {
                    return new DefaultResponse { ResponseCode = "01", ResponseMessage = "No customer found." };
                }

                var customerData = customers.Select(c => new
                {
                    c.CustomerId,
                    c.CustomerName,
                    c.CustomerPhone,
                    c.DeliveryAddress
                });

                return new
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success",
                    Data = customerData
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-SearchCustomersAsync", ex.Message, "CoreService.cs", merchantId.ToString());
                return new DefaultResponse { ResponseCode = "05", ResponseMessage = "Service Failed" };
            }
        }

        public async Task<DefaultResponse> RefundOrderAsync(long OrderId,long merchantId,string userId)
        {
            try
            {
                var order = await _coreRepository.GetOrderByIdAsync(OrderId, merchantId);

                if (order.Id == 0 || order == null)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Order not found"
                    };
                }

                if (order.IsRefunded)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Order already refunded"
                    };
                }
                order.IsRefunded = true;
                order.RefundedAt = DateTime.Now;
                order.RefundedBy = userId;

                bool updated = await _coreRepository.UpdateOrderAsync(order);

                if (!updated)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Refund failed"
                    };
                }

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Order refunded successfully"
                };
            }
            catch(Exception ex)
            {
                await LogWrite("Error-RefundOrderAsync", ex.Message, "CoreService.cs:RefundOrderAsync", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service failed"
                };
            }
        }
        public async Task<ReportResponseDto> GetReportAsync(ReportRequestDto request, long merchantId)
        {
            try
            {
                // Parse dates
                if (!DateTime.TryParse(request.FromDate, out var fromDate) ||
                    !DateTime.TryParse(request.ToDate, out var toDate))
                {
                    return new ReportResponseDto
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Invalid date format. Use yyyy-MM-dd"
                    };
                }

                // Validate date range
                if (fromDate > toDate)
                {
                    return new ReportResponseDto
                    {
                        ResponseCode = "01",
                        ResponseMessage = "From date cannot be after to date"
                    };
                }

                // Get all report data
                var kpi = await _coreRepository.GetKpiDataAsync(merchantId, fromDate, toDate);
                var productStats = await _coreRepository.GetProductStatsAsync(merchantId, fromDate, toDate);
                var timeData = await _coreRepository.GetTimeDataAsync(merchantId, fromDate, toDate);

                // NEW DATA
                var taxSummary = await _coreRepository.GetTaxSummaryAsync(merchantId, fromDate, toDate);
                var discountSummary = await _coreRepository.GetDiscountSummaryAsync(merchantId, fromDate, toDate);
                var paymentMethodStats = await _coreRepository.GetPaymentMethodStatsAsync(merchantId, fromDate, toDate);
                var orderStats = await _coreRepository.GetOrderStatsAsync(merchantId, fromDate, toDate);

                // Log the report generation
                await _coreRepository.LogWriteAsync(
                    "ReportGenerated",
                    $"Report generated for {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                    "CoreService.GetReportAsync",
                    merchantId.ToString());

                return new ReportResponseDto
                {
                    ResponseCode = "00",
                    ResponseMessage = "Report generated successfully",
                    Data = new ReportDataDto
                    {
                        Kpi = kpi,
                        ProductStats = productStats,
                        TimeData = timeData,
                        TaxSummary = taxSummary,
                        DiscountSummary = discountSummary,
                        PaymentMethodStats = paymentMethodStats,
                        OrderStats = orderStats
                    }
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-GetReport", ex.Message, "CoreService.GetReportAsync", merchantId.ToString());

                return new ReportResponseDto
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong while generating report"
                };
            }
        }

        public async Task<EditOrderResponse> EditOrderAsync(EditOrderRequest request, long merchantId, string userId)
        {
            try
            {
                // 1. Get existing order
                var existingOrder = await _coreRepository.GetOrderForEditAsync(request.OrderId, merchantId);

                if (existingOrder == null || existingOrder.Id == 0)
                {
                    return new EditOrderResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Order not found"
                    };
                }

                // 2. Check if order is refunded
                if (existingOrder.IsRefunded)
                {
                    return new EditOrderResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Cannot edit refunded order"
                    };
                }

                // 3. Store old values for logging
                var oldValues = JsonConvert.SerializeObject(new
                {
                    existingOrder.CustomerId,
                    existingOrder.PaymentType,
                    existingOrder.TotalAmount,
                    existingOrder.ItemsCount,
                    existingOrder.TaxType,
                    existingOrder.TaxValue,
                    existingOrder.TaxAmount
                });

                // 4. Handle customer
                long customerId = existingOrder.CustomerId ?? 0;
                if (request.CustomerId.HasValue)
                {
                    var existingCustomer = await _coreRepository.GetCustomersbyCustomerId(merchantId, request.CustomerId ?? 0);

                    if (existingCustomer != null || existingCustomer.CustomerId != 0)
                    {
                        //var newCustomer = new Customers
                        //{
                        //    MerchantId = merchantId,
                        //    CustomerName = request.CustomerName,
                        //    CustomerPhone = request.CustomerPhone,
                        //    DeliveryAddress = request.DeliveryAddress,
                        //    CreatedAt = DateTime.Now
                        //};
                        existingCustomer.CustomerName = request.CustomerName;
                        existingCustomer.DeliveryAddress = request.DeliveryAddress;
                        existingCustomer.CustomerPhone = request.CustomerPhone;
                        await _coreRepository.UpdatCustomersAsync(existingCustomer);
                        //customerId = newCustomer.CustomerId;
                    }

                    //else
                    //{
                    //    var UpdateCustomer = new Customers
                    //    {
                    //        MerchantId = merchantId,
                    //        CustomerName = request.CustomerName,
                    //        CustomerPhone = request.CustomerPhone,
                    //        DeliveryAddress = request.DeliveryAddress,
                    //        CreatedAt = DateTime.Now
                    //    };
                    //}
                }

                // 5. Recalculate everything (same as AddOrder logic)
                decimal subTotal = 0m;
                decimal grossTotalAll = 0m;
                decimal totalItemDiscounts = 0m;
                int itemCount = 0;
                var orderItems = new List<OrderItems>();

                foreach (var item in request.Items)
                {
                    var product = await _coreRepository.GetProductById(item.ProductId, merchantId);
                    if (product == null)
                    {
                        return new EditOrderResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = $"Product not found: ID {item.ProductId}"
                        };
                    }

                    decimal unitPrice = product.ProductPrice;
                    decimal itemGross = unitPrice * item.Qty;
                    decimal itemDiscountAmount = 0m;

                    if (!string.IsNullOrEmpty(item.DiscountType) && item.DiscountValue.HasValue)
                    {
                        if (item.DiscountType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                        {
                            itemDiscountAmount = Math.Round((itemGross * item.DiscountValue.Value) / 100m, 2);
                        }
                        else if (item.DiscountType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                        {
                            itemDiscountAmount = Math.Round(item.DiscountValue.Value, 2);
                        }
                    }

                    decimal itemNet = Math.Round(itemGross - itemDiscountAmount, 2);

                    grossTotalAll += itemGross;
                    totalItemDiscounts += itemDiscountAmount;
                    subTotal += itemNet;
                    itemCount += item.Qty;

                    orderItems.Add(new OrderItems
                    {
                        OrderId = request.OrderId,
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        UnitPrice = unitPrice,
                        DiscountType = item.DiscountType,
                        DiscountValue = item.DiscountValue,
                        DiscountAmount = itemDiscountAmount,
                        GrossTotal = itemGross,
                        AmountAfterDiscount = itemNet,
                        TotalPrice = itemNet,
                        CreatedAt = DateTime.Now
                    });
                }

                // Apply order discount
                decimal orderDiscountAmount = 0m;
                if (!string.IsNullOrEmpty(request.OrderDiscountType) && request.OrderDiscountValue.HasValue)
                {
                    if (request.OrderDiscountType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        orderDiscountAmount = Math.Round((subTotal * request.OrderDiscountValue.Value) / 100m, 2);
                    }
                    else if (request.OrderDiscountType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        orderDiscountAmount = Math.Round(request.OrderDiscountValue.Value, 2);
                    }
                    if (orderDiscountAmount > subTotal) orderDiscountAmount = subTotal;
                }

                decimal amountAfterDiscount = Math.Round(subTotal - orderDiscountAmount, 2);
                decimal totalDiscountAll = Math.Round(totalItemDiscounts + orderDiscountAmount, 2);

                // ========================
                // CALCULATE TAX - NOW USING REQUEST TAX FIELDS
                // ========================
                decimal taxAmount = 0m;

                // Case 1: Tax fields provided in request
                if (!string.IsNullOrEmpty(request.TaxType) && request.TaxValue.HasValue && request.TaxValue > 0)
                {
                    if (request.TaxType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        taxAmount = Math.Round((amountAfterDiscount * request.TaxValue.Value) / 100m, 2);
                    }
                    else if (request.TaxType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        taxAmount = Math.Round(request.TaxValue.Value, 2);
                    }
                }
                // Case 2: No tax fields in request, keep existing tax
                else if (existingOrder.TaxValue > 0 && !string.IsNullOrEmpty(existingOrder.TaxType))
                {
                    if (existingOrder.TaxType.Equals("percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        taxAmount = Math.Round((amountAfterDiscount * existingOrder.TaxValue.Value) / 100m, 2);
                    }
                    else if (existingOrder.TaxType.Equals("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        taxAmount = Math.Round(existingOrder.TaxValue.Value, 2);
                    }
                }

                decimal finalPayable = amountAfterDiscount + taxAmount;

                // 6. Update order with all fields including tax
                existingOrder.CustomerId = customerId > 0 ? customerId : null;
                existingOrder.PaymentType = request.PaymentType;
                existingOrder.TotalAmount = finalPayable;
                existingOrder.GrossTotal = grossTotalAll;
                existingOrder.TotalDiscount = totalDiscountAll;
                existingOrder.ItemsCount = itemCount;
                existingOrder.OrderDiscountType = request.OrderDiscountType;
                existingOrder.OrderDiscountValue = request.OrderDiscountValue;
                existingOrder.OrderDiscountAmount = orderDiscountAmount;

                // UPDATE TAX FIELDS - Use request values if provided, otherwise keep existing
                existingOrder.TaxType = !string.IsNullOrEmpty(request.TaxType) ? request.TaxType : existingOrder.TaxType;
                existingOrder.TaxValue = request.TaxValue ?? existingOrder.TaxValue;
                existingOrder.TaxAmount = taxAmount;

                // 7. Update tracking fields - FIXED
                existingOrder.IsEdited = true;
                existingOrder.EditedBy = userId;
                existingOrder.EditedAt = DateTime.Now;
                existingOrder.EditCount = existingOrder.EditCount + 1; // ✅ FIXED - no null coalescing needed

                // 8. Save changes
                var updated = await _coreRepository.UpdateOrderWithItemsAsync(existingOrder, orderItems);

                if (!updated)
                {
                    return new EditOrderResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to update order"
                    };
                }

                // 9. Log the edit with tax fields included
                var newValues = JsonConvert.SerializeObject(new
                {
                    existingOrder.CustomerId,
                    existingOrder.PaymentType,
                    existingOrder.TotalAmount,
                    existingOrder.ItemsCount,
                    existingOrder.TaxType,
                    existingOrder.TaxValue,
                    existingOrder.TaxAmount
                });

                //var changes = $"Old: {oldValues} | New: {newValues}";
                //await _coreRepository.LogOrderEditAsync(request.OrderId, userId, changes);

                // 10. Get updated order for response
                var historyResult = await GetOrderHistoryAsync(new OrderHistoryRequest
                {
                    FromDate = DateTime.MinValue.ToString("yyyy-MM-dd"),
                    ToDate = DateTime.MaxValue.ToString("yyyy-MM-dd")
                }, merchantId);

                OrderHistoryResponse? updatedOrder = null;

                // Safely extract the order from the result using proper type checking
                if (historyResult != null)
                {
                    var historyType = historyResult.GetType();
                    var dataProperty = historyType.GetProperty("Data");

                    if (dataProperty != null)
                    {
                        var dataValue = dataProperty.GetValue(historyResult);
                        if (dataValue is List<OrderHistoryResponse> orders)
                        {
                            updatedOrder = orders.FirstOrDefault(o => o.Id == request.OrderId);
                        }
                    }
                }

                return new EditOrderResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Order updated successfully",
                    UpdatedOrder = updatedOrder
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-EditOrder", ex.Message, "CoreService.EditOrderAsync", merchantId.ToString());
                return new EditOrderResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong while editing order"
                };
            }
        }

        //Website Work
        public async Task<WebsiteConfigResponseDto?> GetWebsiteConfigBySubdomainAsync(string subdomain)
        {
            try
            {
                var config = await _coreRepository.GetWebsiteConfigBySubdomainAsync(subdomain);

                if (config == null)
                    return null;

                return MapToWebsiteConfigResponseDto(config);
            }
            catch (Exception ex)
            {
                await LogWrite("Error-GetWebsiteConfigBySubdomain", ex.Message, "CoreService.GetWebsiteConfigBySubdomainAsync", "System");
                return null;
            }
        }

        public async Task<WebsiteConfigResponseDto?> GetWebsiteConfigByMerchantIdAsync(long merchantId)
        {
            try
            {
                var config = await _coreRepository.GetWebsiteConfigByMerchantIdAsync(merchantId);

                if (config == null)
                    return null;

                return MapToWebsiteConfigResponseDto(config);
            }
            catch (Exception ex)
            {
                await LogWrite("Error-GetWebsiteConfigByMerchantId", ex.Message, "CoreService.GetWebsiteConfigByMerchantIdAsync", merchantId.ToString());
                return null;
            }
        }


        public async Task<DefaultResponse> UpdateWebsiteConfigAsync(long merchantId, UpdateWebsiteConfigDto updateDto)
        {
            try
            {
                var config = await _coreRepository.GetWebsiteConfigByMerchantIdAsync(merchantId);

                if (config == null)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Website configuration not found"
                    };
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateDto.LogoUrl))
                    config.LogoUrl = updateDto.LogoUrl;

                if (!string.IsNullOrEmpty(updateDto.PrimaryColor))
                    config.PrimaryColor = updateDto.PrimaryColor;

                if (!string.IsNullOrEmpty(updateDto.SecondaryColor))
                    config.SecondaryColor = updateDto.SecondaryColor;

                if (!string.IsNullOrEmpty(updateDto.BackgroundColor))
                    config.BackgroundColor = updateDto.BackgroundColor;

                if (!string.IsNullOrEmpty(updateDto.HeroTitle))
                    config.HeroTitle = updateDto.HeroTitle;

                if (!string.IsNullOrEmpty(updateDto.HeroDescription))
                    config.HeroDescription = updateDto.HeroDescription;

                if (updateDto.WorkingHours != null)
                    config.WorkingHours = JsonConvert.SerializeObject(updateDto.WorkingHours);

                if (!string.IsNullOrEmpty(updateDto.ContactPhone))
                    config.ContactPhone = updateDto.ContactPhone;

                if (!string.IsNullOrEmpty(updateDto.ContactEmail))
                    config.ContactEmail = updateDto.ContactEmail;

                if (!string.IsNullOrEmpty(updateDto.ContactAddress))
                    config.ContactAddress = updateDto.ContactAddress;

                if (updateDto.IsActive.HasValue)
                    config.IsActive = updateDto.IsActive.Value;

                bool updated = await _coreRepository.UpdateWebsiteConfigAsync(config);

                if (!updated)
                {
                    return new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to update website configuration"
                    };
                }

                await LogWrite("WebsiteConfigUpdated", $"Website config updated for merchant {merchantId}", "CoreService.UpdateWebsiteConfigAsync", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "00",
                    ResponseMessage = "Website configuration updated successfully"
                };
            }
            catch (Exception ex)
            {
                await LogWrite("Error-UpdateWebsiteConfig", ex.Message, "CoreService.UpdateWebsiteConfigAsync", merchantId.ToString());
                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong while updating website configuration"
                };
            }
        }


        // ==================== HELPER METHODS ====================

        private WebsiteConfigResponseDto MapToWebsiteConfigResponseDto(WebsiteConfig config)
        {
            var response = new WebsiteConfigResponseDto
            {
                Id = config.Id,
                MerchantId = config.MerchantId,
                Subdomain = config.Subdomain,
                LogoUrl = config.LogoUrl,
                PrimaryColor = config.PrimaryColor,
                SecondaryColor = config.SecondaryColor,
                BackgroundColor = config.BackgroundColor,
                HeroTitle = config.HeroTitle,
                HeroDescription = config.HeroDescription,
                ContactPhone = config.ContactPhone,
                ContactEmail = config.ContactEmail,
                ContactAddress = config.ContactAddress,
                IsActive = config.IsActive,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };

            // Parse working hours JSON if exists
            if (!string.IsNullOrEmpty(config.WorkingHours))
            {
                try
                {
                    response.WorkingHours = JsonConvert.DeserializeObject<List<WorkingHoursResponseDto>>(config.WorkingHours);
                }
                catch
                {
                    response.WorkingHours = new List<WorkingHoursResponseDto>();
                }
            }

            return response;
        }

        private string GenerateSubdomainFromName(string name)
        {
            // Convert to lowercase, replace spaces with hyphens, remove special characters
            string subdomain = name.ToLower()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace(".", "")
                .Replace("&", "and");

            // Remove any non-alphanumeric characters except hyphens
            subdomain = Regex.Replace(subdomain, @"[^a-z0-9-]", "");

            return subdomain;
        }

        public async Task<MenuResponseDto> GetMenuBySubdomainAsync(string subdomain)
        {
            try
            {
                return await _coreRepository.GetMenuBySubdomainAsync(subdomain);
            }
            catch (Exception ex)
            {
                await LogWrite("Error-GetMenuBySubdomain", ex.Message, "CoreService.GetMenuBySubdomainAsync", subdomain);
                return new MenuResponseDto();
            }
        }

        public async Task<List<City>> GetCityListService()
        {
            try
            {
                return await _coreRepository.GetCityList();
            }
            catch (Exception ex)
            {
                return new List<City>();
            }
        }
        public async Task<List<Branches>> GetBranchesByNameService(BranchDto branchDto, long MerchantId)
        {
            try
            {
                return await _coreRepository.GetBranchesByName(branchDto, MerchantId);
            }
            catch (Exception ex)
            {
                return new List<Branches>();
            }
        }

        public async Task<bool> AddBranchService(BranchDto branchDto, long MerchantId)
        {
            try
            {

                var SMBranches = new Branches
                {
                    BranchName = branchDto.BranchName,
                    Address = branchDto.Address,
                    CityID = branchDto.CityID,
                    Active = branchDto.Active,
                    CreationDate = DateTime.Now,
                    MerchantId = MerchantId,


                };
                return await _coreRepository.AddBranch(SMBranches);
            }
            catch (Exception ex)
            {
                await LogWrite("AddBranchService", "Error: " + ex.Message,"CoreService.cs - AddBranchService", MerchantId.ToString());
                return false;
            }
        }

        public async Task<SystemUsers> GetUserByUserIdService(string UserID)
        {
            try
            {

                return await _coreRepository.GetUserByUserIdAsync(UserID);


            }
            catch (Exception ex)
            {

                await LogWrite("Error-GetUserByUserIdService",ex.Message, "coreRepository.cs-GetUserByUserIdService", "System");
                return null;
            }
        }
        public async Task<List<UserRoles>> GetUserRolesListService()
        {
            try
            {
                return await _coreRepository.GetUserRolesList();
            }
            catch (Exception ex)
            {
                return new List<UserRoles>();
            }
        }
        public async Task<bool> AddUserAsync(AddBranchUserDto addBranchUserDto, long MerchantId)
        {
            try
            {

                //string pwd = string.Empty;
                //pwd = addMerchantUser.MPIN.ToString().Trim();
                //pwd = Decrypt(pwd);
                //pwd = CalculateMD5Hash(pwd);
                //addBranchUserDto.MPIN = pwd;

                var User = new SystemUsers
                {
                    MerchantId = MerchantId,
                    MobileNumber = addBranchUserDto.MobileNumber,
                    UserID = addBranchUserDto.UserID,
                    FullName = addBranchUserDto.FullName,
                    PasswordHash = addBranchUserDto.Password,
                    IsActive = addBranchUserDto.Active,
                    Email = addBranchUserDto.Email,
                    CreatedAt = DateTime.Now,
                    BranchId = addBranchUserDto.BranchId,
                    RoleId = addBranchUserDto.RoleId
                };

                return await _coreRepository.AddUserAsync(User);
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public async Task<List<Branches>> GetBranchesListService(long MerchantId, string Role, int BranchId)
        {
            try
            {
                if(Role == "BusinessAdmin")
                {

                    return await _coreRepository.GetBranchesList(MerchantId);
                }
                else
                {
                    return await _coreRepository.GetBranchesListById(BranchId);

                }

            }
            catch (Exception ex)
            {
                return new List<Branches>();
            }
        }

        public async Task<List<BranchUsersDto>> GetLocationsWithUsersAsync(long merchantId, string role, int? userLocationId)
        {
            try
            {
                List<Branches> branchs;

                // 1) Role location filter
                if (role == "BusinessAdmin")
                {
                    // BusinessAdmin: sab locations
                    branchs = await _coreRepository.GetLocationsByMerchantAsync(merchantId);
                }
                else
                {
                    // LocationAdmin/User: sirf apni assigned location
                    if (userLocationId.HasValue)
                    {
                        // ek single location ko list me lao
                        branchs = await _coreRepository.GetLocationsByBranchCodesAsync(new List<int> { userLocationId.Value });
                    }
                    else
                    {
                        branchs = new List<Branches>();
                    }
                }

                // 2) Agar koi location na mile to empty return
                if (branchs == null || !branchs.Any())
                {
                    return new List<BranchUsersDto>();
                }

                // 3) Us merchant ke sab users lo
                var users = await _coreRepository.GetUsersByMerchantAsync(merchantId, role, userLocationId);

                var result = new List<BranchUsersDto>();

                // 4) Har location ke liye users filter karke DTO banao
                foreach (var location in branchs)
                {
                    // Ab BranchCode string hai, isliye location.BranchCode use karenge
                    var usersInLocation = users
                        .Where(u => u.BranchId == location.Id)
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            UserId = u.UserID,
                            UserRole = "",
                            FullName = u.FullName,
                            MobileNumber = u.MobileNumber,
                            Email = u.Email,
                            CreationDate = u.CreatedAt.ToString("dd-MM-yyyy"),
                            Active = u.IsActive
                        })
                        .ToList();

                    result.Add(new BranchUsersDto
                    {
                        BranchId = location.Id, 
                        BranchName = location.BranchName,
                        Address = location.Address,
                        Active = location.Active,
                        CityID = location.CityID,
                        CreationDate = location.CreationDate?.ToString("dd-MM-yyyy"),
                        Users = usersInLocation
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogWrite("Error", ex.Message, "GetLocationsWithUsersAsync", merchantId.ToString());
                throw;
            }
        }
    }
}

