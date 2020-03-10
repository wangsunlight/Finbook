using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<ProjectProperty>
    {
        public void Configure(EntityTypeBuilder<ProjectProperty> builder)
        {
            builder.ToTable("ProjectProperties")
                .Property(p => p.Key).HasMaxLength(100);

            builder.ToTable("ProjectProperties")
                .HasKey(p => new
                {
                    p.ProjectId,
                    p.Key,
                    p.Value
                });
        }
    }
}
