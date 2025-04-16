﻿// <auto-generated />
using System;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DroneVideoManager.Data.Migrations
{
    [DbContext(typeof(DroneVideoDbContext))]
    [Migration("20250416140546_AddVideoMetadata")]
    partial class AddVideoMetadata
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("DroneVideoManager.Core.Models.DroneMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DroneModel")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("FlightDate")
                        .HasColumnType("TEXT");

                    b.Property<double>("HomeAltitude")
                        .HasColumnType("REAL");

                    b.Property<double>("HomeLatitude")
                        .HasColumnType("REAL");

                    b.Property<double>("HomeLongitude")
                        .HasColumnType("REAL");

                    b.Property<int>("VideoFileId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("VideoFileId")
                        .IsUnique();

                    b.ToTable("DroneMetadata");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsWatched")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastScannedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentFolderId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ParentFolderId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClientName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.TelemetryPoint", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Altitude")
                        .HasColumnType("REAL");

                    b.Property<int>("DroneMetadataId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Heading")
                        .HasColumnType("REAL");

                    b.Property<double>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<double>("Speed")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DroneMetadataId");

                    b.ToTable("TelemetryPoints");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Altitude")
                        .HasColumnType("REAL");

                    b.Property<string>("ColorSpace")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("FileSize")
                        .HasColumnType("REAL");

                    b.Property<int?>("FolderId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("FramesPerSecond")
                        .HasColumnType("REAL");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ImportedDate")
                        .HasColumnType("TEXT");

                    b.Property<double?>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double?>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("VideoFiles");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AudioBitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AudioChannels")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AudioCodec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("AudioSampleRate")
                        .HasColumnType("INTEGER");

                    b.Property<long>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CameraModel")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CameraSettings")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ColorDepth")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ColorSpace")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExactCreationTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsVariableFrameRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RecordingMode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoCodec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("VideoFileId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("VideoFileId")
                        .IsUnique();

                    b.ToTable("VideoMetadata");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoProject", b =>
                {
                    b.Property<int>("VideoFileId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsUsedInFinalCut")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("VideoFileId", "ProjectId");

                    b.HasIndex("ProjectId");

                    b.ToTable("VideoProjects");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoTag", b =>
                {
                    b.Property<int>("VideoFileId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateTagged")
                        .HasColumnType("TEXT");

                    b.HasKey("VideoFileId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("VideoTags");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.DroneMetadata", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.VideoFile", "VideoFile")
                        .WithOne("DroneMetadata")
                        .HasForeignKey("DroneVideoManager.Core.Models.DroneMetadata", "VideoFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VideoFile");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Folder", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.Folder", "ParentFolder")
                        .WithMany("SubFolders")
                        .HasForeignKey("ParentFolderId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentFolder");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.TelemetryPoint", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.DroneMetadata", "DroneMetadata")
                        .WithMany("TelemetryPoints")
                        .HasForeignKey("DroneMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DroneMetadata");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoFile", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.Folder", "Folder")
                        .WithMany("Videos")
                        .HasForeignKey("FolderId");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoMetadata", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.VideoFile", "VideoFile")
                        .WithOne("Metadata")
                        .HasForeignKey("DroneVideoManager.Core.Models.VideoMetadata", "VideoFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VideoFile");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoProject", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.Project", "Project")
                        .WithMany("Videos")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DroneVideoManager.Core.Models.VideoFile", "VideoFile")
                        .WithMany("Projects")
                        .HasForeignKey("VideoFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("VideoFile");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoTag", b =>
                {
                    b.HasOne("DroneVideoManager.Core.Models.Tag", "Tag")
                        .WithMany("Videos")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DroneVideoManager.Core.Models.VideoFile", "VideoFile")
                        .WithMany("Tags")
                        .HasForeignKey("VideoFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tag");

                    b.Navigation("VideoFile");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.DroneMetadata", b =>
                {
                    b.Navigation("TelemetryPoints");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Folder", b =>
                {
                    b.Navigation("SubFolders");

                    b.Navigation("Videos");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Project", b =>
                {
                    b.Navigation("Videos");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.Tag", b =>
                {
                    b.Navigation("Videos");
                });

            modelBuilder.Entity("DroneVideoManager.Core.Models.VideoFile", b =>
                {
                    b.Navigation("DroneMetadata");

                    b.Navigation("Metadata");

                    b.Navigation("Projects");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
