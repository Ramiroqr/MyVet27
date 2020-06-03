using MyVet.Common.Models;
using MyVet.Common.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyVet.Prism.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private string _passwword;
        private bool _isRunning;
        private bool _isEnabled;
        private DelegateCommand _loginCommand;

        public LoginPageViewModel(
            INavigationService navigationService,
            IApiService apiService) : base(navigationService)
        {
            Title = "Login";
            IsEnabled = true;
            _navigationService = navigationService;
            _apiService = apiService;

            Email = "jzuluaga55@hotmail.com";
            Password = "123456";
        }

        //public DelegateCommand LoginCommand
        //{
        //    get
        //    {
        //        if (_loginCommand == null)
        //            _loginCommand = new DelegateCommand(Login);

        //        return _loginCommand;
        //    }
        //} es lo mismo que la linea de abajo

        public DelegateCommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(Login));



        public string Email { get; set; }

        public string Password
        {
            get => _passwword;
            set => SetProperty(ref _passwword, value);
        }

        public bool IsRuuning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        private async void Login()
        {
            if (string.IsNullOrEmpty(Email))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter an email.",
                    "Accept");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                await App.Current.MainPage.DisplayAlert(
                "Error",
                "You must enter a password.",
                "Accept");
                return;
            }

            IsRuuning = true;
            IsEnabled = false;

            var url = App.Current.Resources["UrlAPI"].ToString();
            var connection = await _apiService.CheckConnection(url);
            if (!connection)
            {
                IsEnabled = true;
                IsRuuning = false;
                await App.Current.MainPage.DisplayAlert("Error", "Check the internet connection.", "Accept");
                return;
            }


            var request = new TokenRequest
            {
                Password = Password,
                Username = Email
            };

            var response = await _apiService.GetTokenAsync(url, "Account", "/CreateToken", request);

            

            if(!response.IsSuccess)
            {
                IsRuuning = false;
                IsEnabled = true;
                await App.Current.MainPage.DisplayAlert("Error", "Email or passwword incorrect.", "Accept");
                Password = String.Empty;
                return;
            }

            var token = response.Result;
            var response2 = await _apiService.GetOwnerByEmailAsync(url, "api", "/Owners/GetOwnerByEmail", "bearer", token.Token, Email);

            if (!response2.IsSuccess)
            {
                IsRuuning = false;
                IsEnabled = true;
                await App.Current.MainPage.DisplayAlert("Error", "This user have a big problem, call support", "Accept");
                return;
            }

            var owner = response2.Result;

            IsRuuning = false;
            IsEnabled = true;

            var parameters = new NavigationParameters();
            parameters = new NavigationParameters
            {
                {"owner", owner }
            };

            IsRuuning = false;
            IsEnabled = true;

            await _navigationService.NavigateAsync("PetsPage", parameters);
            Password = string.Empty;
        }
    }
}
