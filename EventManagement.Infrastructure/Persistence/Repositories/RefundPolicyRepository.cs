using System;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class RefundPolicyRepository : RepositoryBase<RefundPolicy>, IRefundPolicyRepository
    {
        public RefundPolicyRepository(EventManagementDbContext context) : base(context) { }

        public async Task<RefundPolicy?> GetApplicablePolicyAsync(Guid? eventId, string? ticketCategory)
        {
            if (eventId == null) return null;
            var now = DateTime.UtcNow;
            // Prefer exact match Event + Category; fallback Event only; then null.
            var query = _context.RefundPolicies
                .Include(p => p.RefundPolicyRules)
                .Where(p => p.IsEnabled == true && p.EventId == eventId && (p.EffectiveFrom == null || p.EffectiveFrom <= now) && (p.EffectiveTo == null || p.EffectiveTo >= now));

            RefundPolicy? selected = null;
            if (!string.IsNullOrWhiteSpace(ticketCategory))
            {
                selected = await query.Where(p => p.TicketCategory == ticketCategory).FirstOrDefaultAsync();
            }
            if (selected == null)
            {
                selected = await query.Where(p => p.TicketCategory == null).FirstOrDefaultAsync();
            }
            if (selected == null)
            {
                selected = await _context.RefundPolicies
                    .Include(p => p.RefundPolicyRules)
                    .Where(p => p.IsEnabled == true && p.EventId == null && (p.EffectiveFrom == null || p.EffectiveFrom <= now) && (p.EffectiveTo == null || p.EffectiveTo >= now) && p.TicketCategory == null)
                    .FirstOrDefaultAsync();
            }
            if (selected != null)
            {
                selected.RefundPolicyRules = selected.RefundPolicyRules
                    .OrderByDescending(r => r.CutoffMinutesBeforeStart)
                    .ThenBy(r => r.RuleOrder)
                    .ToList();
            }
            return selected;
        }
    }
}