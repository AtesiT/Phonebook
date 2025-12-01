using SQLite;
using Phonebook.Models;
using System.Globalization;

namespace Phonebook.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            Initialize();
        }

        private async void Initialize()
        {
            _database = new SQLiteAsyncConnection(
                Path.Combine(FileSystem.AppDataDirectory, "contacts.db3"),
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache
            );
            await _database.CreateTableAsync<Contact>();
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            return await _database.Table<Contact>().ToListAsync();
        }

        public async Task<List<Contact>> SearchContactsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetContactsAsync();
            var allContacts = await GetContactsAsync();
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return allContacts?
                .Where(c =>
                    compareInfo.IndexOf(c.Name ?? "", searchTerm, CompareOptions.IgnoreCase) >= 0 ||
                    compareInfo.IndexOf(c.PhoneNumber ?? "", searchTerm, CompareOptions.IgnoreCase) >= 0 ||
                    compareInfo.IndexOf(c.Email ?? "", searchTerm, CompareOptions.IgnoreCase) >= 0 ||
                    compareInfo.IndexOf(c.Address ?? "", searchTerm, CompareOptions.IgnoreCase) >= 0 ||
                    compareInfo.IndexOf(c.Description ?? "", searchTerm, CompareOptions.IgnoreCase) >= 0)
                .ToList() ?? new List<Contact>();
        }

        public async Task<Contact> GetContactAsync(int id)
        {
            return await _database.Table<Contact>()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveContactAsync(Contact contact)
        {
            return contact.Id != 0 ?
                await _database.UpdateAsync(contact):
                await _database.InsertAsync(contact);
        }

        public async Task<int> DeleteContactAsync(Contact contact)
        {
            return await _database.DeleteAsync(contact);
        }
    }
}