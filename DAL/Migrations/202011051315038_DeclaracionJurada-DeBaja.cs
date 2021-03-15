namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeclaracionJuradaDeBaja : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblDeclaracionesJuradas", "DeBaja", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblDeclaracionesJuradas", "FechaBaja", c => c.DateTime());
            AddColumn("dbo.tblDeclaracionesJuradas", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblDeclaracionesJuradas", "UserId");
            DropColumn("dbo.tblDeclaracionesJuradas", "FechaBaja");
            DropColumn("dbo.tblDeclaracionesJuradas", "DeBaja");
        }
    }
}
