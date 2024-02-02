namespace mainykdovanok.Models
{
    public class VideoModel
    {
        public int Id { get; set; }
        public IFormFile File { get; set; }
        public int Item { get; set; }
        public int User { get; set; }
    }
}
