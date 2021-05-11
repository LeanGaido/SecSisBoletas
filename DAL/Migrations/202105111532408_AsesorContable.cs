namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AsesorContable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AsesorContable",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        idEmpresa = c.Int(nullable: false),
                        NombreContacto = c.String(),
                        AreaTelefono = c.String(),
                        NumeroTelefono = c.String(),
                        AreaCelular = c.String(),
                        NumeroCelular = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblEmpresas", t => t.idEmpresa, cascadeDelete: true)
                .Index(t => t.idEmpresa);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AsesorContable", "idEmpresa", "dbo.tblEmpresas");
            DropIndex("dbo.AsesorContable", new[] { "idEmpresa" });
            DropTable("dbo.AsesorContable");
        }
    }
}
