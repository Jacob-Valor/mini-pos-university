# Mini POS System

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3.9-purple)](https://avaloniaui.net/)
[![MariaDB](https://img.shields.io/badge/MariaDB-10.11-003545)](https://mariadb.org/)
[![Docker](https://img.shields.io/badge/Docker-Supported-2496ED)](https://www.docker.com/)

A modern Point of Sale (POS) system built with .NET 10 and Avalonia UI, featuring a MariaDB database backend. The application is designed for desktop deployment with Docker support for the database layer.

## Features

- 🛒 **Product Management** - Manage inventory, categories, and brands
- 👥 **Employee Management** - Track staff information and credentials
- 🏪 **Customer Database** - Maintain customer records and transaction history  
- 💳 **Sales Processing** - Complete POS functionality for retail operations
- 🎨 **Modern UI** - Built with Avalonia UI for cross-platform desktop support
- 🐳 **Docker Support** - Containerized database deployment
- 🌏 **Localization** - Support for Lao language (lo-LA)

## Tech Stack

- **Frontend**: Avalonia UI 11.3.9 (Cross-platform .NET XAML framework)
- **Backend**: .NET 10.0
- **Database**: Maria DB 10.11
- **ORM**: ADO.NET with MySqlConnector
- **MVVM**: CommunityToolkit.Mvvm
- **Containerization**: Docker & Docker Compose

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) & [Docker Compose](https://docs.docker.com/compose/install/) (for database)
- Linux/macOS/Windows with GUI support

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-repo/mini_pos.git
cd mini_pos
```

### 2. Setup Environment Variables

```bash
cp .env.example .env
# Edit .env with your configuration
```

### 3. Start the Database

```bash
# Start MariaDB with Docker Compose
docker-compose up mariadb -d

# Check database logs
docker-compose logs -f mariadb
```

### 4. Run the Application

```bash
# Restore dependencies and run
dotnet restore
dotnet run
```

Alternatively, use your IDE (Visual Studio, Rider, VS Code) to build and run the project.

## Docker Deployment

### Database Only (Recommended for Development)

```bash
# Start only the database service
docker-compose up mariadb -d
```

### Full Stack (Requires X11 Forwarding)

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

**Note**: Running GUI applications in Docker requires additional setup. See [Docker GUI Configuration](#docker-gui-configuration) below.

## Project Structure

```
mini_pos/
├── Assets/                 # Application resources (fonts, icons)
├── Converters/            # Value converters for data binding
├── db/                    # Database schemas and initialization
│   └── workshop.sql       # Database schema and sample data
├── Models/                # Data models (Employee, Product, etc.)
├── ViewModels/            # MVVM view models
├── Views/                 # AXAML UI views
├── App.axaml             # Application entry point
├── Program.cs            # Application initialization
├── Dockerfile            # Container build configuration
├── docker-compose.yml    # Multi-container orchestration
├── appsettings.json      # Application configuration
└── mini_pos.csproj       # Project file
```

## Configuration

### Database Connection

Edit `appsettings.json` or set environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=mini_pos;User=root;Password=your_password;"
  }
}
```

### Environment Variables

See `.env.example` for all available configuration options:

- `DB_HOST` - Database server hostname
- `DB_PORT` - Database port (default: 3306)
- `DB_NAME` - Database name
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

## Database Management

### Initialize Database

The database is automatically initialized with the schema from `db/workshop.sql` when first started.

### Reset Database

```bash
# WARNING: This will delete all data!
docker-compose down -v
docker-compose up mariadb -d
```

### Access Database

```bash
# Using docker exec
docker exec -it mini_pos_db mysql -u root -p mini_pos

# Or use your favorite MySQL client
# Host: localhost, Port: 3306, Database: mini_pos
```

## Docker GUI Configuration

Running the Avalonia UI application in Docker requires X11 forwarding:

### Linux (X11)

```bash
# Allow local connections
xhost +local:docker

# Uncomment X11 volumes in docker-compose.yml
# volumes:
#   - /tmp/.X11-unix:/tmp/.X11-unix
# environment:
#   - DISPLAY=$DISPLAY
```

### Alternative: Local Development

For the best development experience, run the database in Docker and the application locally:

```bash
# Start only the database
docker-compose up mariadb -d

# Run the application locally
dotnet run
```

## Development

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### Running

```bash
# Development mode
dotnet run

# Watch mode (auto-reload)
dotnet watch run
```

### Adding NuGet Packages

```bash
dotnet add package PackageName
```

## Troubleshooting

### Application won't start on Linux

**Issue**: "No graphical session detected"

**Solution**: Ensure `DISPLAY` environment variable is set:
```bash
export DISPLAY=:0
dotnet run
```

Alternatively, use `xvfb` for headless environments:
```bash
xvfb-run -a dotnet run
```

### Database connection failed

**Issue**: Cannot connect to MariaDB

**Solutions**:
1. Ensure database container is running: `docker ps`
2. Check database logs: `docker-compose logs mariadb`
3. Verify connection string in `appsettings.json`
4. Ensure database has been initialized (check `db/workshop.sql` was loaded)

### Docker build fails

**Issue**: Build errors during `docker build`

**Solutions**:
1. Clear Docker cache: `docker system prune -a`
2. Rebuild without cache: `docker-compose build --no-cache`
3. Check Docker has enough disk space

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/)
- Database: [MariaDB](https://mariadb.org/)
- Icons and fonts: Various open-source resources

## Support

For issues, questions, or contributions, please open an issue on the GitHub repository.

---

**Made with ❤️ using .NET and Avalonia UI**
