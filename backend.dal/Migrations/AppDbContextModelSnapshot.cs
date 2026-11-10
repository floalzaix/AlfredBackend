using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Backend.DAL.Migrations;

[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot : ModelSnapshot {
    protected override void BuildModel(ModelBuilder modelBuilder) {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Backend.Models.Household", b => {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<string>("InvitationCode").IsRequired().HasMaxLength(16).HasColumnType("character varying(16)");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.HasKey("Id");
            b.HasIndex("InvitationCode").IsUnique().HasDatabaseName("UX_Households_InvitationCode");
            b.ToTable("Households");
        });
        modelBuilder.Entity("Backend.Models.HouseholdMember", b => {
            b.Property<Guid>("HouseholdId").HasColumnType("uuid");
            b.Property<string>("UserId").HasMaxLength(128).HasColumnType("character varying(128)");
            b.HasKey("HouseholdId", "UserId");
            b.ToTable("HouseholdMembers");
        });
        modelBuilder.Entity("Backend.Models.HouseholdMember", b => {
            b.HasOne("Backend.Models.Household", "Household").WithMany()
                .HasForeignKey("HouseholdId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.Navigation("Household");
        });
    }
}
