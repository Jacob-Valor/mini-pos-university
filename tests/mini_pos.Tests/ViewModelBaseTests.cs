using Xunit;

namespace mini_pos.Tests;

public class ViewModelBaseTests
{
    [Fact]
    public void ViewModelBase_CanBeInstantiated()
    {
        var vm = new TestableViewModel();

        Assert.NotNull(vm);
    }

    [Fact]
    public void ViewModelBase_ObservableProperty_ChangesCorrectly()
    {
        var vm = new TestableViewModel();

        vm.TestProperty = "Hello";

        Assert.Equal("Hello", vm.TestProperty);
    }

    [Fact]
    public void ViewModelBase_CanSetMultipleProperties()
    {
        var vm = new TestableViewModel();

        vm.TestProperty = "Value1";
        vm.AnotherProperty = 42;

        Assert.Equal("Value1", vm.TestProperty);
        Assert.Equal(42, vm.AnotherProperty);
    }

    private class TestableViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private string _testProperty = string.Empty;
        private int _anotherProperty;

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        public int AnotherProperty
        {
            get => _anotherProperty;
            set => SetProperty(ref _anotherProperty, value);
        }
    }
}
