using Microsoft.EntityFrameworkCore;
using SpeechAndFaceRecognizerWebCore.Data.Entities;

namespace SpeechAndFaceRecognizerWebCore.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<User>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<User>()
                .HasOne(e => e.MicrosoftSpeekerIdentificationProfile)
                .WithOne(e => e.User)
                .HasForeignKey<MicrosoftSpeekerIdentificationProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasOne(e => e.MicrosoftFaceIdentificationPerson)
                .WithOne(e => e.User)
                .HasForeignKey<MicrosoftFaceIdentificationPerson>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MicrosoftSpeekerIdentificationProfile>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<MicrosoftSpeekerIdentificationProfile>()
                .HasOne(e => e.User)
                .WithOne(e => e.MicrosoftSpeekerIdentificationProfile)
                .HasForeignKey<MicrosoftSpeekerIdentificationProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MicrosoftFaceIdentificationPersonGroup>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<MicrosoftFaceIdentificationPersonGroup>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<MicrosoftFaceIdentificationPersonGroup>()
                .HasMany(e=>e.Persons)
                .WithOne(e=>e.PersonGroup)
                .HasForeignKey(e=>e.PersonGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MicrosoftFaceIdentificationPerson>()
                .HasOne(e => e.PersonGroup)
                .WithMany(e => e.Persons)
                .HasForeignKey(e => e.PersonGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MicrosoftFaceIdentificationPerson>()
                .HasOne(e => e.User)
                .WithOne(e => e.MicrosoftFaceIdentificationPerson)
                .HasForeignKey<MicrosoftFaceIdentificationPerson>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MicrosoftFaceIdentificationPersonFace>()
                .HasOne(e => e.Person)
                .WithMany(e => e.Faces)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        }


        public DbSet<User> Users { get; set; }

        public DbSet<MicrosoftSpeekerIdentificationProfile> MicrosoftSpeekerIdentificationProfiles { get; set; }

        public DbSet<MicrosoftFaceIdentificationPersonGroup> MicrosoftFaceIdentificationPersonGroups { get; set; }

        public DbSet<MicrosoftFaceIdentificationPerson> MicrosoftFaceIdentificationPersons { get; set; }

        public DbSet<MicrosoftFaceIdentificationPersonFace> MicrosoftFaceIdentificationPersonFaces { get; set; }
    }
}
