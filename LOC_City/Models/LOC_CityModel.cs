using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MVC_Project.Areas.LOC_City.Models
{
    public class LOC_CityModel
    {
        public int? CityID { get; set; }

        [Required(ErrorMessage = "City Name is Required")]
        [DisplayName("State Name")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "City Code is Required")]
        [DisplayName("State Code")]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        public string Citycode { get; set; }

        public int? StateID { get; set; }

        [Required(ErrorMessage = "State Name is Required")]
        [DisplayName("State Name")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "Country Name is Required")]
        [DisplayName("Country Name")]
        public string? CountryName { get; set; }

        public int CountryID { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime Modified { get; set; }

        public bool IsSelected { get; set; }

	}
    public class LOC_CityDropDownModel
    {
        public int CityID { get; set; }
        public string? CityName { get; set; }
    }


}
