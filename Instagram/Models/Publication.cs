using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Instagram.Models
{
    public class Publication
    {
        public int Id { get; set; }
        [StringLength(900)]
        [Display(Name = "Добавить коментарий")]
        public string? Description { get; set; }
        public int? LikeCount { get; set; }
        public int? ComentsCount { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime? DateOfAdd { get; set; }
        [Display(Name = "Изображение")]
        public string? Pictures { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Coment> Coments { get; set; }

        public Publication()
        {
            LikeCount = 0;
            ComentsCount = 0;
            DateOfAdd = DateTime.Now;

            Likes = new List<Like>();
            Coments = new List<Coment>();
        }
    }
}
