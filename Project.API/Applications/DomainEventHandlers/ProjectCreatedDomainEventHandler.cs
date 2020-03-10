using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.Domain.Events;
using Project.API.Applications.IntegrationEvent;

namespace Project.API.Applications.DomainEventHandlers
{
    public class ProjectCreatedDomainEventHandler : INotificationHandler<ProjectCreatedEvent>
    {
        private ICapPublisher _capPublisher;

        public ProjectCreatedDomainEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectCreatedIntegrationEvent
            {
                UserId = notification.Project.UserId,
                ProjectId = notification.Project.Id,
                CreatedTime = DateTime.Now
            };

            _capPublisher.Publish("finbook.projectapi.projectcreated", @event);

            return Task.CompletedTask;
        }
    }
}
