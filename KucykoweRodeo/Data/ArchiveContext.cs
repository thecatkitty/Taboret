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

        private static (List<T>, List<T>) GetFeatures<T>(IEnumerable<T> feature, string query) where T : Feature, new()
        {
            var names = query.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var comparableNames = names
                .Select(name => name.ToLower())
                .ToList();

            var knowns = feature
                .AsQueryable()
                .Where(author => comparableNames
                    .Contains(author.ComparableName))
                .ToList();

            var unknowns = names
                .Where(name => knowns.All(author => author.ComparableName != name.ToLower()))
                .Select(author => new T
                {
                    Name = author,
                    ComparableName = author.ToLower()
                })
                .ToList();

            return (knowns, unknowns);
        }

        public (List<Author>, List<Author>) GetAuthors(string query) => GetFeatures(Authors, query);

        public (List<Tag>, List<Tag>) GetTags(string query) => GetFeatures(Tags, query);

        private static List<T> SuggestFeatures<T>(IEnumerable<T> feature, string query) where T : Feature, new()
        {
            query = (query ?? "").ToLower();

            var rawTags = query.Split(',', StringSplitOptions.TrimEntries);
            var term = rawTags.Last();

            IQueryable<T> authors = feature
                .AsQueryable()
                .OrderByDescending(tag => tag.Articles.Count);

            if (rawTags.Length > 1)
            {
                var excluded = rawTags[..^1];
                authors = authors.Where(author => !excluded.Contains(author.ComparableName));
            }

            if (term.Length != 0)
            {
                authors = authors.Where(author => author.ComparableName.Contains(rawTags.Last()));
            }

            return authors.ToList();
        }

        public List<Author> SuggestAuthors(string query) => SuggestFeatures(Authors, query);

        public List<Tag> SuggestTags(string query) => SuggestFeatures(Tags, query);
    }
}
