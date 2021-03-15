namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class totalPagadoBoleta : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletaAportes", "TotalPagado", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblBoletaAportes", "TotalPagado");
        }
    }
}
