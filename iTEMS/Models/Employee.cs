using System.ComponentModel.DataAnnotations;

namespace iTEMS.Models
{
    public class Employee : UserActivity
    {
        [Display(Name = "ID")]
        public int Id { get; set; }
        [Display(Name = "Employee Number")]
        public string? EmpNo { get; set; }
        [Display(Name = "Username")]
        public string? UserName { get; set; }
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "Area")]
        public string? AreaByState { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateofBirth { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }
        [Display(Name = "Department")]
        public string? Department{ get; set; }
        [Display(Name = "Designation")]
        public string? Designation { get; set; }
        [Display(Name = "Role ID")]
        public string? RoleId { get; set; }

        public List<string> Countries { get; set; }
        public List<string> States { get; set; }
        public List <string> Areas { get; set; }

        // Constructor to initialize the list of countries
        public Employee()
        {


            Areas = new List<string>
            {
                "Johor Bahru", "Batu Pahat", "Kluang", "Muar", "Segamat",
                "Alor Setar", "Kulim", "Sungai Petani", "Langkawi", "Baling",
                "Kota Bharu", "Pangkalan Chepa", "Tumpat", "Kuala Krai", "Gua Musang",
                "Melaka City", "Ayer Keroh", "Alor Gajah", "Jasin", "Masjid Tanah",
                "Seremban", "Port Dickson", "Nilai", "Bahau", "Tampin",
                "Kuantan", "Temerloh", "Jerantut", "Bentong", "Raub",
                "Ipoh", "Taiping", "Sitiawan", "Teluk Intan", "Kampar", "Batu Gajah",
                "Kangar", "Arau", "Kuala Perlis",
                "George Town", "Butterworth", "Seberang Perai", "Bukit Mertajam", "Gelugor",
                "Kota Kinabalu", "Sandakan", "Tawau", "Lahad Datu", "Keningau",
                "Kuching", "Sibu", "Miri", "Bintulu", "Limbang",
                "Shah Alam", "Petaling Jaya", "Subang Jaya", "Klang", "Seri Kembangan",
                "Kuala Terengganu", "Chukai", "Dungun", "Kuala Berang", "Marang",
                "Default"
            };

            States = new List<string>
            {
                "Johor",
                "Kedah",
                "Kelantan",
                "Melaka",
                "Negeri Sembilan",
                "Pahang",
                "Perak",
                "Perlis",
                "Penang",
                "Sabah",
                "Sarawak",
                "Selangor",
                "Terengganu",
                "Default"
            };

            // Initialize the list of countries
            Countries = new List<string>
            {
                "Afghanistan",
                "Albania",
                "Algeria",
                "Andorra",
                "Angola",
                "Antigua and Barbuda",
                "Argentina",
                "Armenia",
                "Australia",
                "Austria",
                "Azerbaijan",
                "Bahamas",
                "Bahrain",
                "Bangladesh",
                "Barbados",
                "Belarus",
                "Belgium",
                "Belize",
                "Benin",
                "Bhutan",
                "Bolivia",
                "Bosnia and Herzegovina",
                "Botswana",
                "Brazil",
                "Brunei",
                "Bulgaria",
                "Burkina Faso",
                "Burundi",
                "Côte d'Ivoire",
                "Cabo Verde",
                "Cambodia",
                "Cameroon",
                "Canada",
                "Central African Republic",
                "Chad",
                "Chile",
                "China",
                "Colombia",
                "Comoros",
                "Congo (Congo-Brazzaville)",
                "Costa Rica",
                "Croatia",
                "Cuba",
                "Cyprus",
                "Czechia (Czech Republic)",
                "Democratic Republic of the Congo",
                "Denmark",
                "Djibouti",
                "Dominica",
                "Dominican Republic",
                "East Timor (Timor-Leste)",
                "Ecuador",
                "Egypt",
                "El Salvador",
                "Equatorial Guinea",
                "Eritrea",
                "Estonia",
                "Eswatini (fmr. Swaziland)",
                "Ethiopia",
                "Fiji",
                "Finland",
                "France",
                "Gabon",
                "Gambia",
                "Georgia",
                "Germany",
                "Ghana",
                "Greece",
                "Grenada",
                "Guatemala",
                "Guinea",
                "Guinea-Bissau",
                "Guyana",
                "Haiti",
                "Holy See",
                "Honduras",
                "Hungary",
                "Iceland",
                "India",
                "Indonesia",
                "Iran",
                "Iraq",
                "Ireland",
                "Israel",
                "Italy",
                "Jamaica",
                "Japan",
                "Jordan",
                "Kazakhstan",
                "Kenya",
                "Kiribati",
                "Kuwait",
                "Kyrgyzstan",
                "Laos",
                "Latvia",
                "Lebanon",
                "Lesotho",
                "Liberia",
                "Libya",
                "Liechtenstein",
                "Lithuania",
                "Luxembourg",
                "Madagascar",
                "Malawi",
                "Malaysia",
                "Maldives",
                "Mali",
                "Malta",
                "Marshall Islands",
                "Mauritania",
                "Mauritius",
                "Mexico",
                "Micronesia",
                "Moldova",
                "Monaco",
                "Mongolia",
                "Montenegro",
                "Morocco",
                "Mozambique",
                "Myanmar (formerly Burma)",
                "Namibia",
                "Nauru",
                "Nepal",
                "Netherlands",
                "New Zealand",
                "Nicaragua",
                "Niger",
                "Nigeria",
                "North Korea",
                "North Macedonia",
                "Norway",
                "Oman",
                "Pakistan",
                "Palau",
                "Palestine State",
                "Panama",
                "Papua New Guinea",
                "Paraguay",
                "Peru",
                "Philippines",
                "Poland",
                "Portugal",
                "Qatar",
                "Romania",
                "Russia",
                "Rwanda",
                "Saint Kitts and Nevis",
                "Saint Lucia",
                "Saint Vincent and the Grenadines",
                "Samoa",
                "San Marino",
                "Sao Tome and Principe",
                "Saudi Arabia",
                "Senegal",
                "Serbia",
                "Seychelles",
                "Sierra Leone",
                "Singapore",
                "Slovakia",
                "Slovenia",
                "Solomon Islands",
                "Somalia",
                "South Africa",
                "South Korea",
                "South Sudan",
                "Spain",
                "Sri Lanka",
                "Sudan",
                "Suriname",
                "Sweden",
                "Switzerland",
                "Syria",
                "Tajikistan",
                "Tanzania",
                "Thailand",
                "Togo",
                "Tonga",
                "Trinidad and Tobago",
                "Tunisia",
                "Turkey",
                "Turkmenistan",
                "Tuvalu",
                "Uganda",
                "Ukraine",
                "United Arab Emirates",
                "United Kingdom",
                "United States of America",
                "Uruguay",
                "Uzbekistan",
                "Vanuatu",
                "Venezuela",
                "Vietnam",
                "Yemen",
                "Zambia",
                "Zimbabwe"
            };

        }
    }
}
