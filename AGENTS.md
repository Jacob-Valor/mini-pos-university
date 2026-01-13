# Development Guidelines for mini_pos

This document outlines the build commands, code style, and architectural patterns for the `mini_pos` project.

## Build Commands

| Command | Description |
| --- | --- |
| `dotnet build` | Build the solution. |
| `dotnet run` | Run the application. |
| `dotnet test` | Run all tests (requires test project). |
| `dotnet test --filter "TestName"` | Run a specific test case. |

## Code Style Guidelines

### Architecture

- **Pattern**: MVVM using [ReactiveUI](https://www.reactiveui.net/) (`ViewModelBase` inherits from `ReactiveObject`).
- **Bindings**: Use compiled bindings by default (`AvaloniaUseCompiledBindingsByDefault=true`).
- **Resilience**: Handle DBus `TaskCanceledException` gracefully in `Program.cs`.

### Naming Conventions

| Entity | Convention | Example |
| --- | --- | --- |
| **Classes** | PascalCase | `MainWindowViewModel`, `ViewModelBase` |
| **Properties** | PascalCase | `public string Name { get; set; }` |
| **Methods** | PascalCase | `CalculateTotal()` |
| **Fields** | `_camelCase` | `_itemsList` |
| **Namespaces** | `lowercase_underscore` | `mini_pos`, `mini_pos.ViewModels` |

### Import Organization

1. **System imports**
   ```csharp
   using System;
   using System.Collections.Generic;
   ```
2. **Third-party imports**
   ```csharp
   using Avalonia;
   using ReactiveUI;
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

### Error Handling

- **Linux specific**: Catch `TaskCanceledException` for DBus issues.
- **Standard**: Use standard C# `try-catch` blocks for expected exceptions.
- **Validation**: Validate all user input within the ViewModels before processing.

### File Organization

- **`Views/`**: Contains `.axaml` files and their code-behind.
- **`ViewModels/`**: Contains ViewModel classes implementing the logic.
- **`Models/`**: Contains data models and business entities.
