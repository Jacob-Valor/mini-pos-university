using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly Employee _currentUser;
    private readonly IDialogService? _dialogService;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _surname = string.Empty;

    [ObservableProperty]
    private string _gender = "ຊາຍ";

    [ObservableProperty]
    private DateTimeOffset _dateOfBirth = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _province = string.Empty;

    [ObservableProperty]
    private string _district = string.Empty;

    [ObservableProperty]
    private string _village = string.Empty;

    [ObservableProperty]
    private string _position = string.Empty;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _oldPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public ObservableCollection<string> Provinces { get; } = new() { "ນະຄອນຫຼວງວຽງຈັນ", "ວຽງຈັນ", "ຫຼວງພະບາງ", "ຈຳປາສັກ" };
    public ObservableCollection<string> Districts { get; } = new() { "ສີສັດຕະນາກ", "ໄຊເສດຖາ", "ຈັນທະບູລີ", "ສີໂຄດຕະບອງ" };
    public ObservableCollection<string> Villages { get; } = new() { "ວັດນາກ", "ທາດຂາວ", "ທົ່ງພານທອງ", "ດົງປ່າລານ" };
    public ObservableCollection<string> Positions { get; } = new() { "Admin", "Employee", "Manager" };

    public ProfileViewModel(Employee employee, IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _currentUser = employee;
        _databaseService = databaseService;
        _dialogService = dialogService;
        LoadUserData();
    }

    private void LoadUserData()
    {
        Id = _currentUser.Id;
        Username = _currentUser.Username;
        Name = _currentUser.Name;
        Surname = _currentUser.Surname;
        Gender = _currentUser.Gender;
        DateOfBirth = _currentUser.DateOfBirth;
        StartDate = _currentUser.DateOfBirth;
        PhoneNumber = _currentUser.PhoneNumber;
        Province = _currentUser.Province;
        District = _currentUser.District;
        Village = _currentUser.Village;
        Position = _currentUser.Position;
        ImagePath = _currentUser.ImagePath;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        _currentUser.Name = Name;
        _currentUser.Surname = Surname;
        _currentUser.Gender = Gender;
        _currentUser.DateOfBirth = DateOfBirth.DateTime;
        _currentUser.PhoneNumber = PhoneNumber;
        _currentUser.Username = Username;

        bool success = await _databaseService.UpdateEmployeeProfileAsync(_currentUser);
        if (success)
        {
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ອັບເດດໂປຣໄຟລ໌ສຳເລັດ (Profile updated)");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ອັບເດດໂປຣໄຟລ໌ບໍ່ສຳເລັດ (Failed to update profile)");
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນລະຫັດຜ່ານເກົ່າ ແລະ ລະຫັດໃໝ່");
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ລະຫັດຜ່ານໃໝ່ບໍ່ຕົງກັນ");
            return;
        }

        var storedHash = await _databaseService.GetStoredPasswordHashAsync(Username);
        if (storedHash == null || !PasswordHelper.VerifyPassword(OldPassword, storedHash))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ລະຫັດຜ່ານເກົ່າບໍ່ຖືກຕ້ອງ");
            return;
        }

        string newHash = PasswordHelper.HashPassword(NewPassword);
        bool success = await _databaseService.UpdatePasswordAsync(Id, newHash);

        if (success)
        {
            OldPassword = "";
            NewPassword = "";
            ConfirmPassword = "";
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ປ່ຽນລະຫັດຜ່ານສຳເລັດ (Password changed)");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ປ່ຽນລະຫັດຜ່ານບໍ່ສຳເລັດ");
        }
    }
}
