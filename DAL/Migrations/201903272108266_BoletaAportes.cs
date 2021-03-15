namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BoletaAportes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblBoletaAportes",
                c => new
                    {
                        IdBoleta = c.Int(nullable: false, identity: true),
                        IdDeclaracionJurada = c.Int(nullable: false),
                        MesBoleta = c.Int(nullable: false),
                        AnioBoleta = c.Int(nullable: false),
                        FechaVencimiento = c.DateTime(nullable: false),
                        TotalSueldos = c.Double(nullable: false),
                        RecargoMora = c.Double(),
                        BoletaPagada = c.Boolean(nullable: false),
                        FechaPago = c.DateTime(),
                        FechaBoleta = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdBoleta)
                .ForeignKey("dbo.tblDeclaracionesJuradas", t => t.IdDeclaracionJurada, cascadeDelete: true)
                .Index(t => t.IdDeclaracionJurada);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblBoletaAportes", "IdDeclaracionJurada", "dbo.tblDeclaracionesJuradas");
            DropIndex("dbo.tblBoletaAportes", new[] { "IdDeclaracionJurada" });
            DropTable("dbo.tblBoletaAportes");
        }
    }
}
