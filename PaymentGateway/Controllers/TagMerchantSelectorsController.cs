using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.Tags;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin, ServiceProvider")]
    public class TagMerchantSelectorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagMerchantSelectorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TagMerchantSelectors
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchant = await _context
                .Merchant
                .Include(x=>x.Tags)
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if(merchant == null)
            {
                return NotFound();
            }

            var tags = _context.TagMerchant.Where(x => x.User.Id == GetUserId());

            var ids = merchant.Tags.Select(x => x.TagId);

            var data = tags.Select(x => new TagMerchantItem { TagName = x.Name, IsSelected = ids.Contains(x.Id), TagId = x.Id }).ToList();

            return View(new TagMerchantSelector { MerchantId = id.Value, TagMerchantItems = data });
        }

        [HttpPost]
        public async Task<IActionResult> Index(TagMerchantSelector data)
        {
            var merchant = await _context
                .Merchant
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == data.MerchantId);

            if (merchant == null)
            {
                return NotFound();
            }

            var tagIds = data.TagMerchantItems.Where(x => x.IsSelected).Select(x => x.TagId);

            var newTagsList = await _context
                .TagMerchant
                .Where(x => x.User.Id == GetUserId() && tagIds.Contains(x.Id))
                .Select(x => new TagMerchantConnection { MerchantId = data.MerchantId, TagId = x.Id, })
                .ToListAsync();

            var oldRecords = await _context.TagMerchantConnection.Where(x => x.MerchantId == data.MerchantId).ToArrayAsync();
            _context.TagMerchantConnection.RemoveRange(oldRecords);

            await _context.TagMerchantConnection.AddRangeAsync(newTagsList);


            await _context.SaveChangesAsync();

            return View(data);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
