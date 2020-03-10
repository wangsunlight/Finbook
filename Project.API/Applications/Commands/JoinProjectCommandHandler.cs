using MediatR;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommandHandler : IRequestHandler<JoinProjectCommand>
    {
        private IProjectRepository _ProjectRepository;

        public JoinProjectCommandHandler(IProjectRepository ProjectRepository)
        {
            this._ProjectRepository = ProjectRepository;
        }

        public async Task<Unit> Handle(JoinProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _ProjectRepository.GetAsync(request.Contributor.ProjectId);

            if (project == null)
            {
                throw new Domain.Exceptions.ProjectDomainException($"project not fount ID ={request.Contributor.ProjectId}");
            }

            if (project.UserId == request.Contributor.UserId)
            {
                throw new Domain.Exceptions.ProjectDomainException($"你不能加入你自己的项目");
            }

            project.AddContributor(request.Contributor);
            await _ProjectRepository.UnitOfWork.SaveEntitiesAsync();

            return Unit.Value;
        }
    }
}
