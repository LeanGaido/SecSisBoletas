namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletasEspeciales_FechaVencimiento : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletasAportesEspeciales", "FechaVencimiento", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblBoletasAportesEspeciales", "FechaVencimiento");
        }
    }
}
