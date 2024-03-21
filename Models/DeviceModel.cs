namespace mainykdovanok.Models
{
    public class DeviceModel
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public int Status { get; set; }

        public int User { get; set; }

        public int Category { get; set; }

        public int Type { get; set; }
        public List<IFormFile> Images { get; set; }

        public List<string> Questions { get; set; }

        public DateTime EndDate { get; set; }
    }
}
