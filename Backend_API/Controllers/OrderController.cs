using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.Services;
using Backend_API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly BookstoreContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly EmailContentService _emailContentService;

        public OrderController(BookstoreContext context, IMapper mapper, IEmailSender emailSender,
            EmailContentService emailContentService)
        {
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _emailContentService = emailContentService;
        }

        // GET LIST ORDER
        [HttpGet]
        [Route("get_orders")]
        public async Task<ActionResult<OrderList>> GetOrders(
            int? page,
            int? pageSize, 
            bool? orderByDesc, 
            string? search, 
            int? status,
            DateTime? fromDate,
            DateTime? toDate
            )
        {
            IQueryable<Order> query = _context.Orders;

            // Apply filtering by status if the 'status' parameter is provided
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status);
            }

            // Apply searching if the 'search' parameter is provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Code.Contains(search) ||
                    o.Name.Contains(search) ||
                    o.Phone.Contains(search) ||
                    o.Email.Contains(search)
                    );
            }

            // Apply filtering by CreateAt datetime
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate);
            }

            if (toDate.HasValue)
            {
                // When using DateTime for filtering, it's common to include the end of the day.
                // So, toDate is often set to the end of the day (23:59:59).
                DateTime toDateEndOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(o => o.CreatedAt <= toDateEndOfDay);
            }

            // Calculate the total number of items matching the criteria
            int totalItems = await query.CountAsync();

            // Initialize variables for totalPages and itemsPerPage
            int? totalPages = null;
            int itemsPerPage = 0;

            // Apply pagination if the 'page' and 'pageSize' parameters are provided
            if (page.HasValue && pageSize.HasValue)
            {
                int currentPage = page.Value;
                itemsPerPage = pageSize.Value;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                query = query.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);
            }

            // Apply sorting by CreateAt if the 'orderByDescending' parameter is provided and true
            if (orderByDesc.HasValue && orderByDesc.Value)
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                query = query.OrderBy(o => o.CreatedAt);
            }

            var orders = await query.ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound();
            }

            //Map entities to DTOs
            var orderDTOs = _mapper.Map<List<OrderDTO>>(orders);

            var response = new OrderList
            {
                Orders = orderDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }

        [HttpGet("order_count")]
        public async Task<ActionResult<OrderCountResponse>> GetOrderCount(DateTime? fromDate, DateTime? toDate)
        {
            IQueryable<Order> query = _context.Orders;

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate);
            }

            if (toDate.HasValue)
            {
                DateTime toDateEndOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(o => o.CreatedAt <= toDateEndOfDay);
            }

            if (!fromDate.HasValue && !toDate.HasValue)
            {
                // If both fromDate and toDate are null, calculate for today
                DateTime today = DateTime.Today;
                query = query.Where(o => o.CreatedAt >= today);
            }

            int totalOrders = await query.CountAsync();
            int confirmedOrders = await query.CountAsync(o => o.Status == 1);
            int shippingOrders = await query.CountAsync(o => o.Status == 3);
            int deliveredOrders = await query.CountAsync(o => o.Status == 4);

            var response = new OrderCountResponse
            {
                TotalOrders = totalOrders,
                ConfirmedOrders = confirmedOrders,
                ShippingOrders = shippingOrders,
                DeliveredOrders = deliveredOrders
            };

            return response;
        }

        // GET BY CODE
        [HttpGet("{code}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(string code)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.ReturnRequest)
                .FirstOrDefaultAsync(o => o.Code == code);

            if (order == null)
            {
                return NotFound();
            }

            var orderDTO = _mapper.Map<OrderDTO>(order);
            return orderDTO;
        }

        // theo doi don hang
        [HttpGet("order_tracking")]
        public async Task<ActionResult<OrderDTO>> GetOrderByCodeAndEmail(string code, string email)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }

            // Find the order by code
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.ReturnRequest)
                .FirstOrDefaultAsync(o => o.Code == code);

            if (order == null)
            {
                return NotFound();
            }

            // Check if the provided email matches the order's email
            if (order.Email != email)
            {
                return NotFound();
            }

            var orderDTO = _mapper.Map<OrderDTO>(order);

            return orderDTO;
        }

        [HttpPatch("change_order_status/{code}")]
        public async Task<IActionResult> ChangeOrderStatus(string code, [FromBody] int status)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Code == code);

                if (order == null)
                {
                    return NotFound();
                }

                // Update the order status
                order.Status = status;
                order.UpdatedAt = DateTime.Now;

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the order status: {ex.Message}");
            }
        }

        [HttpPatch("confirm_payment/{code}")]
        public async Task<IActionResult> ConfirmOrderPayment(string code)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.Code == code);

                if (order == null)
                {
                    return NotFound();
                }

                // Check if the current order status is 0
                if (order.Status != 0)
                {
                    return BadRequest("Order status is not 0.");
                }

                // Update the order status to 1 and set the updatedAt property
                order.Status = 1;
                order.UpdatedAt = DateTime.Now;

                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = await _context.Products.FindAsync(orderProduct.ProductId);
                    if (product != null)
                    {
                        // Reduce the product quantity based on the OrderProduct's quantity
                        product.Quantity -= orderProduct.Quantity;
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                // Reduce the quantity of coupon if coupon is applied
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                    if (coupon != null)
                    {
                        coupon.Quantity -= 1;
                        _context.Entry(coupon).State = EntityState.Modified;
                    }
                }


                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var orderDTO = _mapper.Map<OrderDTO>(order);
                // Generate email content for the "Order Confirmed" email
                string emailContent = _emailContentService.ConstructOrderConfirmedEmailContent(orderDTO);

                // Send the email
                var message = new Message(new string[] { "nguyenduyquoc129829042001@gmail.com" }, "Order Confirmed", emailContent);
                await _emailSender.SendEmailAsync(message);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the order status: {ex.Message}");
            }
        }

        // POST: api/Order
        [HttpPost("post")]
        public async Task<ActionResult<Order>> PostOrder(OrderCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var orderProduct in model.OrderProducts)
            {
                // Validate product quantity
                var product = await _context.Products.FindAsync(orderProduct.ProductId);
                if (product == null || product.Quantity < orderProduct.Quantity)
                {
                    return BadRequest("Not enough stock for a product in the order.");
                }
            }

            if (!string.IsNullOrEmpty(model.CouponCode))
            {
                // Validate the coupon quantity
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == model.CouponCode);

                if (coupon == null || coupon.Quantity == 0)
                {
                    return BadRequest("Coupon code has been use up.");
                }
            }

            var order = _mapper.Map<Order>(model);

            // Set the order status based on the payment method
            if (model.PaymentMethod == "PAYPAL" || model.PaymentMethod == "VNPAY")
            {
                order.Status = 0; // PENDING
            }
            else if (model.PaymentMethod == "COD")
            {
                order.Status = 1; // CONFIRMED
                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = await _context.Products.FindAsync(orderProduct.ProductId);
                    if (product != null)
                    {
                        // Reduce the product quantity based on the OrderProduct's quantity
                        product.Quantity -= orderProduct.Quantity;
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                // Reduce the quantity of coupon if coupon is applied
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                    if (coupon != null)
                    {
                        coupon.Quantity -= 1;
                        _context.Entry(coupon).State = EntityState.Modified;
                    }
                }

            }
            else
            {
                // Handle other payment methods or invalid cases here
                return BadRequest("Invalid payment method");
            }

            // Generate a unique order code
            order.Code = GenerateUniqueOrderCode(order.PaymentMethod);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Eagerly load the OrderProduct information and include it in the response
            var createdOrder = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (createdOrder == null)
            {
                return NotFound();
            }

            var createdOrderDTO = _mapper.Map<OrderDTO>(createdOrder);

            if (createdOrderDTO.PaymentMethod == "COD")
            {
                // Generate email content for the "Order Confirmed" email
                string emailContent = _emailContentService.ConstructOrderConfirmedEmailContent(createdOrderDTO);

                // Send the email with the populated template
                var message = new Message(new string[] { "nguyenduyquoc129829042001@gmail.com" }, "Order Confirmed", emailContent);
                await _emailSender.SendEmailAsync(message);
            }
            return CreatedAtAction(nameof(GetOrder), new { code = order.Code }, createdOrderDTO);
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("cancel_order")]
        public async Task<IActionResult> CancelOrder(CancelOrderModel model)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Code == model.Code);

                if (order == null || order.Email != model.Email)
                {
                    return NotFound();
                }

                if (order.Status < 3) // Check if the order is cancelable
                {
                    // Update the order status to canceled (6)
                    order.Status = 6;
                    order.CancelReason = model.CancelReason;
                    order.UpdatedAt = DateTime.Now;

                    _context.Entry(order).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return Ok();
                }

                return BadRequest("This order is not cancelable.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while canceling the order: {ex.Message}");
            }
        }

        [HttpPost("confirm_received_order")]
        public async Task<IActionResult> ConfirmReceivedOrder(OrderReceivedConfirmModel model)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Code == model.Code);

                if (order == null || order.Email != model.Email)
                {
                    return NotFound();
                }

                if (order.Status == 4) // Check if the order is delivered
                {
                    // Update the order status to completed (5)
                    order.Status = 5;
                    order.UpdatedAt = DateTime.Now;

                    _context.Entry(order).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return Ok();
                }

                return BadRequest("This order can not be confirmed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while canceling the order: {ex.Message}");
            }
        }

        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        private string GenerateUniqueOrderCode(string paymentMethod)
        {
            string uniqueOrderCode;
            do
            {
                // Generate a random unique string (alphanumeric)
                var uniqueString = GenerateRandomString();
                uniqueOrderCode = $"{GetPaymentMethodCode(paymentMethod)}{DateTime.Now:yyMMddHHmmss}{uniqueString}";
            } while (_context.Orders.Any(o => o.Code == uniqueOrderCode));

            return uniqueOrderCode;
        }

        private string GetPaymentMethodCode(string paymentMethod)
        {
            switch (paymentMethod)
            {
                case "PAYPAL":
                    return "PPL";
                case "VNPAY":
                    return "VNP";
                case "COD":
                    return "COD";
                default:
                    return string.Empty;
            }
        }

        private string GenerateRandomString(int length = 4)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var uniqueString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return uniqueString;
        }
    }
}
