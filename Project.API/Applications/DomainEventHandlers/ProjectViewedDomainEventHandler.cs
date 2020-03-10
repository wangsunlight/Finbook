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
    public class ProjectViewedDomainEventHandler : INotificationHandler<ProjectViewedEvent>
    {
        private ICapPublisher _capPublisher;

        public ProjectViewedDomainEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public Task Handle(ProjectViewedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectViewedIntegrationEvent
            {
                Company = notification.Company,
                Viewer = notification.Viewer,
                Introduction = notification.Introduction
            };

            _capPublisher.Publish("finbook.projectapi.projectviewed", @event);

            return Task.CompletedTask;
        }
    }
}
