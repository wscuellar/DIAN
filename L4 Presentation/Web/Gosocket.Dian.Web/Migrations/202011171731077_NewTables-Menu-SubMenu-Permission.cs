namespace Gosocket.Dian.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewTablesMenuSubMenuPermission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Active", c => c.Byte(nullable: true, defaultValue: 0));

            CreateTable(
                "dbo.Menu",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    Description = c.String(nullable: true, maxLength: 100),
                    Title = c.String(nullable: true, maxLength: 100),
                })
                .PrimaryKey(t => new { t.Id })
                .Index(t => t.Id, unique: true, name: "MenuNameUniqueIndex");

            CreateTable(
                "dbo.SubMenu",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MenuId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 100),
                    Description = c.String(nullable: true, maxLength: 100),
                    Title = c.String(nullable: true, maxLength: 100),
                })
                .PrimaryKey(t => new { t.Id })
                .ForeignKey("dbo.Menu", f => f.MenuId, cascadeDelete: false)
                .Index(t => t.Id, unique: true, name: "SubMenuNameUniqueIndex");

            CreateTable(
                "dbo.Permission",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128),
                    SubMenuId = c.Int(nullable: false),
                    State = c.String(nullable: true, maxLength: 50),
                    CreatedBy = c.String(),
                    UpdatedBy = c.String(),
                })
                .PrimaryKey(t => new { t.Id })
                .ForeignKey("dbo.AspNetUsers", f => f.UserId, cascadeDelete: false)
                .ForeignKey("dbo.SubMenu", f => f.SubMenuId, cascadeDelete: false);

        }
        
        public override void Down()
        {
            DropTable("dbo.Permission");
            DropTable("dbo.SubMenu");
            DropTable("dbo.Menu");
            DropColumn("dbo.AspNetUsers", "Active");
        }
    }
}
