namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DecimalToDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblSueldosBasicos", "Monto", c => c.Double(nullable: false));
            AlterColumn("dbo.tblBoletaAportes", "TotalSueldos", c => c.Double(nullable: false));
            AlterColumn("dbo.tblBoletaAportes", "RecargoMora", c => c.Double());
            AlterColumn("dbo.tblDetallesDeclaracionJurada", "Sueldo", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblDetallesDeclaracionJurada", "Sueldo", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblBoletaAportes", "RecargoMora", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.tblBoletaAportes", "TotalSueldos", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tblSueldosBasicos", "Monto", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
