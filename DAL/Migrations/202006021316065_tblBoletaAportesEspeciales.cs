namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblBoletaAportesEspeciales : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblBoletasAportesEspeciales",
                c => new
                    {
                        IdBoleta = c.Int(nullable: false, identity: true),
                        FechaBoleta = c.DateTime(),
                        Observaciones = c.String(),
                        IdEmpresa = c.Int(nullable: false),
                        Cuit = c.String(nullable: false),
                        RazonSocial = c.String(nullable: false),
                        NombreFantasia = c.String(nullable: false),
                        Calle = c.String(nullable: false),
                        Altura = c.Int(nullable: false),
                        Localidad = c.String(),
                        CodPostal = c.String(),
                        TelefonoFijo = c.String(),
                        TelefonoCelular = c.String(),
                        MesDesdeBoleta = c.Int(nullable: false),
                        AnioDesdeBoleta = c.Int(nullable: false),
                        MesHastaBoleta = c.Int(nullable: false),
                        AnioHastaBoleta = c.Int(nullable: false),
                        CantEmpleados = c.Int(nullable: false),
                        TotalSueldos2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Aportes2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CantAfiliados = c.Int(nullable: false),
                        TotalSueldos5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Aportes5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RecargoMora = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalDepositado = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.IdBoleta);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblBoletasAportesEspeciales");
        }
    }
}
