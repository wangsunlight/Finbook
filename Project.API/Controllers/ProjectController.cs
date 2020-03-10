using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.API.Applications.Commands;
using MediatR;
using Project.Domain.AggregatesModel;
using Project.API.Applications.Service;
using Project.API.Applications.Queries;

namespace Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private IMediator _mediator;
        private IRecommendService _recommendService;
        private IProjectQueries _projectQueries;

        public ProjectController(IMediator mediator, IRecommendService recommendService, IProjectQueries projectQueries)
        {
            this._mediator = mediator;
            this._recommendService = recommendService;
            this._projectQueries = projectQueries;
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetProject()
        {
            var project = await _projectQueries.GetProjectByUserId(UserIdentity.UserId);

            return Ok(project);
        }

        [HttpGet]
        [Route("getmydetail/{projectId}")]
        public async Task<IActionResult> GetMyProjectDetail(int projectId)
        {
            var project = await _projectQueries.GetProjectDetail(projectId);

            if (project.UserId == UserIdentity.UserId)
            {
                return Ok(project);
            }

            return BadRequest("无权查看该项目");
        }

        [HttpGet]
        [Route("recommends/{projectId}")]
        public async Task<IActionResult> GetRecommendProjectDerail(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                var project = await _projectQueries.GetProjectDetail(projectId);
                return Ok(project);
            }

            return BadRequest("无权查看该项目");
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProject([FromBody]Domain.AggregatesModel.Project project)
        {
            if (project == null)
            {
                throw new ArgumentException(nameof(project));
            }
            project.UserId = UserIdentity.UserId;

            var command = new CreateProjectCommand() { Project = project };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPut]
        [Route("view/{projectId}")]
        public async Task<IActionResult> ViewProject(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }

            var command = new ViewProjectCommand()
            {
                UserId = UserIdentity.UserId,
                UserName = UserIdentity.Name,
                Avatar = UserIdentity.Avatar,
                ProjectId = projectId
            };

            await _mediator.Send(command);
            return Ok();
        }

        [HttpPut]
        [Route("join/{projectId}")]
        public async Task<IActionResult> JoinProject(int projectId, [FromBody]ProjectContributor contributor)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }

            var command = new JoinProjectCommand() { Contributor = contributor };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}