namespace mainykdovanok.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public int DeviceId { get; set; }
        public DateTime PostedDateTime { get; set; }
    }
}
