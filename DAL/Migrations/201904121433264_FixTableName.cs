namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixTableName : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FechaVencimiento", newName: "tblFechasVencimientos");
            RenameTable(name: "dbo.Reclamo", newName: "tblReclamos");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.tblReclamos", newName: "Reclamo");
            RenameTable(name: "dbo.tblFechasVencimientos", newName: "FechaVencimiento");
        }
    }
}
