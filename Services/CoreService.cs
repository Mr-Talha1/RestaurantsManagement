using BIPL_RAASTP2M.Models;
using BIPL_RAASTP2M.Repositories;
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
using BIPL_RAASTP2M.Security;
using System.ComponentModel.DataAnnotations;
using BIPL_RAASTP2M.DTO;
using Microsoft.EntityFrameworkCore;

namespace BIPL_RAASTP2M.Services
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
                            MerchantName = GetMerchnat.Name,
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
                        Role = GetUser.Role,
                        MerchantId = GetMerchnat.Id,
                        MerchantName = GetMerchnat.Name,
                        LogoPath = GetMerchnat.LogoPath
                    },
                   Token = await _jwtFactory.LoginToken(GetUser.Role,GetUser.UserID, GetMerchnat.Id.ToString())
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
        public async Task<bool> AddDiningTableService(DiningTableDto req, long merchantId)
        {
            DiningTables table = new DiningTables
            {
                MerchantId = merchantId,
                Name = req.Name,
                IsDeleted=false,
                CreatedAt = DateTime.Now
            };

            return await _coreRepository.AddDiningTableAsync(table);
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
                    CreatedAt = DateTime.UtcNow
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

        public async Task<object> AddOrderAsyncbk(AddOrderRequest model, long merchantId, int userId)
        {
            try
            {
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

                decimal totalAmount = 0;
                int itemCount = 0;

                var orderItems = new List<OrderItems>();

                // ========================
                // VALIDATE EACH PRODUCT
                // ========================
                foreach (var item in model.Items)
                {
                    var product = await _coreRepository.GetProductById(item.ProductId, merchantId);

                    if (product == null || product.Id == 0)
                    {
                        return new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = $"Product not found: ID {item.ProductId}"
                        };
                    }

                    decimal unitPrice = product.ProductPrice;
                    decimal total = unitPrice * item.Qty;

                    totalAmount += total;
                    itemCount += item.Qty;

                    orderItems.Add(new OrderItems
                    {
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        UnitPrice = unitPrice,
                        TotalPrice = total,
                        CreatedAt = DateTime.Now
                    });
                }

                // ========================
                // INSERT ORDER
                // ========================
                var newOrder = new Orders
                {
                    MerchantId = merchantId,
                    UserId = userId,
                    OrderType = model.OrderType,
                    TableId = model.TableId,
                    TotalAmount = totalAmount,
                    ItemsCount = itemCount,
                    OrderNumber = model.OrderNumber, 
                    PaymentType=model.PaymentType,
                    OrderDate=model.OrderDate,
                    CreatedAt = DateTime.Now
                };

                long orderId = await _coreRepository.AddOrderAsync(newOrder);

                // ========================
                // SAVE ORDER ITEMS
                // ========================
                orderItems.ForEach(x => x.OrderId = orderId);

                await _coreRepository.AddOrderItemsAsync(orderItems);

                return new
                {
                    ResponseCode = "00",
                    ResponseMessage = "Order Created Successfully."
                };
            }
            catch (Exception ex)
            {
                await LogWrite("AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }
        public async Task<object> AddOrderAsyncbk1(AddOrderRequest model, long merchantId, int userId)
        {
            try
            {
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

                decimal totalAmount = 0;
                decimal _grossTotal = 0;
                decimal _discountAmount = 0;
                int itemCount = 0;

                var orderItems = new List<OrderItems>();

                // ========================
                // VALIDATE EACH PRODUCT
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
                    decimal grossTotal = unitPrice * item.Qty;
                    decimal discountAmount = 0;

                    // ========== APPLY DISCOUNT ==========
                    if (!string.IsNullOrEmpty(item.DiscountType) && item.DiscountValue.HasValue)
                    {
                        if (item.DiscountType.ToLower() == "percentage")
                        {
                            discountAmount = (grossTotal * item.DiscountValue.Value) / 100;

                        }
                        else if (item.DiscountType.ToLower() == "flat")
                        {
                            discountAmount = item.DiscountValue.Value;

                        }
                    }

                    // TOTAL PRICE AFTER DISCOUNT
                    decimal netTotal = grossTotal - discountAmount;

                    totalAmount += netTotal;
                    itemCount += item.Qty;
                    _grossTotal += grossTotal;
                    _discountAmount += discountAmount;

                    orderItems.Add(new OrderItems
                    {
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        UnitPrice = unitPrice,
                        DiscountType = item.DiscountType,
                        DiscountValue = item.DiscountValue,
                        DiscountAmount = discountAmount,
                        GrossTotal=grossTotal,
                        AmountAfterDiscount= netTotal,
                        TotalPrice = netTotal,
                        CreatedAt = DateTime.Now
                    });
                }


                // ========================
                // INSERT ORDER
                // ========================
                decimal? overallDiscount = orderItems.Sum(x => x.DiscountAmount);

                var newOrder = new Orders
                {
                    MerchantId = merchantId,
                    UserId = userId,
                    OrderType = model.OrderType,
                    TableId = model.TableId,
                    TotalAmount = totalAmount,
                    GrossTotal = _grossTotal,
                    TotalDiscount = _discountAmount,
                    ItemsCount = itemCount,
                    OrderNumber = model.OrderNumber,
                    PaymentType = model.PaymentType,
                    OrderDate = model.OrderDate,
                    CreatedAt = DateTime.Now
                };

                long orderId = await _coreRepository.AddOrderAsync(newOrder);

                // ========================
                // SAVE ORDER ITEMS
                // ========================
                orderItems.ForEach(x => x.OrderId = orderId);

                await _coreRepository.AddOrderItemsAsync(orderItems);

                return new
                {
                    ResponseCode = "00",
                    ResponseMessage = "Order Created Successfully."
                };
            }
            catch (Exception ex)
            {
                await LogWrite("AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }
        public async Task<object> AddOrderAsync(AddOrderRequest model, long merchantId, int userId)
        {
            try
            {
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

                DateTime? orderDate = model.OrderDate != default ? model.OrderDate : DateTime.UtcNow;

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

                // Final totals
                decimal totalDiscountAll = Math.Round(totalItemDiscounts + orderDiscountAmount, 2);
                decimal finalPayable = Math.Round(subTotal - orderDiscountAmount, 2);


                // ========================
                // CREATE ORDER (save)
                // ========================
                var newOrder = new Orders
                {
                    MerchantId = merchantId,
                    UserId = userId,
                    OrderType = model.OrderType,
                    TableId = model.TableId,
                    // TotalAmount store final payable (after all discounts)
                    TotalAmount = finalPayable,
                    GrossTotal = grossTotalAll,
                    TotalDiscount = totalDiscountAll,
                    ItemsCount = itemCount,
                    OrderNumber = model.OrderNumber,
                    PaymentType = model.PaymentType,
                    OrderDate = orderDate,
                    CreatedAt = DateTime.Now,
                    OrderDiscountType=model.OrderDiscountType,
                    OrderDiscountValue=model.OrderDiscountValue,
                    OrderDiscountAmount=orderDiscountAmount
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
                await LogWrite("AddOrderAsync", ex.Message, "CoreService.cs", merchantId.ToString());

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

                return new
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success",
                    Data = list
                };
            }
            catch (Exception ex)
            {
                await LogWrite("GetOrderHistoryAsync", ex.Message, "CoreService.cs", merchantId.ToString());

                return new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                };
            }
        }

    }
}

