namespace ViperAppApi.Models.Domains
{
    public class Friend
    {
        public int Id { get; set; }
        public long UserId { get; set; }     
        public long FriendId { get; set; }    // Changed from int to long
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User FriendUser { get; set; } = null!;
    }
}