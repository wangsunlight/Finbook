using Contact.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Threading;

namespace Contact.API.Data
{
    public class MongoContactAppllyRequestRepository : IContactApplyRequestRespository
    {
        private readonly ContactContext _contactContext;

        public MongoContactAppllyRequestRepository(ContactContext contactContext)
        {
            this._contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == request.UserId && r.ApplierId == request.ApplierId);

            if ((await _contactContext.ContactApplyRequest.CountDocumentsAsync(filter)) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update
                    .Set(r => r.ApplyTime, DateTime.Now);

                //var option = new UpdateOptions { IsUpsert = true };
                var result = await _contactContext.ContactApplyRequest.UpdateOneAsync(filter, update, null, cancellationToken);

                return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
            }

            await _contactContext.ContactApplyRequest.InsertOneAsync(request, null, cancellationToken);
            return true;

        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == userId && r.ApplierId == applierId);


            var update = Builders<ContactApplyRequest>.Update
                .Set(r => r.Approvaled, 1)
                .Set(r => r.HandledTime, DateTime.Now);

            //var option = new UpdateOptions { IsUpsert = true };
            var result = await _contactContext.ContactApplyRequest.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
        }

        public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken)
        {
            return (await _contactContext.ContactApplyRequest.FindAsync(c => c.UserId == userId)).ToList(cancellationToken);

        }
    }
}
