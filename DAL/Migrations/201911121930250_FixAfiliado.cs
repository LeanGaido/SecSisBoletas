namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixAfiliado : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblAfiliados", "IdEmpleado", "dbo.tblEmpleados");
            DropIndex("dbo.tblAfiliados", new[] { "IdEmpleado" });
            AddColumn("dbo.tblAfiliados", "IdEmpleadoEmpresa", c => c.Int(nullable: false,defaultValue: 382));
            CreateIndex("dbo.tblAfiliados", "IdEmpleadoEmpresa");
            AddForeignKey("dbo.tblAfiliados", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas", "idEmpleadoEmpresa", cascadeDelete: true);
            DropColumn("dbo.tblAfiliados", "IdEmpleado");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblAfiliados", "IdEmpleado", c => c.Int(nullable: false));
            DropForeignKey("dbo.tblAfiliados", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas");
            DropIndex("dbo.tblAfiliados", new[] { "IdEmpleadoEmpresa" });
            DropColumn("dbo.tblAfiliados", "IdEmpleadoEmpresa");
            CreateIndex("dbo.tblAfiliados", "IdEmpleado");
            AddForeignKey("dbo.tblAfiliados", "IdEmpleado", "dbo.tblEmpleados", "IdEmpleado", cascadeDelete: true);
        }
    }
}
