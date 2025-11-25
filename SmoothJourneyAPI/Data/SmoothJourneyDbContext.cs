using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmoothJourneyAPI.Models;

namespace SmoothJourneyAPI.Data
{
    public class SmoothJourneyDbContext : DbContext
    {
        public SmoothJourneyDbContext (DbContextOptions<SmoothJourneyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<EmailVerification> EmailVerifications { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Users>().HasIndex(u => u.Email).IsUnique();
            base.OnModelCreating(builder);
        }
    }
}
