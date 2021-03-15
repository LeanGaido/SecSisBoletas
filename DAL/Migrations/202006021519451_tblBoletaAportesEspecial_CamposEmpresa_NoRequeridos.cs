namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletaAportesEspecial_CamposEmpresa_NoRequeridos : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblBoletasAportesEspeciales", "Cuit", c => c.String());
            AlterColumn("dbo.tblBoletasAportesEspeciales", "RazonSocial", c => c.String());
            AlterColumn("dbo.tblBoletasAportesEspeciales", "NombreFantasia", c => c.String());
            AlterColumn("dbo.tblBoletasAportesEspeciales", "Calle", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblBoletasAportesEspeciales", "Calle", c => c.String(nullable: false));
            AlterColumn("dbo.tblBoletasAportesEspeciales", "NombreFantasia", c => c.String(nullable: false));
            AlterColumn("dbo.tblBoletasAportesEspeciales", "RazonSocial", c => c.String(nullable: false));
            AlterColumn("dbo.tblBoletasAportesEspeciales", "Cuit", c => c.String(nullable: false));
        }
    }
}
