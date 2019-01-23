using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string variant = null)
        {
            SearchModelPayments model = new SearchModelPayments
            {
                Data = await _context.Payment
                .AsNoTracking()
                .Include(x=>x.ServiceProvider)
                .Include(x=>x.Store)
                .Include(x=>x.Terminal)
                .ToListAsync()
            };

            return View(model);
        }
    }

    public class SearchModelPayments
    {
        public string ItemTypeTo { get => "Payments"; }

        public int GetStatusCount(Graft.Infrastructure.PaymentStatus status)
        {
            return Data.Count(x => x.Status == status);
        }

        public int FailedCount
        {
            get => Data.Count(x => new[]
            {
                Graft.Infrastructure.PaymentStatus.TimedOut,
                Graft.Infrastructure.PaymentStatus.RejectedByWallet,
                Graft.Infrastructure.PaymentStatus.RejectedByPOS,
                Graft.Infrastructure.PaymentStatus.NotEnoughAmount,
                Graft.Infrastructure.PaymentStatus.Fail,
                Graft.Infrastructure.PaymentStatus.DoubleSpend
            }.Contains(x.Status));
        }

        public int InProgressCount
        {
            get => Data.Count(x => new[]
            {
                Graft.Infrastructure.PaymentStatus.InProgress,
                Graft.Infrastructure.PaymentStatus.New,
                Graft.Infrastructure.PaymentStatus.Waiting
            }.Contains(x.Status));
        }

        public int SuccessfulCount
        {
            get => Data.Count(x => new[]
            {
                Graft.Infrastructure.PaymentStatus.Received,
                Graft.Infrastructure.PaymentStatus.Confirmed
            }.Contains(x.Status));
        }

        public Dictionary<string, int> DataSortedByTerminal
        {
            get => Data.GroupBy(x => x.Terminal).ToDictionary(x => x.Key.Name, y => y.Count());
        }

        public Dictionary<string, int> DataSortedByStore
        {
            get => Data.GroupBy(x => x.Store).ToDictionary(x => x.Key.Name, y => y.Count());
        }

        public Dictionary<string, int> DataSortedByServiceProvider
        {
            get => Data.GroupBy(x => x.ServiceProvider).ToDictionary(x => x.Key.Name, y => y.Count());
        }

        public List<Payment> Data { get; set; } = new List<Payment>();

        public int TotalCount => Data.Count;
    }
}