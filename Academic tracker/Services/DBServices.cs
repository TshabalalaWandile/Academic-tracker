using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Academic_tracker.Models;

namespace Academic_tracker.Services
{
    public class DBServices
    {
        // SQLite database connection instance.
        private SQLiteAsyncConnection _db;

        // Initializes the database connection and creates tables if they don't exist.
        public async Task InitAsync()
        {
            // Return early if already initialized
            if (_db != null)
            {
                return;
            }

            try
            {
                // Get path to store database in app data directory
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trackwise.db3");


                _db = new SQLiteAsyncConnection(dbPath);

                // Create tables for all models if they don't already exist
                await _db.CreateTableAsync<User>();
                await _db.CreateTableAsync<Module>();
                await _db.CreateTableAsync<Assessment>();
            }
            catch (Exception ex)
            {
                // Log the error to the Visual Studio Output window during debugging
                System.Diagnostics.Debug.WriteLine("Database initialisation failed: " + ex.Message);

                // Reset _db to null so the next call to InitAsync can attempt initialisation again
                _db = null;

                // Re-throw the exception so the calling code knows initialisation failed
                throw;
            }

        }

        // ===================== USER OPERATIONS =====================

        // Retrieves a user by their email address. Email is used during login to find the user account.
        public async Task<User> GetUserByEmailAsync(string email)
        {
            await InitAsync();
            return await _db.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await InitAsync();
            await _db.InsertAsync(user);
        }

        public async Task<List<Module>> GetModulesByUserAsync(int userID)
        {
            await InitAsync();
            return await _db.Table<Module>().Where(m => m.UserID == userID).ToListAsync();
        }

        public async Task AddModuleAsync(Module module)
        {
            await InitAsync();
            await _db.InsertAsync(module);
        }

        public async Task<List<Assessment>> GetAssessmentsByModuleAsync(int moduleID)
        {
            await InitAsync();
            return await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();
        }

        public async Task AddAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.InsertAsync(assessment);
        }

        public async Task<double> GetRunningMarkAsync(int moduleID)
        {
            await InitAsync();
            var assessments = await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();

            double runningMark = 0;
            foreach (var a in assessments)
            {
                if (a.TotalMark > 0)
                    runningMark += (a.MarkObtained / a.TotalMark) * a.Weighting;
            }
            return runningMark;
        }

        public async Task UpdateModuleAsync(Module module)
        {
            await InitAsync();
            await _db.UpdateAsync(module);
        }

        public async Task DeleteModuleAsync(Module module)
        {
            await InitAsync();
            await _db.DeleteAsync(module);
        }

        public async Task UpdateAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.UpdateAsync(assessment);
        }

        public async Task DeleteAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.DeleteAsync(assessment);
        }

        public async Task<double> GetTotalWeightingAsync(int moduleID)
        {
            await InitAsync();
            var assessments = await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();
            return assessments.Sum(a => a.Weighting);
        }

        public async Task<bool> ModuleCodeExistsAsync(string moduleCode, int userID, int excludeModuleId = 0)
        {
            await InitAsync();
            var existing = await _db.Table<Module>()
        .Where(m =>
            m.ModuleCode.ToLower() == moduleCode.ToLower() &&
            m.UserID == userID &&
            m.ModuleID != excludeModuleId)
        .FirstOrDefaultAsync();

            return existing != null;
        }

    }
}
