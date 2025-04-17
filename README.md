# DroneVideoManager

A professional video management application designed specifically for drone videographers. DroneVideoManager helps you organize, analyze, and manage your drone footage efficiently.

## Features

- **Video Management**
  - Import and organize drone videos
  - Extract and display video metadata
  - Support for various video formats
  - Automatic video file organization

- **Metadata Analysis**
  - Extract camera settings and flight data
  - Display telemetry information
  - View drone model and settings
  - Analyze flight paths and statistics

- **Project Organization**
  - Create and manage projects
  - Organize videos into folders
  - Add tags and categories
  - Search and filter videos

- **File System Integration**
  - Monitor folders for new videos
  - Automatic metadata extraction
  - File system event handling
  - Backup and restore capabilities

## Prerequisites

- .NET 8.0 SDK
- SQL Server (for database)
- FFMpeg (for video processing)
- TagLibSharp (for metadata extraction)

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
    "DefaultConnection": "Your SQL Server connection string"
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

- **DroneVideoManager.Core**
  - Models and interfaces
  - Core business logic
  - Service contracts

- **DroneVideoManager.Services**
  - Service implementations
  - Business logic
  - External service integration

- **DroneVideoManager.Data**
  - Database context
  - Entity configurations
  - Data access layer

- **DroneVideoManager.UI**
  - User interface
  - WPF application
  - View models and views

## Dependencies

- FFMpegCore (5.2.0) - Video processing
- Microsoft.EntityFrameworkCore (8.0.2) - Database access
- System.IO.Abstractions (22.0.13) - File system operations
- TagLibSharp (2.3.0) - Metadata extraction

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Thanks to all contributors who have helped with this project
- Special thanks to the open-source community for their tools and libraries

## Support

For support, please open an issue in the GitHub repository or contact the maintainers.

## Roadmap

- [ ] Add support for more drone models
- [ ] Implement cloud storage integration
- [ ] Add video editing capabilities
- [ ] Create mobile companion app
- [ ] Add AI-powered video analysis 