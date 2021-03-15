namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TurnoEmpleado : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblTurnosEmpleados",
                c => new
                    {
                        IdTurnoEmpleado = c.Int(nullable: false, identity: true),
                        IdEmpleadoEmpresa = c.Int(nullable: false),
                        Turno = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.IdTurnoEmpleado)
                .ForeignKey("dbo.tblEmpleadosEmpresas", t => t.IdEmpleadoEmpresa, cascadeDelete: false)
                .Index(t => t.IdEmpleadoEmpresa);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblTurnosEmpleados", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas");
            DropIndex("dbo.tblTurnosEmpleados", new[] { "IdEmpleadoEmpresa" });
            DropTable("dbo.tblTurnosEmpleados");
        }
    }
}
