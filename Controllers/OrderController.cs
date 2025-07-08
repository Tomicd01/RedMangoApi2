using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoApi.Data;
using RedMangoApi.Models;
using RedMangoApi.Models.Dto;
using RedMangoApi.Services;
using RedMangoApi.Utility;

namespace RedMangoApi.Controllers
{
    [Route("api/Order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private ApiResponse _response;
        private readonly IBlobService _blobService;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _context.OrderHeaders
                    .Include(oh => oh.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .OrderByDescending(oh => oh.OrderHeaderId);
                if (!string.IsNullOrEmpty(userId))
                {
                    _response.Result = orderHeaders.Where(oh => oh.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = orderHeaders;
                }

                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message.ToString());
                return BadRequest(_response);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrderById(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id does not exist");
                    return BadRequest(_response);
                }
                
                var orderHeaders = _context.OrderHeaders
                    .Include(oh => oh.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Where(oh => oh.OrderHeaderId == id);
                if (orderHeaders == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Order not found");
                    return NotFound(_response);
                }

                _response.IsSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                _response.Result = orderHeaders;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message.ToString());
                return BadRequest(_response);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromForm] OrderHeaderCreateDto orderHeaderDto)
        {
            try
            {
                OrderHeader order = new()
                {
                    PickupName = orderHeaderDto.PickupName,
                    PickupEmail = orderHeaderDto.PickupEmail,
                    PickupPhoneNumber = orderHeaderDto.PickupPhoneNumber,
                    ApplicationUserId = orderHeaderDto.ApplicationUserId,
                    OrderTotal = orderHeaderDto.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentId = orderHeaderDto.StripePaymentId,
                    TotalItems = orderHeaderDto.TotalItems,
                    Status = string.IsNullOrEmpty(orderHeaderDto.Status) ? SD.status_pending : orderHeaderDto.Status
                };

                if (ModelState.IsValid)
                {
                    //if form is successfully completed, add order to database
                    await _context.OrderHeaders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    foreach (var orderDetailDto in orderHeaderDto.OrderDetailsDto)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDto.ItemName,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Price = orderDetailDto.Price,
                            Quantity = orderDetailDto.Quantity
                        };

                        await _context.OrderDetails.AddAsync(orderDetails);
                    }
                    await _context.SaveChangesAsync();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = System.Net.HttpStatusCode.Created;
                    return Ok(_response);
                }

                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Model not valid");
                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message.ToString());
                return BadRequest(_response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateOrder(int id, [FromForm] OrderHeaderUpdateDto headerUpdateDto)
        {
            try
            {
                if (headerUpdateDto == null || id != headerUpdateDto.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Wrong data entered");
                    return BadRequest(_response);
                }

                OrderHeader orderToUpdate = await _context.OrderHeaders
                    .FirstOrDefaultAsync(oh => oh.OrderHeaderId == id);

                if (orderToUpdate == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add($"Order with id={id} not found");
                    return BadRequest(_response);
                }

                if (!string.IsNullOrEmpty(headerUpdateDto.PickupName))
                {
                    orderToUpdate.PickupName = headerUpdateDto.PickupName;
                }
                if (!string.IsNullOrEmpty(headerUpdateDto.PickupEmail))
                {
                    orderToUpdate.PickupEmail = headerUpdateDto.PickupEmail;
                }
                if (!string.IsNullOrEmpty(headerUpdateDto.PickupPhoneNumber))
                {
                    orderToUpdate.PickupPhoneNumber = headerUpdateDto.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(headerUpdateDto.StripePaymentId))
                {
                    orderToUpdate.StripePaymentId = headerUpdateDto.StripePaymentId;
                }
                if (!string.IsNullOrEmpty(headerUpdateDto.Status))
                {
                    orderToUpdate.Status = headerUpdateDto.Status;
                }

                _context.SaveChanges();
                _response.IsSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message.ToString());
                return BadRequest(_response);
            }
            return _response;
        }
    }
}
