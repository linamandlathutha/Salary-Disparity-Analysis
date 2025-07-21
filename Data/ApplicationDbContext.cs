using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Salary_Disparity_Analysis.Models;

namespace Salary_Disparity_Analysis.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MockData>MockDatas { get; set; }
        public DbSet<SalaryDisparityRecord> SalaryDisparityRecords {get; set; }
    }
}
