namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class totalSueldos2Y5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletaAportes", "TotalSueldos2", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tblBoletaAportes", "TotalSueldos5", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.tblBoletaAportes", "TotalSueldos");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblBoletaAportes", "TotalSueldos", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.tblBoletaAportes", "TotalSueldos5");
            DropColumn("dbo.tblBoletaAportes", "TotalSueldos2");
        }
    }
}
