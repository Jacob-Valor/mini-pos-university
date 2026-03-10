# Development Guidelines for mini_pos

This document outlines the build commands, code style, and architectural patterns for the `mini_pos` project.

## 🚀 Quick Start

### 1. Setup Database (All Platforms)

```bash
# Start MariaDB in Docker
docker-compose up -d mariadb

# Verify database is running
docker ps  # Should show mini_pos_db as "healthy"
```

### 2. Run the Application

```bash
# Build and run
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj

# Or use hot reload for development
dotnet watch --project src/mini_pos.Desktop/mini_pos.csproj run
```

## 🛠️ Build Commands

| Command                           | Description                                         |
| --------------------------------- | --------------------------------------------------- |
| `dotnet restore`                  | Restore NuGet packages.                             |
| `dotnet build`                    | Build the solution.                                 |
| `dotnet build -c Release`         | Build in Release mode.                              |
| `dotnet run --project src/mini_pos.Desktop/mini_pos.csproj`                      | Run the desktop application.                                |
| `dotnet run --project src/mini_pos.Desktop/mini_pos.csproj --no-build`           | Run without rebuilding.                             |
| `dotnet watch --project src/mini_pos.Desktop/mini_pos.csproj run`                | Run with hot reload (auto-rebuild on file changes). |
| `dotnet test`                     | Run all tests (requires test project).              |
| `dotnet test --filter "TestName"` | Run a specific test case.                           |

## 🖥️ Cross-Platform Development

### macOS

```bash
# Install .NET SDK
brew install --cask dotnet-sdk

# Start Docker Desktop
open -a Docker

# Start database and run app
docker-compose up -d mariadb
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

### Linux

```bash
# Install .NET SDK
sudo apt-get install -y dotnet-sdk-10.0

# Start Docker
sudo systemctl start docker

# Start database and run app
docker-compose up -d mariadb
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj

# If running remotely via SSH, use xvfb:
xvfb-run -a dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

### Windows

```bash
# Install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0

# Start Docker Desktop

# Start database and run app
docker-compose up -d mariadb
dotnet run --project src/mini_pos.Desktop/mini_pos.csproj
```

## 🐳 Docker Commands

| Command                                        | Description                    |
| ---------------------------------------------- | ------------------------------ |
| `docker-compose up -d mariadb`                 | Start MariaDB database.        |
| `docker-compose down`                          | Stop database.                 |
| `docker-compose down -v`                       | Stop and delete database data. |
| `docker-compose logs -f mariadb`               | View database logs.            |
| `docker ps`                                    | Show running containers.       |
| `docker exec -it mini_pos_db mysql -u root -p` | Access MySQL CLI.              |

## 🏗️ Architecture

- **Pattern**: MVVM using [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) (`ViewModelBase`).
- **UI Framework**: [Avalonia UI](https://avaloniaui.net/) - Cross-platform .NET XAML framework.
- **Bindings**: Compiled bindings enabled (`AvaloniaUseCompiledBindingsByDefault=true`).
- **Cross-Platform**: Supports Windows, macOS (Intel/Apple Silicon), and Linux.

### Platform-Specific Considerations

| Platform    | Backend     | Notes                         |
| ----------- | ----------- | ----------------------------- |
| **Windows** | WPF/Win32   | Native Windows look and feel. |
| **macOS**   | Cocoa       | Requires macOS 10.14+.        |
| **Linux**   | X11/Wayland | Handles display gracefully.   |

### Error Handling

- **Cross-platform**: Handle display connection failures gracefully.
- **Standard**: Use `try-catch` blocks for expected exceptions.
- **Validation**: Validate all user input within ViewModels before processing.

## 📂 Project Structure

```
mini_pos/
├── db/
│   └── workshop.sql     # Database schema and sample data
├── src/
│   ├── mini_pos.Desktop/    # Avalonia desktop app project
│   └── mini_pos.Api/        # ASP.NET Core API project
├── tests/
│   ├── mini_pos.Tests/      # Desktop app tests
│   └── mini_pos.Api.Tests/  # API tests
├── mini_pos.sln         # Solution file
├── docker-compose.yml   # Database container configuration
└── .env                 # Environment variables (gitignored)
```

## 📦 Code Style Guidelines

### Naming Conventions

| Entity         | Convention             | Example                            |
| -------------- | ---------------------- | ---------------------------------- |
| **Classes**    | PascalCase             | `MainWindowViewModel`, `Product`   |
| **Properties** | PascalCase             | `public string Name { get; set; }` |
| **Methods**    | PascalCase             | `CalculateTotal()`                 |
| **Fields**     | `_camelCase`           | `_itemsList`                       |
| **Constants**  | UPPER_SNAKE_CASE       | `MAX_RETRY_COUNT`                  |
| **Namespaces** | `lowercase_underscore` | `mini_pos`, `mini_pos.ViewModels`  |

### Import Organization

1. **System imports**
   ```csharp
   using System;
   using System.Collections.Generic;
   ```
2. **Third-party imports**
   ```csharp
   using Avalonia;
   using CommunityToolkit.Mvvm.ComponentModel;
   ```
3. **Project imports**
   ```csharp
   using mini_pos.ViewModels;
   using mini_pos.Models;
   ```

### Types & Nullability

- **Nullable reference types**: Enabled globally.
- **Optional values**: Use `string?` (or other nullable types) for optional values.
- **Safety**: Prefer explicit null checks (`if (x is not null)`) over the null-forgiving operator (`!`).

### File Organization

| Directory     | Contents                       |
| ------------- | ------------------------------ |
| `src/mini_pos.Desktop/Views/`      | `.axaml` files and code-behind |
| `src/mini_pos.Desktop/ViewModels/` | MVVM ViewModel classes         |
| `src/mini_pos.Desktop/Models/`     | Data models and entities       |
| `src/mini_pos.Desktop/Converters/` | Value converters for binding   |
| `src/mini_pos.Desktop/Services/`   | Business logic services        |

## 🔧 Configuration

### Environment Variables

All configuration is managed via `.env` file:

| Variable      | Description       | Default         |
| ------------- | ----------------- | --------------- |
| `DB_HOST`     | Database host     | `localhost`     |
| `DB_PORT`     | Database port     | `3306`          |
| `DB_NAME`     | Database name     | `mini_pos`      |
| `DB_USER`     | Database user     | `root`          |
| `DB_PASSWORD` | Database password | `root_password` |

**Note:** Copy `.env.example` to `.env` and update values before running.

## 📝 Notes

- The application runs **natively** on all platforms (not in Docker).
- Docker is used **only for the database** (MariaDB).
- The `.env` file contains sensitive data and is gitignored.
- Use `dotnet watch --project src/mini_pos.Desktop/mini_pos.csproj run` for development with hot reload.
