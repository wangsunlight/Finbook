using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.IntegrationEvent
{
    public class ProjectJoinedIntegrationEvent
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public ProjectContributor Contributor { get; set; }
}
}
