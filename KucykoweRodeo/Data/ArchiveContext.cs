using System;
using System.Collections.Generic;
using System.Linq;
using KucykoweRodeo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KucykoweRodeo.Data
{
    public class ArchiveContext : IdentityDbContext<IdentityUser>
    {
        public ArchiveContext(DbContextOptions<ArchiveContext> options) : base(options)
        {
        }

        public DbSet<Magazine> Magazines { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Magazine>().ToTable("Magazine");
            modelBuilder.Entity<Issue>().ToTable("Issue");
            modelBuilder.Entity<Article>().ToTable("Article");
            modelBuilder.Entity<Author>().ToTable("Author");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Tag>().ToTable("Tag");

            base.OnModelCreating(modelBuilder);
        }

        public (List<Author>, List<Author>) GetAuthors(string query)
        {
            var names = query.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var comparableNames = names
                .Select(name => name.ToLower())
                .ToList();

            var knowns = Authors
                .AsQueryable()
                .Where(author => comparableNames
                    .Contains(author.ComparableName))
                .ToList();

            var unknowns = names
                .Where(name => knowns.All(author => author.ComparableName != name.ToLower()))
                .Select(author => new Author
                {
                    Name = author,
                    ComparableName = author.ToLower()
                })
                .ToList();

            return (knowns, unknowns);
        }
    }
}
