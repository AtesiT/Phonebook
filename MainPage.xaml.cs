using Phonebook.Models;
using Phonebook.Services;
using Contact = Phonebook.Models.Contact;
using ClosedXML.Excel;
using Communication = Microsoft.Maui.ApplicationModel.Communication;

namespace Phonebook;

public partial class MainPage : ContentPage
{
    private DatabaseService _databaseService;
    private List<Contact> _allContacts;
    public MainPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadContacts();
    }

    private async Task LoadContacts()
    {
        _allContacts = await GetAllContacts();
        contactsList.ItemsSource = _allContacts;
    }

    private async Task<List<Contact>> GetAllContacts()
    {
        _allContacts = await _databaseService.GetContactsAsync();

        PermissionStatus status1 = await Permissions.RequestAsync<Permissions.ContactsRead>();
        PermissionStatus status2 = await Permissions.RequestAsync<Permissions.ContactsWrite>();

        if (status1 == PermissionStatus.Granted && status2 == PermissionStatus.Granted)
        {
            var contacts = GetContacts();
            await foreach (var contact in contacts)
            {
                _allContacts.Add(contact);
            }
        }
        return _allContacts;
    }

    public async IAsyncEnumerable<Contact> GetContacts()
    {
        var contacts = await Communication.Contacts.Default.GetAllAsync();
        if (contacts == null)
            yield break;

        int i = 0;
        foreach (var contact in contacts)
        {
            i++;
            if (i >= 10) yield break;

            var _contact = new Contact();
            int _id;
            var success = int.TryParse(contact.Id, out _id);

            if (success)
                _contact.Id = _id;
            else
                _contact.Id = 0;

            _contact.Name = contact.DisplayName;
            _contact.PhoneNumber = contact.Phones.FirstOrDefault()?.PhoneNumber ?? "";
            _contact.Email = contact.Emails.FirstOrDefault()?.EmailAddress ?? "";
            _contact.Address = "";
            _contact.Description = "";
            _contact.PhotoPath = "";

            yield return _contact;
        }
    }

    private async void OnContactSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Contact selectedContact)
        {
            await Navigation.PushAsync(new ContactDetailPage(selectedContact));
            contactsList.SelectedItem = null;
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchTerm = e.NewTextValue;
        var filteredContacts = await _databaseService.SearchContactsAsync(searchTerm);
        contactsList.ItemsSource = filteredContacts;
    }

    private async void OnAddContactClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ContactDetailPage(new Contact()));
    }

    private async void OnExportContactClicked(object sender, EventArgs e)
    {
        await ExportToXLSX();
    }

    private async Task ExportToXLSX()
    {
        try
        {
            _allContacts = await GetAllContacts();
            if (_allContacts.Count <= 0)
                return;

            var fileName = $"contacts_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Contacts");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Имя";
                worksheet.Cell(1, 3).Value = "Телефон";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Адрес";
                worksheet.Cell(1, 6).Value = "Описание";
                worksheet.Cell(1, 7).Value = "Фото";

                for (int i = 0; i < _allContacts.Count; i++)
                {
                    var contact = _allContacts[i];
                    worksheet.Cell(2 + i, 1).Value = contact.Id;
                    worksheet.Cell(2 + i, 2).Value = contact.Name;
                    worksheet.Cell(2 + i, 3).Value = contact.PhoneNumber;
                    worksheet.Cell(2 + i, 4).Value = contact.Email;
                    worksheet.Cell(2 + i, 5).Value = contact.Address;
                    worksheet.Cell(2 + i, 6).Value = contact.Description;
                    worksheet.Cell(2 + i, 7).Value = contact.PhotoPath;
                }

                workbook.SaveAs(localPath);
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Экспорт контактов",
                File = new ShareFile(localPath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось экспортировать: {ex.Message}", "OK");
        }
    }

    private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            contactsList.ItemsSource = _allContacts;
        }
        else
        {
            contactsList.ItemsSource = await _databaseService.SearchContactsAsync(e.NewTextValue);
        }
    }
}