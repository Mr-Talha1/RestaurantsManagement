using BIPL_RAASTP2M.Data;
using BIPL_RAASTP2M.DTO;
using BIPL_RAASTP2M.Models;
using BIPL_RAASTP2M.Repositories;
using BIPL_RAASTP2M.Security;
using BIPL_RAASTP2M.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Threading.Channels;


namespace BIPL_RAASTP2M.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CoreController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ICoreService _coreService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _appDbContext;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly HttpClient _httpClient;
        private readonly ICoreRepository _coreRepository;

        public CoreController(ICoreService coreService, IConfiguration configuration, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtIssuerOptions, HttpClient httpClient, ICoreRepository coreRepository)
        {
            _httpClient = httpClient;
            _coreService = coreService;
            _configuration = configuration;
            _jwtFactory = jwtFactory;
            _jwtIssuerOptions = jwtIssuerOptions.Value;
            _coreRepository = coreRepository;
        }

        //-------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            try
            {
                var result = await _coreService.LoginServiceAsync(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-Login", ex.Message, "CoreController:Login", model.UserId ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }
        //DiningTable
        [HttpGet("GetDiningTables")]
        [Authorize]
        public async Task<IActionResult> GetDiningTables()
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.UserID == null)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;

                // Call service
                var result = await _coreService.GetDiningTablesService(tokenData.MerchantId, UserID);

                var response = new
                {
                    responseCode = "00",
                    ResponseMessage = "DiningTables List",
                    data = result
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetDiningTables", ex.Message, "CoreController:GetDiningTables", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });

            }
        }

        [HttpPost("AddDiningTable")]
        [Authorize]
        public async Task<IActionResult> AddDiningTable([FromBody] DiningTableDto req)
        {
                var UserID = "";
                try
                {
                    // Token Validate
                    var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    var tokenData = await _jwtFactory.ValidateJwtToken(token);
                    if (tokenData == null || tokenData.UserID == null)
                    {
                        return Ok(new DefaultResponse
                        {
                            ResponseCode = "04",
                            ResponseMessage = "user is unauthorized",
                        });
                    }

                    UserID = tokenData.UserID;

                    long merchantId = tokenData.MerchantId;

                    // Validation
                    if (string.IsNullOrWhiteSpace(req.Name))
                    {
                        return Ok(new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Table name is required",
                        });
                    }

                    // Call service
                    var result = await _coreService.AddDiningTableService(req, merchantId);

                if (result)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Table Added Successfully.",
                    });
                }
                else {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to Add Table.",
                    });
                }

                }
                catch (Exception ex)
                {
                    await _coreService.LogWrite("Error-AddDiningTable", ex.Message, "CoreController:AddDiningTable", UserID ?? "System");
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "05",
                        ResponseMessage = "Something Went Wrong",
                    });
                }
            }
        [HttpPost("UpdateDiningTable")]
        [Authorize]
        public async Task<IActionResult> UpdateDiningTable([FromBody] DiningTableDto request)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenData = await _jwtFactory.ValidateJwtToken(token);
            if (tokenData == null || tokenData.UserID == null)
            {
                return Ok(new DefaultResponse
                {
                    ResponseCode = "04",
                    ResponseMessage = "user is unauthorized",
                });
            }

            var response = await _coreService.UpdateDiningTableAsync(request, tokenData.MerchantId);
            return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-UpdateDiningTable", ex.Message, "CoreController:UpdateDiningTable", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }
        [HttpPost("DeleteDiningTable/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteDiningTable(int id)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenData = await _jwtFactory.ValidateJwtToken(token);
            if (tokenData == null || tokenData.MerchantId <= 0)
            {
                return Ok(new DefaultResponse
                {
                    ResponseCode = "04",
                    ResponseMessage = "user is unauthorized",
                });
            }
                UserID = tokenData.UserID;
            var result = await _coreService.DeleteDiningTableAsync(id, tokenData.MerchantId);
            return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-DeleteDiningTable", ex.Message, "CoreController:DeleteDiningTable", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }
        //Category
        [HttpPost("AddCategory")]
        [Authorize]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto request)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenData = await _jwtFactory.ValidateJwtToken(token);
            if (tokenData == null || tokenData.MerchantId <= 0)
            {
                return Ok(new DefaultResponse
                {
                    ResponseCode = "04",
                    ResponseMessage = "user is unauthorized",
                });
            }

            var result = await _coreService.AddCategoryService(request, tokenData.MerchantId);
            return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-AddCategory", ex.Message, "CoreController:AddCategory", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }

        [HttpPost("UpdateCategory")]
        [Authorize]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDto categoryDto)
        {
            var UserID = "";
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DefaultResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Invalid request payload",
                    });
                }
                UserID = tokenData.UserID;
                var result = await _coreService.UpdateCategoryAsync(categoryDto,tokenData.MerchantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("UpdateCategory-Error", ex.Message, "CoreController.cs:UpdateCategory", UserID ?? "System");

                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "something went wrong"
                });
            }
        }
        [HttpGet("GetCategories")]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.UserID == null)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;

                // Call service
                var result = await _coreService.GetCategoryService(tokenData.MerchantId);

                var response = new
                {
                    responseCode = "00",
                    ResponseMessage = "Categories List",
                    data = result,
                    count=result.Count
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetCategories", ex.Message, "CoreController:GetCategories", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });

            }
        }
        [HttpPost("DeleteCategory/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }
                UserID = tokenData.UserID;
                var result = await _coreService.DeleteCategoryService(id, tokenData.MerchantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-DeleteCategory", ex.Message, "CoreController:DeleteCategory", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }
        //Product
        [HttpPost("AddProduct")]
        [Authorize]
        [RequestSizeLimit(10_000_000)] // e.g. 10 MB
        public async Task<IActionResult> AddProduct([FromForm] AddProductRequest request)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                // Basic validation
                if (string.IsNullOrWhiteSpace(request.ProductName)) {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Name required",
                    });
                }
                var result = await _coreService.AddProductAsync(request, tokenData.MerchantId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-DeleteCategory", ex.Message, "CoreController:DeleteCategory", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }
        [HttpGet("GetProducts")]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.UserID == null)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;

                // Call service
                var result = await _coreService.GetProductsService(tokenData.MerchantId);

                var response = new
                {
                    responseCode = "00",
                    ResponseMessage = "Products List",
                    data = result,
                    count = result.Count
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetProducts", ex.Message, "CoreController:GetProducts", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });

            }
        }
        [HttpPost("UpdateProduct")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct([FromForm] AddProductRequest addProductRequest)
        {
            var UserID = "";
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DefaultResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Invalid request payload",
                    });
                }
                UserID = tokenData.UserID;
                var result = await _coreService.UpdateProductAsync(addProductRequest, tokenData.MerchantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("UpdateProduct-Error", ex.Message, "CoreController.cs:UpdateProduct", UserID ?? "System");

                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "something went wrong"
                });
            }
        }

        [HttpPost("DeleteProduct/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }
                UserID = tokenData.UserID;
                var result = await _coreService.DeleteProductService(id, tokenData.MerchantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-DeleteProduct", ex.Message, "CoreController:DeleteProduct", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }

        [HttpPost("AddOrder")]
        [Authorize]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderRequest model)
        {
            var UserID = "";
            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;
                //long merchantId = GetMerchantIdFromToken();
            //int userId = GetUserIdFromToken();

            var response = await _coreService.AddOrderAsync(model, tokenData.MerchantId, 1);
            return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-DeleteProduct", ex.Message, "CoreController:DeleteProduct", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }

        [HttpPost("OrderHistory")]
        [Authorize]
        public async Task<IActionResult> OrderHistory([FromBody] OrderHistoryRequest request)
        {
            var UserID = "";

            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;

                var response = await _coreService.GetOrderHistoryAsync(request, tokenData.MerchantId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-OrderHistory", ex.Message, "CoreController:OrderHistory", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }

        [HttpGet("SearchCustomer")]
        [Authorize]
        public async Task<IActionResult> SearchCustomer(string query)
        {
            var UserID = "";

            try
            {
                // Token Validate
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);
                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                UserID = tokenData.UserID;

            var result = await _coreService.SearchCustomersAsync(query, tokenData.MerchantId);

            return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-SearchCustomer", ex.Message, "CoreController:SearchCustomer", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something Went Wrong",
                });
            }
        }

    }

}
