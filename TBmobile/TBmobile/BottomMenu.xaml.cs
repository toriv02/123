using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TBmobile.classes;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace TBmobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class TabbedPage1 : TabbedPage
    {
        
        private HttpClient _httpClient = new HttpClient();
        private string _accessToken;
        public TabbedPage1()
        {
            InitializeComponent();
            /*// Получаем навигационную панель текущей страницы
            NavigationPage navPage = Application.Current.MainPage as NavigationPage;
            // Устанавливаем цвет фона навигационной панели
            navPage.BarBackgroundColor = Color.FromHex("#FF383838");*/

            _accessToken = _accessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            LoadFileNames();
        }
        private async void LoadFileNames()
        {
            try
            {
                var fileNames = await _httpClient.GetFromJsonAsync<List<FileMain>>("https://localhost:7229/api/FileMain/user-files");
                if (fileNames != null)
                    UpdateUIWithFileNames(fileNames);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to fetch data: " + ex.Message, "OK");
            }
        }
        private async Task DownloadFile(int fileId)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync($"https://localhost:7229/api/FileMain/{fileId}");

                if (response.IsSuccessStatusCode)
                {
                    Stream contentStream = await response.Content.ReadAsStreamAsync();
                    string contentDisposition = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                    string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), contentDisposition);

                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(fileStream);
                        await DisplayAlert("Success", "File downloaded successfully: " + savePath, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to download file. Status: " + response.StatusCode, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task DeleteFile(int fileId)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.DeleteAsync($"https://localhost:7229/api/FileDelete/{fileId}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "File deleted", "OK");
                LoadFileNames();
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete file", "OK");
            }
        }

        private async Task LoadTextContent(int fileId, Label label)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync($"https://localhost:7229/api/FileMain/{fileId}");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    label.Text = content;
                    label.BindingContext = content;
                }
                else
                {
                    label.Text = "Error loading file.";
                }
            }
            catch (Exception ex)
            {
                label.Text = "Error: " + ex.Message;
            }
        }

        private async Task UploadFile(string filePath)
        {
            HttpClient httpClient = new HttpClient();

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

                // Set authorization header with token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                HttpResponseMessage response = await httpClient.PostAsync("https://localhost:7229/api/FileMain", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "File uploaded successfully", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Failed to upload file: {response.StatusCode}\n{responseContent}", "OK");
                }
            }
            LoadFileNames();
        }

        private async void UpdateUIWithFileNames(List<FileMain> files)
        {
            TextFilesPanel.Children.Clear();
            ImageFilesPanel.Children.Clear();
            OtherFilesPanel.Children.Clear();

            foreach (var file in files)
            {
                Grid grid = new Grid { Margin = new Thickness(5) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Label fileText = new Label
                {
                    Text = file.Name,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    TextColor = Color.White,
                    LineBreakMode = LineBreakMode.NoWrap,
                };

                Button firstButton = new Button
                {
                    Text = "Copy",
                    Margin = new Thickness(5, 0, 5, 0)
                };

                Button deleteButton = new Button
                {
                    Text = "Delete"
                };
                deleteButton.Clicked += async (s, e) => { await DeleteFile(file.ID); };

                Grid.SetColumn(fileText, 0);
                Grid.SetColumn(firstButton, 1);
                Grid.SetColumn(deleteButton, 2);

                grid.Children.Add(fileText);
                grid.Children.Add(firstButton);
                grid.Children.Add(deleteButton);

                Frame frame = new Frame
                {
                    BackgroundColor = Color.FromHex("#333333"),
                    CornerRadius = 10,
                    Padding = 10,
                    Margin = 5,
                    Content = grid
                };

                if (file.Name.StartsWith("Text_"))
                {
                    firstButton.Text = "Copy";
                    firstButton.Clicked += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(fileText.Text))
                            Xamarin.Essentials.Clipboard.SetTextAsync(fileText.Text);
                    };

                    TextFilesPanel.Children.Add(frame);
                    LoadTextContent(file.ID, fileText);
                }
                else if (file.Extension == ".jpg" || file.Extension == ".png" || file.Extension == ".gif")
                {
                    firstButton.Text = "Download";
                    firstButton.Clicked += async (s, e) => { await DownloadFile(file.ID); };

                    ImageFilesPanel.Children.Add(frame);
                }
                else
                {
                    firstButton.Text = "Download";
                    firstButton.Clicked += async (s, e) => { await DownloadFile(file.ID); };

                    OtherFilesPanel.Children.Add(frame);
                }
            }
        }
        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            ResetControls();
            ImageScrollViewer.IsVisible = true;
        }

        private void FileButton_Clicked(object sender, EventArgs e)
        {
            ResetControls();
            OtherScrollViewer.IsVisible = true;
        }

        private void ResetControls()
        {
            TextInput.IsVisible = false;
            SendTextButton.IsVisible = false;
        }

        private async void SendTextButton_Clicked(object sender, EventArgs e)
        {
            string textContent = TextInput.Text;
            string fileName = $"Text_{DateTime.Now.Ticks}.txt"; // Уникальное имя файла на основе времени
            byte[] fileBytes = Encoding.UTF8.GetBytes(textContent);

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "file", fileName);

                var response = await _httpClient.PostAsync("https://localhost:7229/api/FileMain", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Успех", "Текстовый файл успешно отправлен", "OK");
                    TextInput.Text = "";
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Ошибка при отправке файла: {response.StatusCode}\n{responseContent}", "OK");
                }
            }
            LoadFileNames();
        }

    }
}