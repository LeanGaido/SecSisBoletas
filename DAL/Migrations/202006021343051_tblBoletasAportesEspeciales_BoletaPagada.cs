namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletasAportesEspeciales_BoletaPagada : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblBoletasAportesEspeciales", "BoletaPagada", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblBoletasAportesEspeciales", "FechaPago", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblBoletasAportesEspeciales", "FechaPago");
            DropColumn("dbo.tblBoletasAportesEspeciales", "BoletaPagada");
        }
    }
}
