namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechaVencimiento : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblFechasVencimientos", "mesBoleta", c => c.Int(nullable: false));
            AddColumn("dbo.tblFechasVencimientos", "anioBoleta", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblFechasVencimientos", "anioBoleta");
            DropColumn("dbo.tblFechasVencimientos", "mesBoleta");
        }
    }
}
