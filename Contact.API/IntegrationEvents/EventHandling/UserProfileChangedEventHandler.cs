using Contact.API.Data;
using Contact.API.Dtos;
using Contact.API.IntegrationEvents.Events;
using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Contact.API.IntegrationEvents.EventHandling
{
    public class UserProfileChangedEventHandler : ICapSubscribe
    {
        private IContactRepository _contactRepository;
        public UserProfileChangedEventHandler(IContactRepository contactRepository)
        {
            this._contactRepository = contactRepository;
        }

        [CapSubscribe("finbook.userapi.userprofilechanged")]
        public async Task UpdateContactInfo(UserProfileChangedEvent @event)
        {
            var token = new CancellationToken();
            await _contactRepository.UpdateContactInfoAsync(new UserIdentity
            {
                Avatar = @event.Avatar,
                Company = @event.Company,
                Name = @event.Name,
                Title = @event.Title,
                UserId = @event.UserId
            }, token);
        }
    }
}
