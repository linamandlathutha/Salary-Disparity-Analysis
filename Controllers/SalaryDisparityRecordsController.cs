using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Salary_Disparity_Analysis.Data;
using Salary_Disparity_Analysis.Models;

namespace Salary_Disparity_Analysis.Controllers
{
    public class SalaryDisparityRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalaryDisparityRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SalaryDisparityRecords
        public async Task<IActionResult> Index(
    string searchString,
    string industryFilter,
    string genderFilter,
    string locationFilter,
    string sortOrder,
    int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["IndustryFilter"] = industryFilter;
            ViewData["GenderFilter"] = genderFilter;
            ViewData["LocationFilter"] = locationFilter;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SalarySortParm"] = sortOrder == "Salary" ? "salary_desc" : "Salary";
            ViewData["ExperienceSortParm"] = sortOrder == "Experience" ? "experience_desc" : "Experience";

            var query = _context.SalaryDisparityRecords.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r =>
                    r.Industry.Contains(searchString) ||
                    r.JobTitle.Contains(searchString) ||
                    r.CompanyName.Contains(searchString) ||
                    r.EducationalLevel.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(industryFilter))
            {
                query = query.Where(r => r.Industry == industryFilter);
            }

            if (!string.IsNullOrEmpty(genderFilter))
            {
                query = query.Where(r => r.Gender == genderFilter);
            }

            if (!string.IsNullOrEmpty(locationFilter))
            {
                query = query.Where(r => r.Location == locationFilter);
            }

            // Apply sorting
            query = sortOrder switch
            {
                "Salary" => query.OrderBy(r => r.Salary),
                "salary_desc" => query.OrderByDescending(r => r.Salary),
                "Experience" => query.OrderBy(r => r.YearsExperience),
                "experience_desc" => query.OrderByDescending(r => r.YearsExperience),
                _ => query.OrderBy(r => r.Industry)
            };

            // Prepare data for charts (using the unfiltered query for overall trends)
            var industries = await _context.SalaryDisparityRecords
                .Select(r => r.Industry)
                .Distinct()
                .ToListAsync();

            var avgSalaries = await _context.SalaryDisparityRecords
                .GroupBy(r => r.Industry)
                .Select(g => new {
                    Industry = g.Key,
                    AverageSalary = g.Average(r => r.Salary)
                })
                .OrderBy(x => x.Industry)
                .Select(x => x.AverageSalary)
                .ToListAsync();

            var salaryExperienceData = await query
                .Select(r => new { x = r.YearsExperience, y = r.Salary })
                .ToListAsync();

            // Prepare dropdown options
            ViewBag.Industries = industries;
            ViewBag.Genders = await _context.SalaryDisparityRecords
                .Select(r => r.Gender)
                .Distinct()
                .ToListAsync();
            ViewBag.Locations = await _context.SalaryDisparityRecords
                .Select(r => r.Location)
                .Distinct()
                .ToListAsync();

            // Chart data
            ViewBag.AvgSalaries = avgSalaries;
            ViewBag.SalaryExperienceData = salaryExperienceData;

            // Pagination
            int pageSize = 10;
            var paginatedRecords = await PaginatedList<SalaryDisparityRecord>.CreateAsync(
                query.AsNoTracking(),
                pageNumber ?? 1,
                pageSize);

            return View(paginatedRecords);
        }


        [HttpGet]
        public async Task<IActionResult> GetAverageSalaryByIndustry()
        {
            var data = await _context.SalaryDisparityRecords
                .GroupBy(r => r.Industry)
                .Select(g => new
                {
                    Industry = g.Key,
                    AverageSalary = g.Average(r => r.Salary)
                })
                .OrderByDescending(x => x.AverageSalary)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalaryExperienceData()
        {
            var data = await _context.SalaryDisparityRecords
                .Select(r => new
                {
                    x = r.YearsExperience,
                    y = r.Salary,
                    gender = r.Gender
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalyticsData()
        {
            var data = await _context.SalaryDisparityRecords.ToListAsync();

            // Calculate average salary
            var averageSalary = data.Average(r => r.Salary);

            // Calculate gender pay gap
            var maleAvg = data.Where(r => r.Gender == "Male").Average(r => r.Salary);
            var femaleAvg = data.Where(r => r.Gender == "Female").Average(r => r.Salary);
            var genderGapPercentage = Math.Round(((maleAvg - femaleAvg) / maleAvg * 100), 1);
            var genderGapNote = maleAvg > femaleAvg
                ? "Men earn more on average"
                : femaleAvg > maleAvg
                    ? "Women earn more on average"
                    : "No gender pay gap";

            // Calculate experience impact
            var experienceValue = data
                .GroupBy(r => Math.Floor(r.YearsExperience))
                .OrderBy(g => g.Key)
                .Select(g => new {
                    Years = g.Key,
                    AvgSalary = g.Average(r => r.Salary)
                })
                .ToList();

            var experienceValuePerYear = experienceValue.Count > 1
                ? Math.Round((experienceValue.Last().AvgSalary - experienceValue.First().AvgSalary) /
                            (experienceValue.Last().Years - experienceValue.First().Years) /
                            experienceValue.First().AvgSalary * 100, 1)
                : 0;

            // Industry comparison
            var industryComparison = data
                .GroupBy(r => r.Industry)
                .Select(g => new {
                    Industry = g.Key,
                    AvgSalary = Math.Round(g.Average(r => r.Salary)),
                    Gap = Math.Round((g.Average(r => r.Salary) - averageSalary) / averageSalary * 100, 1)
                })
                .OrderByDescending(x => x.AvgSalary)
                .ToList();

            // Education impact
            var educationImpact = data
                .GroupBy(r => r.EducationalLevel)
                .Select(g => new {
                    Level = g.Key,
                    AvgSalary = Math.Round(g.Average(r => r.Salary)),
                    Premium = g.Key switch
                    {
                        "Doctorate" => Math.Round((g.Average(r => r.Salary) - averageSalary) / averageSalary * 100, 1),
                        "Master's" => Math.Round((g.Average(r => r.Salary) - averageSalary) / averageSalary * 100, 1),
                        "Bachelor's" => Math.Round((g.Average(r => r.Salary) - averageSalary) / averageSalary * 100, 1),
                        _ => 0
                    }
                })
                .OrderByDescending(x => x.AvgSalary)
                .ToList();

            // Regional analysis
            var regionalAnalysis = data
                .GroupBy(r => r.Location)
                .Select(g => new {
                    Location = g.Key,
                    AvgSalary = Math.Round(g.Average(r => r.Salary)),
                    Variation = Math.Round((g.Average(r => r.Salary) - averageSalary) / averageSalary * 100, 1)
                })
                .OrderByDescending(x => x.AvgSalary)
                .ToList();

            // Key findings
            var keyFindings = new List<dynamic>();

            if (genderGapPercentage > 5)
            {
                keyFindings.Add(new
                {
                    icon = "venus-mars",
                    text = $"Significant gender pay gap detected: {genderGapPercentage}% difference"
                });
            }

            var highestIndustry = industryComparison.First();
            if (highestIndustry.Gap > 15)
            {
                keyFindings.Add(new
                {
                    icon = "industry",
                    text = $"{highestIndustry.Industry} pays {highestIndustry.Gap}% above average (R{highestIndustry.AvgSalary})"
                });
            }

            var lowestIndustry = industryComparison.Last();
            if (lowestIndustry.Gap < -15)
            {
                keyFindings.Add(new
                {
                    icon = "exclamation-triangle",
                    text = $"{lowestIndustry.Industry} pays {Math.Abs(lowestIndustry.Gap)}% below average (R{lowestIndustry.AvgSalary})"
                });
            }

            var highestEducation = educationImpact.First();
            if (highestEducation.Premium > 20)
            {
                keyFindings.Add(new
                {
                    icon = "graduation-cap",
                    text = $"{highestEducation.Level} holders earn {highestEducation.Premium}% more (R{highestEducation.AvgSalary})"
                });
            }

            if (experienceValuePerYear > 5)
            {
                keyFindings.Add(new
                {
                    icon = "user-clock",
                    text = $"Each year of experience adds ~{experienceValuePerYear}% to salary"
                });
            }

            return Json(new
            {
                averageSalary = Math.Round(averageSalary, 2),
                genderGapPercentage,
                genderGapNote,
                experienceValue = Math.Round(experienceValue.LastOrDefault()?.AvgSalary ?? averageSalary, 2),
                experienceValuePerYear,
                industryComparison,
                educationImpact,
                regionalAnalysis,
                keyFindings
            });
        }























        // GET: SalaryDisparityRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salaryDisparityRecord = await _context.SalaryDisparityRecords
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salaryDisparityRecord == null)
            {
                return NotFound();
            }

            return View(salaryDisparityRecord);
        }

        // GET: SalaryDisparityRecords/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SalaryDisparityRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Industry,Gender,Location,Salary,YearsExperience,EducationalLevel,JobTitle,CompanyName,StartDate,EmploymentStatus")] SalaryDisparityRecord salaryDisparityRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salaryDisparityRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(salaryDisparityRecord);
        }

        // GET: SalaryDisparityRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salaryDisparityRecord = await _context.SalaryDisparityRecords.FindAsync(id);
            if (salaryDisparityRecord == null)
            {
                return NotFound();
            }
            return View(salaryDisparityRecord);
        }

        // POST: SalaryDisparityRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Industry,Gender,Location,Salary,YearsExperience,EducationalLevel,JobTitle,CompanyName,StartDate,EmploymentStatus")] SalaryDisparityRecord salaryDisparityRecord)
        {
            if (id != salaryDisparityRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salaryDisparityRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalaryDisparityRecordExists(salaryDisparityRecord.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(salaryDisparityRecord);
        }

        // GET: SalaryDisparityRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salaryDisparityRecord = await _context.SalaryDisparityRecords
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salaryDisparityRecord == null)
            {
                return NotFound();
            }

            return View(salaryDisparityRecord);
        }

        // POST: SalaryDisparityRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salaryDisparityRecord = await _context.SalaryDisparityRecords.FindAsync(id);
            if (salaryDisparityRecord != null)
            {
                _context.SalaryDisparityRecords.Remove(salaryDisparityRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SalaryDisparityRecordExists(int id)
        {
            return _context.SalaryDisparityRecords.Any(e => e.Id == id);
        }
    }
}
