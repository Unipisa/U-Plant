namespace UPlant.Models
{
    public class UserFromSAML2 : UserFromWSo2
    {
        public string oin { get; set; }
    }
    public class UserFromWSo2
    {
        public string sub { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string fiscalNumber { get; set; }
        public string email { get; set; }
    }
}
