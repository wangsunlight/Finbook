using MediatR;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class ViewProjectCommandHandler : IRequestHandler<ViewProjectCommand>
    {
        private IProjectRepository _ProjectRepository;

        public ViewProjectCommandHandler(IProjectRepository ProjectRepository)
        {
            this._ProjectRepository = ProjectRepository;
        }

        public async Task<Unit> Handle(ViewProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _ProjectRepository.GetAsync(request.ProjectId);

            if (project == null)
            {
                throw new Domain.Exceptions.ProjectDomainException($"project not fount ID ={request.ProjectId}");
            }

            if (project.UserId == request.UserId)
            {
                throw new Domain.Exceptions.ProjectDomainException($"你不能加入你自己的项目");
            }

            project.AddViewer(request.UserId, request.UserName, request.Avatar);
            await _ProjectRepository.UnitOfWork.SaveEntitiesAsync();

            return Unit.Value;
        }

    }
}
