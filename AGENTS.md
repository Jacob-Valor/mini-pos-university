# AGENTS.md - Development Guidelines for mini_pos

## Build Commands
- `dotnet build` - Build the solution
- `dotnet run` - Run the application
- `dotnet test` - Run all tests (add test project first)
- `dotnet test --filter "TestName"` - Run single test

## Code Style Guidelines

### Architecture
- MVVM pattern using ReactiveUI (ViewModelBase inherits from ReactiveObject)
- Use compiled bindings by default (AvaloniaUseCompiledBindingsByDefault=true)
- Handle DBus TaskCanceledException gracefully in Program.cs

### Naming Conventions
- Classes: PascalCase (MainWindowViewModel, ViewModelBase)
- Properties: PascalCase with { get; set; }
- Methods: PascalCase
- Fields: _camelCase with underscore prefix
- Namespaces: lowercase_underscore (mini_pos, mini_pos.ViewModels)

### Import Organization
1. System imports (using System;)
2. Third-party imports (using Avalonia;, using ReactiveUI;)
3. Project imports (using mini_pos.ViewModels;)

### Types & Nullability
- Nullable reference types enabled
- Use string? for optional strings
- Prefer explicit null checks over ! operator

### Error Handling
- Catch TaskCanceledException for DBus issues on Linux
- Use standard C# exception handling
- Validate user input in ViewModels

### File Organization
- Views/ folder for AXAML files
- ViewModels/ folder for ViewModel classes
- Models/ folder for data models (currently empty)