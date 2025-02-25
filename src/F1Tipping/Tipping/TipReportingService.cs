using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Tipping
{
    public class TipReportingService
    {
        private readonly ModelDbContext _modelDb;

        public TipReportingService(ModelDbContext modelDb)
        {
            _modelDb = modelDb;
        }

        public IEnumerable<Tip> GetTips(Player player, IEventWithResults @event)
        {
            return _modelDb.Tips
                .Where(t => t.Target.Event == @event && t.Tipper == player)
                .GroupBy(t => t.Target.Type)
                .Select(tg => tg.OrderByDescending(t => t.SubmittedAt).First());

            /* TODO: Debug this, resolve multiple-calls-to-db-context problem
            return await _modelDb.Tips
                .Where(t => t.Target.Event == @event && t.Tipper == player)
                .GroupBy(t => t.Target.Type)
                .Select(tg => tg.OrderByDescending(t => t.SubmittedAt).First())
                .ToListAsync();
            */
        }
    }
}