# DroneVideoManager

A professional video management application designed specifically for drone videographers. DroneVideoManager helps you organize, analyze, and manage your drone footage efficiently.

![DroneVideoManager Screenshot](docs/images/screenshot.png)

## Features

- **Video Management**
  - Import and organize drone videos
  - Extract and display video metadata
  - Support for various video formats (MP4, MOV, AVI)
  - Automatic video file organization
  - Thumbnail generation
  - Video preview

- **Metadata Analysis**
  - Extract camera settings and flight data
  - Display telemetry information
  - View drone model and settings
  - Analyze flight paths and statistics
  - Export metadata to CSV/JSON
  - Timeline visualization

- **Project Organization**
  - Create and manage projects
  - Organize videos into folders
  - Add tags and categories
  - Search and filter videos
  - Custom metadata fields
  - Project templates

- **File System Integration**
  - Monitor folders for new videos
  - Automatic metadata extraction
  - File system event handling
  - Backup and restore capabilities
  - File integrity checks
  - Duplicate detection

## Prerequisites

- **Development Environment**
  - .NET 8.0 SDK
  - Visual Studio 2022 or Visual Studio Code
  - Git

- **Runtime Requirements**
  - Windows 10/11
  - SQL Server 2019 or later
  - FFMpeg (for video processing)
  - TagLibSharp (for metadata extraction)
  - 8GB RAM minimum
  - 500MB free disk space

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/DroneVideoManager.git
```

2. Navigate to the project directory:
```bash
cd DroneVideoManager
```

3. Restore NuGet packages:
```bash
dotnet restore
```

4. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DroneVideoManager;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

5. Run database migrations:
```bash
dotnet ef database update
```

6. Build and run the application:
```bash
dotnet build
dotnet run
```

## Project Structure

```
DroneVideoManager/
├── DroneVideoManager.Core/         # Core models and interfaces
│   ├── Models/                     # Domain models
│   └── Services/                   # Service interfaces
├── DroneVideoManager.Services/     # Service implementations
│   ├── Video/                      # Video processing services
│   ├── Metadata/                   # Metadata extraction services
│   └── FileSystem/                 # File system services
├── DroneVideoManager.Data/         # Data access layer
│   ├── Configurations/             # Entity configurations
│   └── Migrations/                 # Database migrations
└── DroneVideoManager.UI/           # WPF user interface
    ├── ViewModels/                 # View models
    ├── Views/                      # XAML views
    └── Converters/                 # Value converters
```

## Dependencies

- **Core Dependencies**
  - FFMpegCore (5.2.0) - Video processing
  - Microsoft.EntityFrameworkCore (8.0.2) - Database access
  - System.IO.Abstractions (22.0.13) - File system operations
  - TagLibSharp (2.3.0) - Metadata extraction

- **Development Dependencies**
  - xUnit - Unit testing
  - Moq - Mocking framework
  - FluentAssertions - Assertion library

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation
- Use meaningful commit messages

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to all contributors who have helped with this project
- Special thanks to the open-source community for their tools and libraries
- FFMpeg team for video processing capabilities
- TagLibSharp team for metadata extraction

## Support

For support, please:
1. Check the [documentation](docs/)
2. Search existing [issues](https://github.com/yourusername/DroneVideoManager/issues)
3. Create a new issue if needed

## Roadmap

### Short Term
- [ ] Add support for DJI Mavic 3
- [ ] Implement basic video editing
- [ ] Add cloud storage integration (OneDrive, Google Drive)

### Medium Term
- [ ] Create mobile companion app
- [ ] Add AI-powered video analysis
- [ ] Implement collaborative features

### Long Term
- [ ] Web-based version
- [ ] Advanced video editing suite
- [ ] Drone fleet management 