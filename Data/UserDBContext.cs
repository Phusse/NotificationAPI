
using Microsoft.EntityFrameworkCore;

namespace NotificationAPI;

public class UserDBContext : DbContext
{
    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<RegisterModel> RegisterModels { get; set; }
    public UserDBContext(DbContextOptions<UserDBContext> options) : base(options) 
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}
