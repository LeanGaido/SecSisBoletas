namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletas_cantEmpleados_Aportes2_cantAfiliados_aportes5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletaAportes", "CantEmpleados", c => c.Int(nullable: false));
            AddColumn("dbo.tblBoletaAportes", "Aportes2", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tblBoletaAportes", "CantAfiliados", c => c.Int(nullable: false));
            AddColumn("dbo.tblBoletaAportes", "Aportes5", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblBoletaAportes", "Aportes5");
            DropColumn("dbo.tblBoletaAportes", "CantAfiliados");
            DropColumn("dbo.tblBoletaAportes", "Aportes2");
            DropColumn("dbo.tblBoletaAportes", "CantEmpleados");
        }
    }
}
