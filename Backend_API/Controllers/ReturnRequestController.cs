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
    public class ReturnRequestController : ControllerBase
    {
        private readonly BookstoreContext _context;

        private readonly IMapper _mapper;

        private readonly IEmailSender _emailSender;

        private readonly EmailContentService _emailContentService;

        public ReturnRequestController(
            BookstoreContext context,
            IMapper mapper,
            IEmailSender emailSender,
            EmailContentService emailContentService
        )
        {
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _emailContentService = emailContentService;
        }

        // GET: api/return-request
        [HttpGet]
        public async Task<ActionResult<ReturnRequestList>> GetReturnRequests(
            int? page,
            int? pageSize,
            bool? orderByDesc,
            string? search,
            int? status,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            IQueryable<ReturnRequest> query = _context.ReturnRequests
                .Include(r => r.Order);

            // Apply filtering by status if the 'status' parameter is provided
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status);
            }

            // Apply searching if the 'search' parameter is provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.Order.Code.Contains(search) ||
                    r.Order.Name.Contains(search) ||
                    r.Order.Phone.Contains(search) ||
                    r.Order.Email.Contains(search)
                );
            }

            // Apply filtering by CreateAt datetime
            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate);
            }

            if (toDate.HasValue)
            {
                // When using DateTime for filtering, it's common to include the end of the day.
                // So, toDate is often set to the end of the day (23:59:59).
                DateTime toDateEndOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(r => r.CreatedAt <= toDateEndOfDay);
            }

            // Apply sorting by CreateAt if the 'orderByDescending' parameter is provided and true
            if (orderByDesc.HasValue && orderByDesc.Value)
            {
                query = query.OrderByDescending(r => r.CreatedAt);
            }
            else
            {
                query = query.OrderBy(r => r.CreatedAt);
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

            var returnRequests = await query.ToListAsync();

            if (returnRequests == null || returnRequests.Count == 0)
            {
                return NotFound();
            }

            //Map entities to DTOs
            var returnRequestDTOs = _mapper.Map<List<ReturnRequestDTO>>(returnRequests);

            var response = new ReturnRequestList
            {
                ReturnRequests = returnRequestDTOs,
                TotalPages = totalPages,
                TotalItems = totalItems,
            };

            return response;
        }

        // GET: api/return-request/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReturnRequestDTO>> GetReturnRequest(int id)
        {
            if (_context.ReturnRequests == null)
            {
                return NotFound();
            }
            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                .Include(r => r.ReturnRequestImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnRequest == null)
            {
                return NotFound();
            }

            var returnRequestDTO = _mapper.Map<ReturnRequestDTO>(returnRequest);

            return returnRequestDTO;
        }

       

        [HttpPatch("change_status/{id}")]
        public async Task<IActionResult> ChangeReturnRequestStatus(int id, [FromBody] int status)
        {
            try
            {
                var returnRequest = await _context.ReturnRequests
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (returnRequest == null)
                {
                    return NotFound();
                }

                // Update the request status
                returnRequest.Status = status;
                returnRequest.UpdatedAt = DateTime.Now;
                _context.Entry(returnRequest).State = EntityState.Modified;

                // If status = 3 (Return request COMPLETED) then change order status to 5 "COMPLETED"
                if (status == 3)
                {
                    returnRequest.Order.Status = 5;
                    returnRequest.Order.UpdatedAt = DateTime.Now;
                }


                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the return request status: {ex.Message}");
            }
        }

        // POST: api/return-request
        [HttpPost]
        public async Task<ActionResult> PostReturnRequest(ReturnRequestCreateModel model)
        {
            try
            {
                // Find the corresponding Order
                var order = await _context.Orders
                    .Include(o => o.OrderProducts)
                    .FirstOrDefaultAsync(o => o.Id == model.OrderId);

                if (order == null)
                {
                    return BadRequest("Order not found.");
                }

                // Create a new ReturnRequest
                var returnRequest = _mapper.Map<ReturnRequest>(model);
                returnRequest.Status = 0; // Set the initial status

                _context.ReturnRequests.Add(returnRequest);

                // Iterate through ReturnProducts to update ReturnQuantity in OrderProducts
                foreach (var returnProduct in model.ReturnProducts)
                {
                    // Find the corresponding OrderProduct
                    var orderProduct = order.OrderProducts.FirstOrDefault(op => op.Id == returnProduct.OrderProductId);

                    if (orderProduct != null)
                    {
                        // Update ReturnQuantity
                        orderProduct.ReturnQuantity = returnProduct.ReturnQuantity;
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Eagerly load the OrderProduct information and include it in the response
                var createdRequest = await _context.ReturnRequests
                    .Include(r => r.Order)
                        .ThenInclude(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                    .Include(r => r.ReturnRequestImages)
                    .FirstOrDefaultAsync(r => r.Id == returnRequest.Id);

                if (createdRequest == null)
                {
                    return NotFound();
                }

                var returnRequestDTO = _mapper.Map<ReturnRequestDTO>(createdRequest);


                // Generate email content for the "Return Request Received" email
                string emailContent = _emailContentService.ConstructReturnRequestCreateEmailContent(returnRequestDTO);
                //Send email
                var message = new Message(new string[] { "hungtranxd2697@gmail.com" }, "Return Request Received", emailContent);
                await _emailSender.SendEmailAsync(message);

                return CreatedAtAction(nameof(GetReturnRequest), new { id = returnRequest.Id }, returnRequestDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // xac nhan yeu cau
        [HttpPatch("confirm_request")]
        public async Task<IActionResult> ConfirmReturnRequest(ReturnRequestConfirm model)
        {
            try
            {
                var returnRequest = await _context.ReturnRequests
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

                if (returnRequest == null)
                {
                    return NotFound();
                }

                // Update the request status to "CONFIRMED"
                returnRequest.Status = 1;
                returnRequest.Response = model.Response;
                returnRequest.UpdatedAt = DateTime.Now;

                _context.Entry(returnRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var returnRequestDTO = _mapper.Map<ReturnRequestDTO>(returnRequest);
                // Generate email content for the "Return Request Confirmed" email
                string emailContent = _emailContentService.ConstructReturnRequestConfirmedEmailContent(returnRequestDTO);
                //Send email
                var message = new Message(new string[] { "hungtranxd2697@gmail.com" }, "Return Request Confirmed", emailContent);
                await _emailSender.SendEmailAsync(message);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while confirm the return request: {ex.Message}");
            }
        }


        // tu choi yeu cau tra hang
        [HttpPatch("decline_request")]
        public async Task<IActionResult> DeclineReturnRequest(ReturnRequestConfirm model)
        {
            try
            {
                var returnRequest = await _context.ReturnRequests
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

                if (returnRequest == null)
                {
                    return NotFound();
                }

                // Update the request status to "DECLINED"
                returnRequest.Status = 4;
                returnRequest.Response = model.Response;
                returnRequest.UpdatedAt = DateTime.Now;
                _context.Entry(returnRequest).State = EntityState.Modified;

                // Update the associated order to 5 "COMPLETED"
                returnRequest.Order.Status = 5;
                returnRequest.Order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var returnRequestDTO = _mapper.Map<ReturnRequestDTO>(returnRequest);
                // Generate email content for the "Return Request Declined" email
                string emailContent = _emailContentService.ConstructReturnRequestDeclinedEmailContent(returnRequestDTO);
                //Send email
                var message = new Message(new string[] { "hungtranxd2697@gmail.com" }, "Return Request Declined", emailContent);
                await _emailSender.SendEmailAsync(message);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while confirm the return request: {ex.Message}");
            }
        }

       

    }
}
