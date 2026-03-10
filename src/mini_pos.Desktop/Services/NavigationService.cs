using System;

using Microsoft.Extensions.DependencyInjection;

namespace mini_pos.Services;

/// <summary>
/// Service for navigating between views and creating ViewModels with proper DI.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Creates a ViewModel of type T with all dependencies injected.
    /// </summary>
    T CreateViewModel<T>() where T : ViewModels.ViewModelBase;

    /// <summary>
    /// Creates a ViewModel with additional constructor arguments.
    /// </summary>
    T CreateViewModelWithArgs<T>(params object[] args) where T : ViewModels.ViewModelBase;
}

/// <summary>
/// Implementation of navigation service using Microsoft DI container.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T CreateViewModel<T>() where T : ViewModels.ViewModelBase
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T CreateViewModelWithArgs<T>(params object[] args) where T : ViewModels.ViewModelBase
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
    }
}
