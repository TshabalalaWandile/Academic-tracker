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
        private SQLiteAsyncConnection _db;

        public async Task InitAsync()
        {
            if (_db != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trackwise.db3");
            _db = new SQLiteAsyncConnection(dbPath);

            await _db.CreateTableAsync<User>();
            await _db.CreateTableAsync<Module>();
            await _db.CreateTableAsync<Assessment>();
        }

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
    }
}
