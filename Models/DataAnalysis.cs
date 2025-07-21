using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Salary_Disparity_Analysis.Models
{
    public class SalaryDisparityRecord
    {
        public int Id { get; set; }

        [Required]
        public string Industry { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Salary { get; set; }

        [Display(Name = "Years of Experience")]
        [Range(0, 50)]
        public double YearsExperience { get; set; }

        [Display(Name = "Educational Level")]
        public string EducationalLevel { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; }
    }
}

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        this.AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}



































