/**
 * The goal of this class is to register the Models that you
 * wnat to persist (save to the db.
 * 
 * It will be instantiated by the DI (Dependency Injection) of this
 * projet (Backend.DAL) using this syntaxe : 
 * build.Services.AddDbContext<AppDbContext>(...)
 * It basically says that when asked an AppDbContexte, it should
 * give the DbContext herited class (AppDbContext) instantiated
 * with the given options and all the models.
 * 
 * The seconde purpose of registering the models here is that the
 * migration process checks the listed models and creates them in the
 * database once updated.
 * 
 * There is also a part for the constraints as you can for instance
 * have a foreign on an Entity that you want to find back in the db.
 * 
 * Meant to be used only in the repositories.
 */

//
//  Imports
//

using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.DAL;

//
//  Class
//

public class AppDbContext(
    DbContextOptions<AppDbContext> options
) : DbContext(options) {
    //
    //  Entities registry
    //

    // public DbSet<MyExampleModel> MyExampleModels => Set<MyExampleModel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdMember> HouseholdMembers => Set<HouseholdMember>();

    // YOURS

    //
    //  Constraints registry
    //

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<MyExampleModel>(model => {
        //     model.HasOne(model => model.myExampleCategory)
        //         .WithMany(category => category.myExampleModel)
        //         .HasForeignKey(model => model.myExampleCategoryId)
        //         .OnDelete(DeleteBehavior.Restrict); <- Could be DeleteBehavior.Cascade too
        // });

        modelBuilder.Entity<User>(user => {
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Username).HasMaxLength(64);
            user.Property(u => u.Email).HasMaxLength(256);
            user.Property(u => u.PasswordHash).HasMaxLength(512);
            user.Property(u => u.Role).HasMaxLength(32);
        });

        modelBuilder.Entity<Household>(household => {
            household.Property(h => h.Name).HasMaxLength(100).IsRequired();
            household.Property(h => h.InvitationCode).HasMaxLength(16);
            household.HasIndex(h => h.InvitationCode).IsUnique();
        });

        // Configure the relationship from the CLR Household entity so EF Core
        // binds Members to the real ICollection<HouseholdMember> navigation.
        modelBuilder.Entity<Household>(household => {
            household.HasMany(h => h.Members)
                .WithOne(m => m.Household)
                .HasForeignKey(m => m.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HouseholdMember>(member => {
            member.HasKey(m => new { m.HouseholdId, m.UserId });
            member.Property(m => m.Role).HasConversion<string>().HasMaxLength(16).IsRequired();
            member.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserToken>(userToken => {
            userToken.HasOne<User>()
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            userToken.HasIndex(ut => ut.Jti).IsUnique();
            userToken.HasIndex(ut => ut.ExpiresAt);
        });
    }
}