using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HoliPics.Models;

namespace HoliPics.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<HoliPics.Models.Album> Albums { get; set; } = default!;
        public DbSet<HoliPics.Models.Img> Images { get; set; } = default!;
    }
}
