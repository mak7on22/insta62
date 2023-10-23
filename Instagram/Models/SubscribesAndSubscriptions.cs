namespace Instagram.Models
{
    public class SubscribesAndSubscriptions
    {
        public int Id { get; set; }
        public int SubscriberId { get; set;}
        public User Subscriber { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
    }   
}
