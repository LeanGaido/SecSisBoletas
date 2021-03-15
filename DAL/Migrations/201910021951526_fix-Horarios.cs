namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixHorarios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblDetallesDeclaracionJurada", "SueldoBase", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tblHorariosEmpleados", "Turno", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblHorariosEmpleados", "Turno");
            DropColumn("dbo.tblDetallesDeclaracionJurada", "SueldoBase");
        }
    }
}
