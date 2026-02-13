# Dev Version

# 🏪 Mini POS System

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3.9-purple)](https://avaloniaui.net/)
[![MariaDB](https://img.shields.io/badge/MariaDB-10.11-003545)](https://mariadb.org/)
[![Docker](https://img.shields.io/badge/Docker-Supported-2496ED)](https://www.docker.com/)

A modern Point of Sale (POS) system built with .NET 10 and Avalonia UI, featuring a MariaDB database backend. The application is designed for desktop deployment with Docker support for the database layer.

## ✨ Features

- 🛒 **Product Management** - Manage inventory, categories, and brands
- 👥 **Employee Management** - Track staff information and credentials
- 🏪 **Customer Database** - Maintain customer records and transaction history
- 💳 **Sales Processing** - Complete POS functionality for retail operations
- 🎨 **Modern UI** - Built with Avalonia UI for cross-platform desktop support
- 🐳 **Docker Support** - Containerized database deployment
- 🌏 **Localization** - Support for Lao language (lo-LA)
- 🚀 **High-Performance MVVM** - Source-generated ViewModels with CommunityToolkit.Mvvm 8.4
- 🧭 **Navigation Service** - Factory pattern for proper DI ViewModel creation

## 🛠️ Tech Stack

- 🖥️ **Frontend**: Avalonia UI 11.3.9 (Cross-platform .NET XAML framework)
- 🔙 **Backend**: .NET 10.0
- 🗄️ **Database**: Maria DB 10.11
- 🔌 **ORM**: ADO.NET with MySqlConnector
- 🏗️ **MVVM**: CommunityToolkit.Mvvm 8.4 (Source-generated)
- 📦 **Containerization**: Docker & Docker Compose
- 💉 **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## 📋 Prerequisites

- 📦 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- 🐳 [Docker](https://www.docker.com/get-started) & [Docker Compose](https://docs.docker.com/compose/install/) (for database)
- 🖥️ **Cross-platform**: Supports Linux, macOS (Intel/Apple Silicon), and Windows

## ✅ Quick Demo (Grade-Max)

### 1) Start Database (Docker)

```bash
cp .env.example .env
docker-compose up -d mariadb
```

### 2) Run App

```bash
dotnet restore
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

Optional DB smoke test (no UI):

```bash
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj -- --test-db
```

Run tests:

```bash
dotnet test
```

Note: `dotnet test` runs integration tests that start a MariaDB container via Testcontainers, so Docker must be running.

Unit tests only (skip integration tests):

```bash
dotnet test --filter "FullyQualifiedName!~mini_pos.Tests.Integration"
```

### 3) Login Accounts (Seed Data)

These accounts are included in `db/workshop.sql`:

- Admin: `admin` / `1234`
- Employee: `phut` / `1234`

### 4) Suggested Demo Flow

1. Login
2. Go to Sales
3. Scan/enter a barcode from seed data (examples): `002`, `003`, `006`
4. Add to cart, enter money received, save sale
5. Generate a sales report PDF

## 🏛️ Architecture Overview

### MVVM with CommunityToolkit.Mvvm

This project uses **CommunityToolkit.Mvvm** for high-performance MVVM implementation:

- **Source Generators** - Compile-time code generation for properties and commands
- **ObservableProperty** - `[ObservableProperty]` attributes replace manual property change notifications
- **RelayCommand** - `[RelayCommand]` attributes auto-generate async/sync commands with CanExecute support
- **40-50% less boilerplate** compared to ReactiveUI

**Example:**
```csharp
// Before (ReactiveUI) - 15 lines
private string _name = string.Empty;
public string Name
{
    get => _name;
    set => this.RaiseAndSetIfChanged(ref _name, value);
}
public ReactiveCommand<Unit, Unit> SaveCommand { get; }

// After (CommunityToolkit.Mvvm) - 4 lines
[ObservableProperty]
private string _name = string.Empty;

[RelayCommand]
private async Task SaveAsync() { ... }
```

### Dependency Injection

All services and ViewModels are registered in `App.axaml.cs` using Microsoft DI:

- `IMySqlConnectionFactory` - Opens MySQL/MariaDB connections
- `IBrandRepository`, `IProductRepository`, `ICustomerRepository`, ... - Data access repositories
- `IDialogService` - User notifications and confirmations  
- `IReportService` - PDF report generation
- `INavigationService` - Factory pattern for ViewModel creation

### 💻 macOS (Recommended: JetBrains Rider)

**Requirements:**

- [Rider](https://www.jetbrains.com/rider/) (recommended) or [Visual Studio Code](https://code.visualstudio.com/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)

**Setup:**

```bash
# Install .NET SDK via Homebrew
brew install --cask dotnet-sdk

# Or download from: https://dotnet.microsoft.com/download/dotnet/10.0

# Start Docker Desktop
open -a Docker

# Start MariaDB container
docker run -d --name mini_pos_db \
  -p 3306:3306 \
  -e MYSQL_ROOT_PASSWORD=root_password \
  -e MYSQL_DATABASE=mini_pos \
  mariadb:10.11

# Run the application
dotnet restore
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

**Using Rider:**

1. Open the `mini_pos.sln` file in Rider
2. Configure Docker connection in Rider's Services tool window
3. Run/debug directly from the IDE with full Avalonia UI designer support

### 🐧 Linux

```bash
# Install .NET SDK
sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0

# Start Docker service
sudo systemctl start docker

# Start MariaDB with Docker Compose
docker-compose up mariadb -d

# Run the application
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

### 🪟 Windows

```bash
# Install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0

# Start Docker Desktop

# Start MariaDB container
docker run -d --name mini_pos_db -p 3306:3306 -e MYSQL_ROOT_PASSWORD=root_password -e MYSQL_DATABASE=mini_pos mariadb:10.11

# Run the application
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

Alternatively, use Visual Studio 2022 with the Avalonia extension.

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/your-repo/mini_pos.git
cd mini_pos
```

### 2️⃣ Setup Environment Variables

```bash
cp .env.example .env
# Edit .env with your configuration
```

### 3️⃣ Start the Database

```bash
# Start MariaDB with Docker Compose
docker-compose up mariadb -d

# Check database logs
docker-compose logs -f mariadb
```

### 4️⃣ Run the Application

```bash
# Restore dependencies and run
dotnet restore
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

Alternatively, use your IDE (Visual Studio, Rider, VS Code) to build and run the project.

## 🐳 Docker Deployment

### 🗄️ Database Only (Recommended for Development)

```bash
# Start only the database service
docker-compose up mariadb -d
```

### 🚀 Full Stack (Linux/X11 Only)

**Note**: Running GUI applications in Docker is only supported on Linux with X11 forwarding. For macOS and Windows, run the application natively and use Docker only for the database.

```bash
# Build and start all services (Linux only)
docker-compose up -d

# View logs
docker-compose logs -f
```

See [Docker GUI Configuration](#docker-gui-configuration) for Linux X11 setup.

## 📂 Project Structure

```
mini_pos/
├── db/                    # Database schemas and initialization
│   └── workshop.sql       # Database schema and sample data
├── src/
│   ├── mini_pos.Desktop/  # Avalonia desktop app project
│   └── mini_pos.Api/      # ASP.NET Core API project
├── tests/
│   ├── mini_pos.Tests/    # Desktop app tests
│   └── mini_pos.Api.Tests/# API tests
├── Dockerfile            # Container build configuration
├── docker-compose.yml    # Multi-container orchestration
└── mini_pos.sln          # Solution file
```

## ⚙️ Configuration

### 🔌 Database Connection

Edit `src/mini_pos.Desktop/appsettings.json` or set environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=mini_pos;User=root;Password=your_password;"
  }
}
```

### 📝 Environment Variables

See `.env.example` for all available configuration options:

- `DB_HOST` - Database server hostname
- `DB_PORT` - Database port (default: 3306)
- `DB_NAME` - Database name
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

## 🗃️ Database Management

### 🆕 Initialize Database

The database is automatically initialized with the schema from `db/workshop.sql` when first started.

### 🔄 Reset Database

```bash
# WARNING: This will delete all data!
docker-compose down -v
docker-compose up mariadb -d
```

### 🔍 Access Database

```bash
# Using docker exec
docker exec -it mini_pos_db mysql -u root -p mini_pos

# Or use your favorite MySQL client
# Host: localhost, Port: 3306, Database: mini_pos
```

## 🖥️ Docker GUI Configuration

Running the Avalonia UI application in Docker requires X11 forwarding:

### 🐧 Linux (X11)

```bash
# Allow local connections
xhost +local:docker

# Uncomment X11 volumes in docker-compose.yml
# volumes:
#   - /tmp/.X11-unix:/tmp/.X11-unix
# environment:
#   - DISPLAY=$DISPLAY
```

### 💻 Alternative: Local Development

For the best development experience, run the database in Docker and the application locally:

```bash
# Start only the database
docker-compose up mariadb -d

# Run the application locally
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

## 💻 Development

### 🔨 Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### ▶️ Running

```bash
# Development mode
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj

# Watch mode (auto-reload)
dotnet watch --project src/mini_pos.Desktop/mini_pos.csproj run
```

### 📦 Adding NuGet Packages

```bash
dotnet add package PackageName
```

## 🔧 Troubleshooting

### ❌ Application won't start

**On Linux:**
**Issue**: "No graphical session detected"

**Solution**: Ensure `DISPLAY` environment variable is set:

```bash
export DISPLAY=:0
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

Alternatively, use `xvfb` for headless environments:

```bash
xvfb-run -a dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

**On macOS:**
The application uses the native Cocoa backend and should work out of the box. If you encounter issues:

1. Ensure Docker Desktop is running
2. Check that the MariaDB container is healthy: `docker ps`
3. Verify database connection in `.env` file

### 🔌 Database connection failed

**Issue**: Cannot connect to MariaDB

**Solutions**:

1. Ensure database container is running: `docker ps`
2. Check database logs: `docker-compose logs mariadb`
3. Verify connection string in `appsettings.json`
4. Ensure database has been initialized (check `db/workshop.sql` was loaded)

### 🏗️ Docker build fails

**Issue**: Build errors during `docker build`

**Solutions**:

1. Clear Docker cache: `docker system prune -a`
2. Rebuild without cache: `docker-compose build --no-cache`
3. Check Docker has enough disk space

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/)
- Database: [MariaDB](https://mariadb.org/)
- Icons and fonts: Various open-source resources

## 💬 Support

For issues, questions, or contributions, please open an issue on the GitHub repository.

---

**Made with ❤️ using .NET and Avalonia UI**
