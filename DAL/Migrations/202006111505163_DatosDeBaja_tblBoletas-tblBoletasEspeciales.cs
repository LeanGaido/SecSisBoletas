namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatosDeBaja_tblBoletastblBoletasEspeciales : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletaAportes", "DeBaja", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblBoletaAportes", "FechaBaja", c => c.DateTime());
            AddColumn("dbo.tblBoletaAportes", "UserId", c => c.String());
            AddColumn("dbo.tblBoletasAportesEspeciales", "DeBaja", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblBoletasAportesEspeciales", "FechaBaja", c => c.DateTime());
            AddColumn("dbo.tblBoletasAportesEspeciales", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblBoletasAportesEspeciales", "UserId");
            DropColumn("dbo.tblBoletasAportesEspeciales", "FechaBaja");
            DropColumn("dbo.tblBoletasAportesEspeciales", "DeBaja");
            DropColumn("dbo.tblBoletaAportes", "UserId");
            DropColumn("dbo.tblBoletaAportes", "FechaBaja");
            DropColumn("dbo.tblBoletaAportes", "DeBaja");
        }
    }
}
