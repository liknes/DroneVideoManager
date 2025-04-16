using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(DroneVideoDbContext context)
        {
            // Ensure database is created and migrated
            context.Database.Migrate();

            // Only seed if database is empty
            if (context.VideoFiles.Any() || context.Folders.Any() || context.Projects.Any())
                return;

            // Seed Tags
            var tags = new[]
            {
                new Tag { Name = "Landscape", Description = "Scenic landscape shots", CreatedDate = DateTime.Now },
                new Tag { Name = "Urban", Description = "City and urban area footage", CreatedDate = DateTime.Now },
                new Tag { Name = "Sunset", Description = "Footage during sunset/golden hour", CreatedDate = DateTime.Now },
                new Tag { Name = "Aerial", Description = "High altitude aerial shots", CreatedDate = DateTime.Now },
                new Tag { Name = "Follow", Description = "Follow shots of subjects", CreatedDate = DateTime.Now }
            };
            context.Tags.AddRange(tags);

            // Seed Projects
            var projects = new[]
            {
                new Project 
                { 
                    Name = "City Documentary",
                    Description = "Documentary about urban development",
                    ClientName = "City Council",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Project 
                { 
                    Name = "Nature Series",
                    Description = "Series showcasing local nature",
                    ClientName = "Nature Channel",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            };
            context.Projects.AddRange(projects);

            // Seed Folders
            var rootFolder = new Folder
            {
                Name = "Drone Footage",
                Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Drone Footage"),
                CreatedDate = DateTime.Now,
                LastScannedDate = DateTime.Now,
                IsWatched = true
            };

            var subFolders = new[]
            {
                new Folder 
                { 
                    Name = "City Shots",
                    Path = Path.Combine(rootFolder.Path, "City Shots"),
                    CreatedDate = DateTime.Now,
                    LastScannedDate = DateTime.Now,
                    IsWatched = true,
                    ParentFolder = rootFolder
                },
                new Folder 
                { 
                    Name = "Nature Shots",
                    Path = Path.Combine(rootFolder.Path, "Nature Shots"),
                    CreatedDate = DateTime.Now,
                    LastScannedDate = DateTime.Now,
                    IsWatched = true,
                    ParentFolder = rootFolder
                }
            };

            context.Folders.Add(rootFolder);
            context.Folders.AddRange(subFolders);

            // Save changes to generate IDs
            context.SaveChanges();

            // Create sample video files
            var videos = new[]
            {
                new VideoFile
                {
                    FileName = "DJI_0001.MP4",
                    FilePath = Path.Combine(subFolders[0].Path, "DJI_0001.MP4"),
                    FileSize = 256.5, // MB
                    Duration = TimeSpan.FromMinutes(5),
                    CreatedDate = DateTime.Now.AddDays(-5),
                    ImportedDate = DateTime.Now,
                    Description = "City skyline at sunset",
                    Folder = subFolders[0],
                    Width = 3840,
                    Height = 2160,
                    FramesPerSecond = 30,
                    ColorSpace = "H.264",
                    FileHash = "sample_hash_1",
                    DroneMetadata = new DroneMetadata
                    {
                        DroneModel = "DJI Mavic 3",
                        FlightDate = DateTime.Now.AddDays(-5),
                        HomeLatitude = 59.9139,
                        HomeLongitude = 10.7522,
                        HomeAltitude = 100
                    }
                },
                new VideoFile
                {
                    FileName = "DJI_0002.MP4",
                    FilePath = Path.Combine(subFolders[1].Path, "DJI_0002.MP4"),
                    FileSize = 512.3, // MB
                    Duration = TimeSpan.FromMinutes(8),
                    CreatedDate = DateTime.Now.AddDays(-3),
                    ImportedDate = DateTime.Now,
                    Description = "Forest flythrough",
                    Folder = subFolders[1],
                    Width = 3840,
                    Height = 2160,
                    FramesPerSecond = 60,
                    ColorSpace = "H.264",
                    FileHash = "sample_hash_2",
                    DroneMetadata = new DroneMetadata
                    {
                        DroneModel = "DJI Mavic 3",
                        FlightDate = DateTime.Now.AddDays(-3),
                        HomeLatitude = 59.9139,
                        HomeLongitude = 10.7522,
                        HomeAltitude = 50
                    }
                }
            };

            context.VideoFiles.AddRange(videos);

            // Add some video-project relationships
            context.VideoProjects.AddRange(new[]
            {
                new VideoProject
                {
                    VideoFile = videos[0],
                    Project = projects[0],
                    DateAdded = DateTime.Now,
                    Notes = "Perfect for opening sequence",
                    IsUsedInFinalCut = true
                },
                new VideoProject
                {
                    VideoFile = videos[1],
                    Project = projects[1],
                    DateAdded = DateTime.Now,
                    Notes = "Good establishing shot",
                    IsUsedInFinalCut = false
                }
            });

            // Add some video tags
            context.VideoTags.AddRange(new[]
            {
                new VideoTag
                {
                    VideoFile = videos[0],
                    Tag = tags[0], // Landscape
                    DateTagged = DateTime.Now
                },
                new VideoTag
                {
                    VideoFile = videos[0],
                    Tag = tags[2], // Sunset
                    DateTagged = DateTime.Now
                },
                new VideoTag
                {
                    VideoFile = videos[1],
                    Tag = tags[0], // Landscape
                    DateTagged = DateTime.Now
                }
            });

            // Save all changes
            context.SaveChanges();
        }
    }
} 