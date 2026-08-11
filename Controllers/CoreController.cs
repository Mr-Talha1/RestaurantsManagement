using TBAppBackend.Data;
using TBAppBackend.DTO;
using TBAppBackend.Models;
using TBAppBackend.Repositories;
using TBAppBackend.Security;
using TBAppBackend.Services;
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


namespace TBAppBackend.Controllers
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
                return Ok(result);
               

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

            var response = await _coreService.AddOrderAsync(model, tokenData.MerchantId, UserID);
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

        [HttpGet("RefundOrder")]
        [Authorize]
        public async Task<IActionResult> RefundOrder(long OrderId)
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

                    var response = await _coreService.RefundOrderAsync(OrderId, tokenData.MerchantId,tokenData.UserID
                );

                return Ok(response);
            }
            catch(Exception ex)
            {
                 await _coreService.LogWrite("Error-RefundOrder", ex.Message, "CoreController:RefundOrder", UserID ?? "System");
                    return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                });
            }
        }
        [HttpPost("reports/summary")]
        [Authorize]
        public async Task<IActionResult> GetReportSummary([FromBody] ReportRequestDto request)
        {
            var UserID = "";
            try
            {
                // Validate token
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new ReportResponseDto
                    {
                        ResponseCode = "04",
                        ResponseMessage = "User is unauthorized"
                    });
                }

                UserID = tokenData.UserID;

                // Get report
                var result = await _coreService.GetReportAsync(request, tokenData.MerchantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-ReportSummary", ex.Message, "CoreController:GetReportSummary", UserID ?? "System");
                return Ok(new ReportResponseDto
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                });
            }
        }

        [HttpPost("EditOrder")]
        [Authorize]
        public async Task<IActionResult> EditOrder([FromBody] EditOrderRequest request)
        {
            var UserID = "";
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new EditOrderResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "User is unauthorized"
                    });
                }

                UserID = tokenData.UserID;

                var response = await _coreService.EditOrderAsync(request, tokenData.MerchantId, UserID);
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-EditOrder", ex.Message, "CoreController:EditOrder", UserID ?? "System");
                return Ok(new EditOrderResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                });
            }
        }


        [HttpGet("merchant/website")]
        [Authorize]
        public async Task<IActionResult> GetMerchantWebsite()
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
                        ResponseMessage = "User is unauthorized"
                    });
                }

                UserID = tokenData.UserID;
                var result = await _coreService.GetWebsiteConfigByMerchantIdAsync(tokenData.MerchantId);

                if (result == null)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Website configuration not found"
                    });
                }

                return Ok(new
                {
                    responseCode = "00",
                    responseMessage = "Success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetMerchantWebsite", ex.Message, "CoreController:GetMerchantWebsite", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                });
            }
        }

        /// <summary>
        /// Update website configuration for current merchant (Requires Auth)
        /// Used by POS app to customize website settings
        /// </summary>
        [HttpPut("merchant/website")]
        [Authorize]
        public async Task<IActionResult> UpdateMerchantWebsite([FromBody] UpdateWebsiteConfigDto updateDto)
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
                        ResponseMessage = "User is unauthorized"
                    });
                }

                UserID = tokenData.UserID;
                var result = await _coreService.UpdateWebsiteConfigAsync(tokenData.MerchantId, updateDto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-UpdateMerchantWebsite", ex.Message, "CoreController:UpdateMerchantWebsite", UserID ?? "System");
                return Ok(new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Something went wrong"
                });
            }
        }

        [HttpGet("public/website/{subdomain}/menu")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenuBySubdomain(string subdomain)
        {
            try
            {
                var result = await _coreService.GetMenuBySubdomainAsync(subdomain);

                return Ok(new
                {
                    responseCode = "00",
                    responseMessage = "Success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetMenuBySubdomain", ex.Message, "CoreController:GetMenuBySubdomain", subdomain);
                return StatusCode(500, new
                {
                    responseCode = "05",
                    responseMessage = "Something went wrong"
                });
            }
        }
        [HttpGet("public/website/{subdomain}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWebsiteBySubdomain(string subdomain)
        {
            try
            {
                var result = await _coreService.GetWebsiteConfigBySubdomainAsync(subdomain);

                if (result == null)
                {
                    return NotFound(new
                    {
                        responseCode = "404",
                        responseMessage = "Restaurant not found"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _coreService.LogWrite("Error-GetWebsiteBySubdomain", ex.Message, "CoreController:GetWebsiteBySubdomain", subdomain);
                return StatusCode(500, new
                {
                    responseCode = "05",
                    responseMessage = "Something went wrong"
                });
            }
        }

        // ===============  Branch work

        //GetCity
        [HttpGet("GetCity")]
        [Authorize]
        public async Task<IActionResult> GetCity()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(error => error.ErrorMessage));
            }
            try
            {

                var token = HttpContext.Request.Headers["Authorization"];
                token = token.ToString().Replace("Bearer ", "");

                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                var GetRole = await _coreService.GetCityListService();


                var response = new
                {
                    ResponseCode = "00",
                    CityList = GetRole


                };
                return Ok(response);

            }
            catch (WebException ex)
            {

                var response = new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "service fail",

                };
                return Ok(response);
            }


        }

        //AddBranch
        [HttpPost("AddBranch")]
        [Authorize]
        public async Task<IActionResult> AddBranch(BranchDto branchDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(error => error.ErrorMessage));
            }
            try
            {
                var token = HttpContext.Request.Headers["Authorization"];
                token = token.ToString().Replace("Bearer ", "");

                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                    var checkLocationExist = await _coreService.GetBranchesByNameService(branchDto, tokenData.MerchantId);
                    if (checkLocationExist.Count == 0)
                    {
                        var result = await _coreService.AddBranchService(branchDto, tokenData.MerchantId);

                        if (result == true)
                        {
                            var response = new DefaultResponse
                            {
                                ResponseCode = "00",
                                ResponseMessage = "Branches Added Successfully"


                            };
                            return Ok(response);
                        }
                        else
                        {
                            var response = new DefaultResponse
                            {
                                ResponseCode = "01",
                                ResponseMessage = "Some Thing Went Wrong Please Try Again Later"


                            };
                            return Ok(response);
                        }

                    }
                    else
                    {
                        var response = new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "BranchName Already Exist"


                        };
                        return Ok(response);

                    }

            }
            catch (WebException ex)
            {
                await _coreService.LogWrite("Error-AddBranch", ex.Message, "CoreController:AddBranch", "System");

                var response = new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Fail",

                };
                return Ok(response);
            }


        }

        //GetUserRoles
        [HttpGet("GetUserRoles")]
        [Authorize]
        public async Task<IActionResult> GetUserRoles()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(error => error.ErrorMessage));
            }
            try
            {

                var token = HttpContext.Request.Headers["Authorization"];
                token = token.ToString().Replace("Bearer ", "");

                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                var GetRole = await _coreService.GetUserRolesListService();


                var response = new
                {
                    ResponseCode = "00",
                    RoleList = GetRole


                };
                return Ok(response);

            }
            catch (WebException ex)
            {

                var response = new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "service fail",

                };
                return Ok(response);
            }


        }

        //AddBranchUser
        [HttpPost("AddBranchUser")]
        [Authorize]
        public async Task<IActionResult> AddBranchUser(AddBranchUserDto addBranchUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(error => error.ErrorMessage));
            }
            try
            {
                var token = HttpContext.Request.Headers["Authorization"];
                token = token.ToString().Replace("Bearer ", "");

                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                addBranchUserDto.UserID = addBranchUserDto.UserID.Trim();

                    var checkUserExist = await _coreService.GetUserByUserIdService(addBranchUserDto.UserID);

                    if (!string.IsNullOrEmpty(checkUserExist.UserID))
                    {
                        var response = new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "UserId already exist"
                        };
                        return Ok(response);
                    }


                    var result = await _coreService.AddUserAsync(addBranchUserDto, tokenData.MerchantId);

                    if (result == true)
                    {
                        var response = new DefaultResponse
                        {
                            ResponseCode = "00",
                            ResponseMessage = "User Added Successfully"


                        };
                        return Ok(response);
                    }
                    else
                    {
                        var response = new DefaultResponse
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Some Thing Went Wrong Please Try Again Later"


                        };
                        return Ok(response);
                    }
              

            }
            catch (WebException ex)
            {
                await _coreService.LogWrite("Error-AddBranchUser", ex.Message, "CoreController:AddBranchUser", "System");
                var response = new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Fail",

                };
                return Ok(response);
            }


        }

        //GetBranches
        [HttpGet("GetBranches")]
        [Authorize]
        public async Task<IActionResult> GetSMBranches()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(error => error.ErrorMessage));
            }
            try
            {

                var token = HttpContext.Request.Headers["Authorization"];
                token = token.ToString().Replace("Bearer ", "");

                var tokenData = await _jwtFactory.ValidateJwtToken(token);

                if (tokenData == null || tokenData.MerchantId <= 0)
                {
                    return Ok(new DefaultResponse
                    {
                        ResponseCode = "04",
                        ResponseMessage = "user is unauthorized",
                    });
                }

                var GetBranches = await _coreService.GetBranchesListService(tokenData.MerchantId, tokenData.Role, tokenData.BranchId);


                    var response = new
                    {
                        ResponseCode = "00",
                        BranchList = GetBranches


                    };
                    return Ok(response);
            
            }
            catch (WebException ex)
            {

                var response = new DefaultResponse
                {
                    ResponseCode = "05",
                    ResponseMessage = "service fail",

                };
                return Ok(response);
            }


        }

        //-- Get Branch With Users
        [HttpGet("GetBranchWithUsers")]
        [Authorize]
        public async Task<IActionResult> GetBranchWithUsers(int BranchId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(error => error.ErrorMessage));
            }
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                                        .ToString()
                                        .Replace("Bearer ", "");
                var getTokenDetails = await _jwtFactory.ValidateJwtToken(token);

                if (getTokenDetails == null)
                {
                    return Ok(new
                    {
                        ResponseCode = "04",
                        ResponseMessage = "Unauthorized"
                    });
                }


                long merchantId = getTokenDetails.MerchantId;
                string role = getTokenDetails.Role;
                int? userLocationId = role == "BusinessAdmin" ? null : BranchId;
                var LocationUser = await _coreService.GetLocationsWithUsersAsync(merchantId, role, userLocationId);


                var response = new
                {
                    ResponseCode = "00",
                    Data = LocationUser
                };
                return Ok(response);
            }
            catch (System.Exception ex)
            {

                await _coreService.LogWrite("GetBranchWithUsers ", ex.Message, "CoreController:GetBranchWithUsers","");

                return Ok(new
                {
                    ResponseCode = "05",
                    ResponseMessage = "Service Failed"
                });
            }
        }
    }

}
