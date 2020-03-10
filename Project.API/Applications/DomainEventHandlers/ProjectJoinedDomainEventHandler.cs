using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvent;
using Project.Domain.Events;

namespace Project.API.Applications.DomainEventHandlers
{
    public class ProjectJoinedDomainEventHandler : INotificationHandler<ProjectJoinedEvent>
    {
        private ICapPublisher _capPublisher;

        public ProjectJoinedDomainEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public Task Handle(ProjectJoinedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectJoinedIntegrationEvent
            {
                Company = notification.Company,
                Contributor = notification.Contributor,
                Introduction = notification.Introduction
            };

            _capPublisher.Publish("finbook.projectapi.projectjoined", @event);

            return Task.CompletedTask;
        }
    }
}
