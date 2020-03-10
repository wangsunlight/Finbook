using Contact.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Threading;
using Contact.API.Models;

namespace Contact.API.Data
{
    public class MongoContactRepository : IContactRepository
    {

        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext contactContext)
        {
            this._contactContext = contactContext;
        }

        public async Task<bool> AddContactAsync(int userId, UserIdentity contact, CancellationToken cancellationToken)
        {
            if (_contactContext.ContactBooks.CountDocuments(c => c.UserId == userId) == 0)
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook
                {
                    UserId = userId
                }); ;
            }

            var filter = Builders<ContactBook>.Filter.Eq(c => c.UserId, userId);
            var update = Builders<ContactBook>.Update.AddToSet(c => c.Contacts, new Models.Contact
            {
                UserId = contact.UserId,
                Avatar = contact.Avatar,
                Company = contact.Company,
                Name = contact.Name,
                Title = contact.Title
            });

            var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        public async Task<List<Models.Contact>> GetContactAsync(int userId, CancellationToken cancellationToken)
        {
            var contacBook = (await _contactContext.ContactBooks.FindAsync(c => c.UserId == userId)).FirstOrDefault();
            if (contacBook != null)
            {
                return contacBook.Contacts;
            }

            return new List<Models.Contact>();
        }

        public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.Eq(c => c.UserId, userId),
                Builders<ContactBook>.Filter.Eq("Contacts.UserId", contactId)
                );

            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.UserId", contactId);

            var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
        }

        public async Task<bool> UpdateContactInfoAsync(UserIdentity userInfo, CancellationToken cancellationToken)
        {
            var contactBook = (await _contactContext.ContactBooks.FindAsync(c => c.UserId == userInfo.UserId, null, cancellationToken)).FirstOrDefault();
            if (contactBook == null)
            {
                //throw new Exception($"未查询到该ID的用户信息 ID={userInfo.UserId}");
                return true;
            }

            var contactIds = contactBook.Contacts.Select(c => c.UserId);

            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),
                Builders<ContactBook>.Filter.ElemMatch(c => c.Contacts, contact => contact.UserId == userInfo.UserId)
                );

            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.Name", userInfo.Name)
                .Set("Contacts.$.Avatar", userInfo.Avatar)
                .Set("Contacts.$.Company", userInfo.Company)
                .Set("Contacts.$.Title", userInfo.Title);

            var updateResult = _contactContext.ContactBooks.UpdateMany(filter, update);

            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }
    }
}
