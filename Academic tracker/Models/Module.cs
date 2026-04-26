using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Academic_tracker.Models
{
    public class Module
    {
        [PrimaryKey, AutoIncrement]
        public int ModuleID { get; set; }
        public int UserID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public double TargetMark { get; set; }
    }
}
