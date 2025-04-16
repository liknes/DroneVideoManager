using Microsoft.EntityFrameworkCore;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Data
{
    public class DroneVideoDbContext : DbContext
    {
        public DbSet<VideoFile> VideoFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<VideoProject> VideoProjects { get; set; }
        public DbSet<DroneMetadata> DroneMetadata { get; set; }
        public DbSet<TelemetryPoint> TelemetryPoints { get; set; }
        public DbSet<VideoTag> VideoTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DroneVideoDbContext(DbContextOptions<DroneVideoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure VideoProject (many-to-many relationship)
            modelBuilder.Entity<VideoProject>()
                .HasKey(vp => new { vp.VideoFileId, vp.ProjectId });

            modelBuilder.Entity<VideoProject>()
                .HasOne(vp => vp.VideoFile)
                .WithMany(v => v.Projects)
                .HasForeignKey(vp => vp.VideoFileId);

            modelBuilder.Entity<VideoProject>()
                .HasOne(vp => vp.Project)
                .WithMany(p => p.Videos)
                .HasForeignKey(vp => vp.ProjectId);

            // Configure VideoTag (many-to-many relationship)
            modelBuilder.Entity<VideoTag>()
                .HasKey(vt => new { vt.VideoFileId, vt.TagId });

            // Configure Folder hierarchy
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure DroneMetadata
            modelBuilder.Entity<DroneMetadata>()
                .HasOne(d => d.VideoFile)
                .WithOne(v => v.DroneMetadata)
                .HasForeignKey<DroneMetadata>(d => d.VideoFileId);

            // Configure TelemetryPoint
            modelBuilder.Entity<TelemetryPoint>()
                .HasOne(t => t.DroneMetadata)
                .WithMany(d => d.TelemetryPoints)
                .HasForeignKey(t => t.DroneMetadataId);

            base.OnModelCreating(modelBuilder);
        }
    }
} 