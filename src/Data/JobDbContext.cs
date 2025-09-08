using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VideoProcessingApi.Data.Entities;
using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Data;

public class JobDbContext : DbContext
{
    public DbSet<VideoJob> Jobs { get; set; }
    public DbSet<ProcessingOperation> Operations { get; set; }
    
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InputFilePaths)
                  .HasConversion(
                      v => string.Join(';', v),
                      v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Options)
                  .HasConversion(
                      v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
                      v => v != null ? JsonSerializer.Deserialize<ProcessingOptions>(v, (JsonSerializerOptions?)null) : null);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        modelBuilder.Entity<ProcessingOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Job)
                  .WithMany(e => e.Operations)
                  .HasForeignKey(e => e.JobId);
        });
    }
}
