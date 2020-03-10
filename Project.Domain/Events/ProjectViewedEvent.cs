using MediatR;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Domain.Events
{
    public class ProjectViewedEvent : INotification
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public ProjectViewer Viewer { get; set; }
    }
}
