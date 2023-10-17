using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
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

        public OrderController(BookstoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET LIST ORDER
        [HttpGet]
        [Route("get_orders")]
        public async Task<ActionResult<OrderList>> GetOrders(int? page, int? pageSize, bool? orderByDesc, string? search, int? status)
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
                query = query.Where(o => o.Name.Contains(search));
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
                .FirstOrDefaultAsync(o => o.Code == code);

            if (order == null)
            {
                return NotFound();
            }

            var orderDTO = _mapper.Map<OrderDTO>(order);
            return orderDTO;
        }

        // UPDATE
        /*[HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }*/

        [HttpPatch("changeorderstatus/{code}")]
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

        [HttpPatch("confirm-payment/{code}")]
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

                // Reduce the quantity of each gift in the OrderProduct array
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

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the order status: {ex.Message}");
            }
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = _mapper.Map<Order>(model);

            // Set the order status based on the payment method
            if (model.PaymentMethod == "PAYPAL" || model.PaymentMethod == "VNPAY")
            {
                order.Status = 0; // PENDING
            }
            else if (model.PaymentMethod == "COD")
            {
                order.Status = 1; // CONFIRMED
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
