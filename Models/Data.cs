using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


namespace Salary_Disparity_Analysis.Models
{
    public class MockData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       public int Id {get; set; }
      public string email { get; set; }
        public string ip_address { get; set; }
        public string industry {  get; set; }
        public string gender { get;set; }
        public string location { get; set; }
        public string years_experience {  get; set; }
        public string job_tittle { get; set; }
    }
}
