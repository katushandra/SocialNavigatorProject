using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
    public static class UpdateObjectScore
    {
        public static async Task<decimal?> UpdateScore(
            ILocalDbContext context,
            Guid objectId,
            CancellationToken cancellationToken = default)
        {
            var averageScore = await context.Review
                .Where(x => x.ObjectId == objectId)
                .AverageAsync(x => (decimal?)x.Score, cancellationToken) ?? 0;

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == objectId, cancellationToken);

            if (socialObject != null)
            {
                socialObject.ScoreObject = Math.Round(averageScore, 1);
                await context.SaveChangesAsync(cancellationToken);
                return socialObject.ScoreObject;
            }

            return null;
        }
    }
}
