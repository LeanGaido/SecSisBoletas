namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletasEspeciales_Periodo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletasAportesEspeciales", "Periodo", c => c.String(nullable: false));
            DropColumn("dbo.tblBoletasAportesEspeciales", "MesDesdeBoleta");
            DropColumn("dbo.tblBoletasAportesEspeciales", "AnioDesdeBoleta");
            DropColumn("dbo.tblBoletasAportesEspeciales", "MesHastaBoleta");
            DropColumn("dbo.tblBoletasAportesEspeciales", "AnioHastaBoleta");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblBoletasAportesEspeciales", "AnioHastaBoleta", c => c.Int(nullable: false));
            AddColumn("dbo.tblBoletasAportesEspeciales", "MesHastaBoleta", c => c.Int(nullable: false));
            AddColumn("dbo.tblBoletasAportesEspeciales", "AnioDesdeBoleta", c => c.Int(nullable: false));
            AddColumn("dbo.tblBoletasAportesEspeciales", "MesDesdeBoleta", c => c.Int(nullable: false));
            DropColumn("dbo.tblBoletasAportesEspeciales", "Periodo");
        }
    }
}
