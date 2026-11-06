using System.ComponentModel.DataAnnotations;

namespace ImportedDatabaseViewer.Models
{
    public class Client
    {
        public required int CardCode { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public string? SurName { get; set; }
        public required string PhoneMobile { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? GenderId { get; set; }
        [DataType(DataType.Date)] public required DateTime Birthday { get; set; }
        public string? City { get; set; }
        public int? Pincode { get; set; }
        public int? Bonus { get; set; }
        public int? Turnover { get; set; }
    }
}
