namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HorariosEmpleados : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblHorariosEmpleados",
                c => new
                    {
                        IdHorarioEmpleado = c.Int(nullable: false, identity: true),
                        IdEmpleadoEmpresa = c.Int(nullable: false),
                        Dia = c.String(nullable: false, maxLength: 20),
                        Entrada = c.String(nullable: false, maxLength: 20),
                        Salida = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.IdHorarioEmpleado)
                .ForeignKey("dbo.tblEmpleadosEmpresas", t => t.IdEmpleadoEmpresa, cascadeDelete: false)
                .Index(t => t.IdEmpleadoEmpresa);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblHorariosEmpleados", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas");
            DropIndex("dbo.tblHorariosEmpleados", new[] { "IdEmpleadoEmpresa" });
            DropTable("dbo.tblHorariosEmpleados");
        }
    }
}
