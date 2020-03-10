using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Domain.AggregatesModel.Project>
    {
        private IProjectRepository _ProjectRepository;

        public CreateProjectCommandHandler(IProjectRepository ProjectRepository)
        {
            this._ProjectRepository = ProjectRepository;
        }
        public async Task<Domain.AggregatesModel.Project> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            _ProjectRepository.Add(request.Project);
            await _ProjectRepository.UnitOfWork.SaveEntitiesAsync();

            return request.Project;
        }
    }
}
