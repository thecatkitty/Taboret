using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KucykoweRodeo.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace KucykoweRodeo.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ArchiveContext context)
        {
            context.Database.EnsureCreated();

            if (context.Magazines.Any())
            {
                return;
            }


            Console.Write("Dodawanie magazynów");
            var magazines = new[]
            {
                new Magazine {Name = "Brohoof", Signature = "BH"},
                new Magazine {Name = "Comichoof", Signature = "CH"},
                new Magazine {Name = "MANEzette", Signature = "MZ"},
                new Magazine {Name = "MANEzette Komiks", Signature = "MK"},
                new Magazine {Name = "Equestria Times", Signature = "ET"},
                new Magazine {Name = "Equinox Times", Signature = "EQ"}
            };

            foreach (var magazine in magazines)
            {
                context.Magazines.Add(magazine);
                Console.Write('.');
            }
            context.SaveChanges();
            Console.WriteLine();


            using var fs = new FileStream("Data/dane.xlsx", FileMode.Open, FileAccess.Read);
            var wb = new XSSFWorkbook(fs);

            Console.Write("Dodawanie wydań");
            var sheet = wb.GetSheet("issues");
            for (var i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                context.Issues.Add(GetIssueFromRow(row, context));
                Console.Write('.');
            }
            context.SaveChanges();
            Console.WriteLine();


            Console.Write("Dodawanie artykułów");
            sheet = wb.GetSheet("articles");
            for (var i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                context.Articles.Add(GetArticleFromRow(row, context));
                Console.Write('.');
            }
            context.SaveChanges();
            Console.WriteLine();
        }

        private static Issue GetIssueFromRow(IRow row, ArchiveContext context)
        {
            var issue = new Issue
            {
                Signature = row.GetCell(0).StringCellValue,
                PublicationDate = row.GetCell(1).DateCellValue,
                MagazineSignature = row.GetCell(0).StringCellValue.Substring(0, 2),
                CoverSignature = row.GetCell(3).StringCellValue,
                Url = row.GetCell(4).StringCellValue,
                CoverAuthors = new HashSet<Author>()
            };

            if (row.GetCell(5)?.CellType == CellType.String)
            {
                foreach (var author in GetAuthorsFromCell(row.GetCell(5), context))
                {
                    issue.CoverAuthors.Add(author);
                }
            }

            if (row.GetCell(6)?.CellType == CellType.Numeric && row.GetCell(6).NumericCellValue > 0)
            {
                issue.PageCount = (uint) row.GetCell(6).NumericCellValue;
            }

            if (row.GetCell(7)?.CellType == CellType.Numeric)
            {
                issue.IsArchived = row.GetCell(7).NumericCellValue > 0;
            }

            if (row.GetCell(8) != null)
            {
                issue.UpdateTime = row.GetCell(8).DateCellValue;
            }

            return issue;
        }

        private static Article GetArticleFromRow(IRow row, ArchiveContext context)
        {
            var title = row.GetCell(0).ToString();
            var issueSignature = row.GetCell(2).ToString();
            var ordinalNumber = (uint)row.GetCell(3).NumericCellValue;
            var page = (uint) row.GetCell(4).NumericCellValue;
            var wordCount = row.GetCell(7) == null ? 0 : (uint) row.GetCell(7).NumericCellValue;
            var lead = row.GetCell(8)?.StringCellValue;

            var article = new Article
            {
                Title = title,
                IssueSignature = issueSignature,
                OrdinalNumber = ordinalNumber,
                Page = page,
                WordCount = wordCount,
                Lead = lead
            };

            article.Authors ??= new HashSet<Author>();

            foreach (var author in GetAuthorsFromCell(row.GetCell(1), context))
            {
                article.Authors.Add(author);
            }

            var category = row.GetCell(5).ToString();
            var comparableName = category?.ToLower();
            if (!context.Categories.Any(c => c.ComparableName == comparableName))
            {
                context.Categories.Add(new Category
                {
                    Name = category,
                    ComparableName = comparableName
                });
                context.SaveChanges();
            }
            article.Category = context.Categories
                .First(c => c.ComparableName == comparableName);

            article.Tags ??= new HashSet<Tag>();

            if (row.GetCell(6)?.CellType == CellType.String)
            {
                foreach (var tag in GetTagsFromCell(row.GetCell(6), context))
                {
                    article.Tags.Add(tag);
                }
            }

            return article;
        }

        private static IEnumerable<Author> GetAuthorsFromCell(ICell cell, ArchiveContext context)
        {
            var authors = new HashSet<Author>();
            if (cell is not { CellType: CellType.String })
            {
                return authors;
            }

            foreach (var name in cell.StringCellValue.Split(", "))
            {
                var comparableName = name.ToLower();
                if (!context.Authors.Any(a => a.ComparableName == comparableName))
                {
                    context.Authors.Add(new Author
                    {
                        Name = name,
                        ComparableName = comparableName
                    });
                    context.SaveChanges();
                }
                authors.Add(context.Authors
                    .First(a => a.ComparableName == comparableName));
            }

            return authors;
        }

        private static IEnumerable<Tag> GetTagsFromCell(ICell cell, ArchiveContext context)
        {
            var tags = new HashSet<Tag>();
            if (cell is not { CellType: CellType.String })
            {
                return tags;
            }

            foreach (var name in cell.StringCellValue.Split(", "))
            {
                var comparableName = name.ToLower();
                if (!context.Tags.Any(a => a.ComparableName == comparableName))
                {
                    context.Tags.Add(new Tag
                    {
                        Name = name,
                        ComparableName = comparableName
                    });
                    context.SaveChanges();
                }
                tags.Add(context.Tags
                    .First(a => a.ComparableName == comparableName));
            }

            return tags;
        }
    }
}
