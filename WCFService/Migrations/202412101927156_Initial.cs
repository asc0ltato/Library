namespace WCFService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Author",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookAuthors",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        AuthorId = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.AuthorId })
                .ForeignKey("dbo.Author", t => t.AuthorId, cascadeDelete: true)
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.Book",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Year = c.Int(nullable: false),
                        Image = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookGenres",
                c => new
                    {
                        BookId = c.Int(nullable: false),
                        GenreId = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookId, t.GenreId })
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Genre", t => t.GenreId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.GenreId);
            
            CreateTable(
                "dbo.Genre",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Review",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(maxLength: 255),
                        Rating = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false, storeType: "smalldatetime"),
                        BookId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        Login = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        PasswordHash = c.String(maxLength: 255),
                        ProfilePhotoPath = c.String(),
                        DateCreate = c.DateTime(nullable: false, storeType: "smalldatetime"),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Role", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Listgetbooks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UsersId = c.Int(nullable: false),
                        SampleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sample", t => t.SampleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UsersId, cascadeDelete: true)
                .Index(t => t.UsersId)
                .Index(t => t.SampleId);
            
            CreateTable(
                "dbo.Sample",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 255),
                        Presence = c.Boolean(nullable: false),
                        BookId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Book", t => t.BookId, cascadeDelete: true)
                .Index(t => t.BookId);
            
            CreateTable(
                "dbo.Role",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookAuthors", "BookId", "dbo.Book");
            DropForeignKey("dbo.Review", "UserId", "dbo.Users");
            DropForeignKey("dbo.Users", "RoleId", "dbo.Role");
            DropForeignKey("dbo.Listgetbooks", "UsersId", "dbo.Users");
            DropForeignKey("dbo.Listgetbooks", "SampleId", "dbo.Sample");
            DropForeignKey("dbo.Sample", "BookId", "dbo.Book");
            DropForeignKey("dbo.Review", "BookId", "dbo.Book");
            DropForeignKey("dbo.BookGenres", "GenreId", "dbo.Genre");
            DropForeignKey("dbo.BookGenres", "BookId", "dbo.Book");
            DropForeignKey("dbo.BookAuthors", "AuthorId", "dbo.Author");
            DropIndex("dbo.Sample", new[] { "BookId" });
            DropIndex("dbo.Listgetbooks", new[] { "SampleId" });
            DropIndex("dbo.Listgetbooks", new[] { "UsersId" });
            DropIndex("dbo.Users", new[] { "RoleId" });
            DropIndex("dbo.Review", new[] { "UserId" });
            DropIndex("dbo.Review", new[] { "BookId" });
            DropIndex("dbo.BookGenres", new[] { "GenreId" });
            DropIndex("dbo.BookGenres", new[] { "BookId" });
            DropIndex("dbo.BookAuthors", new[] { "AuthorId" });
            DropIndex("dbo.BookAuthors", new[] { "BookId" });
            DropTable("dbo.Role");
            DropTable("dbo.Sample");
            DropTable("dbo.Listgetbooks");
            DropTable("dbo.Users");
            DropTable("dbo.Review");
            DropTable("dbo.Genre");
            DropTable("dbo.BookGenres");
            DropTable("dbo.Book");
            DropTable("dbo.BookAuthors");
            DropTable("dbo.Author");
        }
    }
}
