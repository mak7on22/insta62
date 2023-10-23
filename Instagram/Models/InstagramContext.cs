using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Models
{
    public class InstagramContext: IdentityDbContext<User, IdentityRole<int>,int>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Coment> Coments { get; set; }
        public DbSet<SubscribesAndSubscriptions> SubscribesAndSubscriptions { get; set; }
        public DbSet<Like> Likes { get; set; }
        public InstagramContext(DbContextOptions<InstagramContext> options) : base(options) { }
    }
}
