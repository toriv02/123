using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using TBmobile.classes;


namespace TBmobile
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        async void Enter (object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TabbedPage1());
            /*var username = login.Text;
            var password = pass.Text;

            try
            {
                var response = await GetToken(username, password);
                if (!string.IsNullOrEmpty(response.AccessToken))
                {
                    // Успешная аутентификация, открыть главное окно
                   
                }
                else
                {
                    await DisplayAlert("Ошибка", "Неверные учетные данные ", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert( "Ошибка", $"Ошибка при подключении к серверу: {ex.Message}", "OK");
            }*/
        }
        private async Task<TokenResponse> GetToken(string username, string password)
        {
            var url = "https://localhost:7229/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                    return tokenResponse;
                }
                catch (JsonException ex)
                {
                    await DisplayAlert ("Ошибка", $"Ошибка при разборе ответа: {ex.Message}", "OK");
                    return null;
                }
            }
            else
            {
                await DisplayAlert ( "Ошибка", $"Ошибка сервера: {response.StatusCode}", "OK");
                return new TokenResponse(); // Возвращаем пустой токен, если ошибка
            }
        }
    }
}
