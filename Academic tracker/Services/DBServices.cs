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

            // Get path to store database in app data directory
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trackwise.db3");


            _db = new SQLiteAsyncConnection(dbPath);

            // Create tables for all models if they don't already exist
            await _db.CreateTableAsync<User>();
            await _db.CreateTableAsync<Module>();
            await _db.CreateTableAsync<Assessment>();
        }

        // Retrieves a user by their email address. Email is used during login to find the user account.
        public async Task<User> GetUserByEmailAsync(string email)
        {
            await InitAsync();
            return await _db.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }


        // Adds a new user to the database. Method is called during account registration.
        public async Task AddUserAsync(User user)
        {
            await InitAsync();
            await _db.InsertAsync(user);
        }

        // Retrieves all modules for a specific user.Used on the Dashboard to display the user's academic modules.
        public async Task<List<Module>> GetModulesByUserAsync(int userID)
        {
            await InitAsync();
            return await _db.Table<Module>().Where(m => m.UserID == userID).ToListAsync();
        }

        // Adds a new module to the database. Called when user creates a new module from the Dashboard
        public async Task AddModuleAsync(Module module)
        {
            await InitAsync();
            await _db.InsertAsync(module);
        }

        // Retrieves all assessments for a specific module. Used on ModuleDetailPage to display assessments.
        public async Task<List<Assessment>> GetAssessmentsByModuleAsync(int moduleID)
        {
            await InitAsync();
            return await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();
        }

        // Adds a new assessment to the database. Called when user creates a new assessment.
        public async Task AddAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.InsertAsync(assessment);
        }

        // Calculates the weighted average (running mark) for a module.
        // Formula: Sum of (Mark% × Weighting) for all assessments.
        // Example: If Assessment A is 90% with 50% weight and Assessment B is 80% with 50% weight,
        // running mark = (90 × 0.5) + (80 × 0.5) = 85%.
        public async Task<double> GetRunningMarkAsync(int moduleID)
        {
            await InitAsync();
            var assessments = await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();

            // Sum weighted percentages from all assessments
            double runningMark = 0;
            foreach (var a in assessments)
            {
                // Calculate assessment percentage and apply weighting
                if (a.TotalMark > 0)
                    runningMark += (a.MarkObtained / a.TotalMark) * a.Weighting;
            }
            return runningMark;
        }

        // Updates an existing module's information. Called when user edits module details (name, code, or target mark).
        public async Task UpdateModuleAsync(Module module)
        {
            await InitAsync();
            await _db.UpdateAsync(module);
        }

        // Deletes a module from the database. This should cascade delete all assessments for this module.
        public async Task DeleteModuleAsync(Module module)
        {
            await InitAsync();
            await _db.DeleteAsync(module);
        }

        // Deletes all assessments belonging to a specific module and must be called before deleting the module to avoid orphaned records.
        public async Task DeleteAssessmentsByModuleAsync(int moduleID)
        {
            await InitAsync();
            await _db.Table<Assessment>().DeleteAsync(a => a.ModuleID == moduleID);
        }

        // Updates an existing assessment's information. Called when user edits assessment details.
        public async Task UpdateAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.UpdateAsync(assessment);
        }

        // Deletes an assessment from the database.
        public async Task DeleteAssessmentAsync(Assessment assessment)
        {
            await InitAsync();
            await _db.DeleteAsync(assessment);
        }

        // Calculates the total weighting of all assessments in a module. Used to validate that total weighting doesn't exceed 100%.
        public async Task<double> GetTotalWeightingAsync(int moduleID)
        {
            await InitAsync();
            var assessments = await _db.Table<Assessment>().Where(a => a.ModuleID == moduleID).ToListAsync();
            return assessments.Sum(a => a.Weighting);
        }

        // Checks if a module code already exists for a user. Used to prevent duplicate module codes within a user's module list.
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
