using Phonebook.Models;
using Phonebook.Services;
using Contact = Phonebook.Models.Contact;

namespace Phonebook
{
    public partial class ContactDetailPage : ContentPage
    {
        private Contact _contact;
        private DatabaseService _databaseService;
        public ContactDetailPage(Contact contact)
        {
            InitializeComponent();
            _contact = contact;
            _databaseService = new DatabaseService();
            BindingContext = _contact;
            LoadContactData();
        }

        private async void LoadContactData()
        {
            if (_contact.Id > 0)
            {
                var fullContact = await _databaseService.GetContactAsync(_contact.Id);
                if (fullContact != null)
                {
                    _contact = fullContact;
                }
            }

            nameEntry.Text = _contact.Name;
            phoneEntry.Text = _contact.PhoneNumber;
            descriptionEntry.Text = _contact.Description;
            addressEntry.Text = _contact.Address;
            emailEntry.Text = _contact.Email;

            if (!string.IsNullOrEmpty(_contact.PhotoPath) && File.Exists(_contact.PhotoPath))
            {
                contactImage.Source = ImageSource.FromFile(_contact.PhotoPath);
            }
            else
            {
                contactImage.Source = "person.png";
                _contact.PhotoPath = null;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _contact.Name = nameEntry.Text;
            _contact.PhoneNumber = phoneEntry.Text;
            _contact.Description = descriptionEntry.Text;
            _contact.Email = emailEntry.Text;
            _contact.Address = addressEntry.Text;

            await _databaseService.SaveContactAsync(_contact);
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Удаление", "Вы уверены, что хотите удалить этот контакт?", "Да", "Нет");
            if (answer)
            {
                await _databaseService.DeleteContactAsync(_contact);
                await Navigation.PopAsync();
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            try
            {
                var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("Внимание", "Разрешите доступ к хранилищу для выбора фото", "ОК");
                    return;
                }

                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите фото контакта"
                });

                if (result != null)
                {
                    var localFilePath = await CopyFileToAppDirectory(result.FullPath);
                    contactImage.Source = ImageSource.FromFile(localFilePath);
                    _contact.PhotoPath = localFilePath;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
            }
        }

        private async Task<string> CopyFileToAppDirectory(string sourceFilePath)
        {
            var fileName = $"contact_{DateTime.Now.Ticks}.jpg";
            var appDataDir = FileSystem.AppDataDirectory;
            var destinationPath = Path.Combine(appDataDir, fileName);
            File.Copy(sourceFilePath, destinationPath, true);
            return destinationPath;
        }
    }
}