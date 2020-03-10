using Microsoft.EntityFrameworkCore;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectContext _context;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public ProjectRepository(ProjectContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectEntity> GetAsync(int id)
        {
            var project = await _context
                               .Projects
                               .Include(x => x.Properties)
                               .Include(x => x.Viewers)
                               .Include(x => x.Contributors)
                               .Include(x => x.VisibleRule)
                               .SingleOrDefaultAsync(o => o.Id == id);

            return project;
        }

        public void Add(ProjectEntity project)
        {
            if (project.IsTransient())
            {
                _context.Add(project);
            }
        }

        public void Update(ProjectEntity project)
        {
            _context.Update(project);
        }
    }
}
