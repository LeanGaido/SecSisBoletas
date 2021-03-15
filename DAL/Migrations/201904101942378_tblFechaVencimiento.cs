namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblFechaVencimiento : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FechaVencimiento",
                c => new
                    {
                        IdFechaVencimiento = c.Int(nullable: false, identity: true),
                        FechaVto = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdFechaVencimiento);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FechaVencimiento");
        }
    }
}
