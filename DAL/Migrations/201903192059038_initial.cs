namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblActividades",
                c => new
                    {
                        IdActividad = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.IdActividad);
            
            CreateTable(
                "dbo.tblAfiliados",
                c => new
                    {
                        IdAfiliado = c.Int(nullable: false, identity: true),
                        IdEmpleado = c.Int(nullable: false),
                        CantFamiliaresACargo = c.Int(nullable: false),
                        FechaAlta = c.DateTime(nullable: false),
                        FechaBaja = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdAfiliado)
                .ForeignKey("dbo.tblEmpleados", t => t.IdEmpleado, cascadeDelete: false)
                .Index(t => t.IdEmpleado);
            
            CreateTable(
                "dbo.tblEmpleados",
                c => new
                    {
                        IdEmpleado = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 30),
                        Apellido = c.String(nullable: false, maxLength: 30),
                        Cuil = c.String(nullable: false),
                        Calle = c.String(nullable: false, maxLength: 50),
                        Altura = c.Int(nullable: false),
                        IdLocalidad = c.Int(nullable: false),
                        Telefono = c.Int(nullable: false),
                        Celular = c.Int(nullable: false),
                        Email = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.IdEmpleado)
                .ForeignKey("dbo.tblLocalidades", t => t.IdLocalidad, cascadeDelete: false)
                .Index(t => t.IdLocalidad);
            
            CreateTable(
                "dbo.tblEmpleadosEmpresas",
                c => new
                    {
                        idEmpleadoEmpresa = c.Int(nullable: false, identity: true),
                        idEmpleado = c.Int(nullable: false),
                        idEmpresa = c.Int(nullable: false),
                        IdCategoria = c.Int(nullable: false),
                        IdJornada = c.Int(nullable: false),
                        FechaAlta = c.DateTime(nullable: false),
                        FechaBaja = c.DateTime(),
                        EsAfiliado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.idEmpleadoEmpresa)
                .ForeignKey("dbo.tblCategorias", t => t.IdCategoria, cascadeDelete: false)
                .ForeignKey("dbo.tblEmpleados", t => t.idEmpleado, cascadeDelete: false)
                .ForeignKey("dbo.tblEmpresas", t => t.idEmpresa, cascadeDelete: false)
                .ForeignKey("dbo.tblJornadas", t => t.IdJornada, cascadeDelete: false)
                .Index(t => t.idEmpleado)
                .Index(t => t.idEmpresa)
                .Index(t => t.IdCategoria)
                .Index(t => t.IdJornada);
            
            CreateTable(
                "dbo.tblCategorias",
                c => new
                    {
                        IdCategoria = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.IdCategoria);
            
            CreateTable(
                "dbo.tblSueldosBasicos",
                c => new
                    {
                        IdSueldoBasico = c.Int(nullable: false, identity: true),
                        IdCategoria = c.Int(nullable: false),
                        Monto = c.Double(nullable: false),
                        Desde = c.DateTime(nullable: false),
                        Hasta = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdSueldoBasico)
                .ForeignKey("dbo.tblCategorias", t => t.IdCategoria, cascadeDelete: false)
                .Index(t => t.IdCategoria);
            
            CreateTable(
                "dbo.tblEmpresas",
                c => new
                    {
                        IdEmpresa = c.Int(nullable: false, identity: true),
                        Cuit = c.String(nullable: false),
                        RazonSocial = c.String(nullable: false, maxLength: 50),
                        NombreFantasia = c.String(nullable: false, maxLength: 30),
                        Calle = c.String(nullable: false),
                        Altura = c.Int(nullable: false),
                        IdLocalidad = c.Int(nullable: false),
                        TelefonoFijo = c.Int(nullable: false),
                        TelefonoCelular = c.Int(nullable: false),
                        Email = c.String(nullable: false),
                        IdActividad = c.Int(nullable: false),
                        FechaAltaEmpresa = c.DateTime(nullable: false),
                        FechaBajaEmpresa = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdEmpresa)
                .ForeignKey("dbo.tblActividades", t => t.IdActividad, cascadeDelete: false)
                .ForeignKey("dbo.tblLocalidades", t => t.IdLocalidad, cascadeDelete: false)
                .Index(t => t.IdLocalidad)
                .Index(t => t.IdActividad);
            
            CreateTable(
                "dbo.tblLocalidades",
                c => new
                    {
                        IdLocalidad = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 30),
                        CodPostal = c.Int(nullable: false),
                        IdProvincia = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IdLocalidad)
                .ForeignKey("dbo.tblProvincias", t => t.IdProvincia, cascadeDelete: false)
                .Index(t => t.IdProvincia);
            
            CreateTable(
                "dbo.tblProvincias",
                c => new
                    {
                        IdProvincia = c.Int(nullable: false, identity: true),
                        Nombre = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.IdProvincia);
            
            CreateTable(
                "dbo.tblJornadas",
                c => new
                    {
                        IdJornada = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.IdJornada);
            
            CreateTable(
                "dbo.tblDeclaracionesJuradas",
                c => new
                    {
                        IdDeclaracionJurada = c.Int(nullable: false, identity: true),
                        idEmpresa = c.Int(nullable: false),
                        mes = c.Int(nullable: false),
                        anio = c.Int(nullable: false),
                        fecha = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdDeclaracionJurada)
                .ForeignKey("dbo.tblEmpresas", t => t.idEmpresa, cascadeDelete: false)
                .Index(t => t.idEmpresa);
            
            CreateTable(
                "dbo.tblDetallesDeclaracionJurada",
                c => new
                    {
                        IdDetalleDeclaracionJurada = c.Int(nullable: false, identity: true),
                        IdDeclaracionJurada = c.Int(nullable: false),
                        IdEmpleadoEmpresa = c.Int(nullable: false),
                        idCategoria = c.Int(nullable: false),
                        idJornadaLaboral = c.Int(nullable: false),
                        Sueldo = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.IdDetalleDeclaracionJurada)
                .ForeignKey("dbo.tblCategorias", t => t.idCategoria, cascadeDelete: false)
                .ForeignKey("dbo.tblDeclaracionesJuradas", t => t.IdDeclaracionJurada, cascadeDelete: false)
                .ForeignKey("dbo.tblEmpleadosEmpresas", t => t.IdEmpleadoEmpresa, cascadeDelete: false)
                .ForeignKey("dbo.tblJornadas", t => t.idJornadaLaboral, cascadeDelete: false)
                .Index(t => t.IdDeclaracionJurada)
                .Index(t => t.IdEmpleadoEmpresa)
                .Index(t => t.idCategoria)
                .Index(t => t.idJornadaLaboral);
            
            CreateTable(
                "dbo.tblLiquidacionesProporcionalesEmpleado",
                c => new
                    {
                        IdLiquidacionProporcionalEmpleado = c.Int(nullable: false, identity: true),
                        IdLiquidacionProporcional = c.Int(nullable: false),
                        IdDetalleDeclaracionJurada = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IdLiquidacionProporcionalEmpleado)
                .ForeignKey("dbo.tblDetallesDeclaracionJurada", t => t.IdDetalleDeclaracionJurada, cascadeDelete: false)
                .ForeignKey("dbo.tblLiquidacionesProporcionales", t => t.IdLiquidacionProporcional, cascadeDelete: false)
                .Index(t => t.IdLiquidacionProporcional)
                .Index(t => t.IdDetalleDeclaracionJurada);
            
            CreateTable(
                "dbo.tblLiquidacionesProporcionales",
                c => new
                    {
                        IdLiquidacionProporcional = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 80),
                    })
                .PrimaryKey(t => t.IdLiquidacionProporcional);
            
            CreateTable(
                "dbo.tblLicenciasEmpleados",
                c => new
                    {
                        IdLicenciaEmpleado = c.Int(nullable: false, identity: true),
                        IdLicenciaLaboral = c.Int(nullable: false),
                        IdEmpleadoEmpresa = c.Int(nullable: false),
                        FechaAltaLicencia = c.DateTime(nullable: false),
                        FechaBajaLicencia = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdLicenciaEmpleado)
                .ForeignKey("dbo.tblEmpleadosEmpresas", t => t.IdEmpleadoEmpresa, cascadeDelete: false)
                .ForeignKey("dbo.tblLicenciasLaborales", t => t.IdLicenciaLaboral, cascadeDelete: false)
                .Index(t => t.IdLicenciaLaboral)
                .Index(t => t.IdEmpleadoEmpresa);
            
            CreateTable(
                "dbo.tblLicenciasLaborales",
                c => new
                    {
                        IdLicenciaLaboral = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.IdLicenciaLaboral);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblLicenciasEmpleados", "IdLicenciaLaboral", "dbo.tblLicenciasLaborales");
            DropForeignKey("dbo.tblLicenciasEmpleados", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas");
            DropForeignKey("dbo.tblLiquidacionesProporcionalesEmpleado", "IdLiquidacionProporcional", "dbo.tblLiquidacionesProporcionales");
            DropForeignKey("dbo.tblLiquidacionesProporcionalesEmpleado", "IdDetalleDeclaracionJurada", "dbo.tblDetallesDeclaracionJurada");
            DropForeignKey("dbo.tblDetallesDeclaracionJurada", "idJornadaLaboral", "dbo.tblJornadas");
            DropForeignKey("dbo.tblDetallesDeclaracionJurada", "IdEmpleadoEmpresa", "dbo.tblEmpleadosEmpresas");
            DropForeignKey("dbo.tblDetallesDeclaracionJurada", "IdDeclaracionJurada", "dbo.tblDeclaracionesJuradas");
            DropForeignKey("dbo.tblDetallesDeclaracionJurada", "idCategoria", "dbo.tblCategorias");
            DropForeignKey("dbo.tblDeclaracionesJuradas", "idEmpresa", "dbo.tblEmpresas");
            DropForeignKey("dbo.tblAfiliados", "IdEmpleado", "dbo.tblEmpleados");
            DropForeignKey("dbo.tblEmpleados", "IdLocalidad", "dbo.tblLocalidades");
            DropForeignKey("dbo.tblEmpleadosEmpresas", "IdJornada", "dbo.tblJornadas");
            DropForeignKey("dbo.tblEmpleadosEmpresas", "idEmpresa", "dbo.tblEmpresas");
            DropForeignKey("dbo.tblEmpresas", "IdLocalidad", "dbo.tblLocalidades");
            DropForeignKey("dbo.tblLocalidades", "IdProvincia", "dbo.tblProvincias");
            DropForeignKey("dbo.tblEmpresas", "IdActividad", "dbo.tblActividades");
            DropForeignKey("dbo.tblEmpleadosEmpresas", "idEmpleado", "dbo.tblEmpleados");
            DropForeignKey("dbo.tblEmpleadosEmpresas", "IdCategoria", "dbo.tblCategorias");
            DropForeignKey("dbo.tblSueldosBasicos", "IdCategoria", "dbo.tblCategorias");
            DropIndex("dbo.tblLicenciasEmpleados", new[] { "IdEmpleadoEmpresa" });
            DropIndex("dbo.tblLicenciasEmpleados", new[] { "IdLicenciaLaboral" });
            DropIndex("dbo.tblLiquidacionesProporcionalesEmpleado", new[] { "IdDetalleDeclaracionJurada" });
            DropIndex("dbo.tblLiquidacionesProporcionalesEmpleado", new[] { "IdLiquidacionProporcional" });
            DropIndex("dbo.tblDetallesDeclaracionJurada", new[] { "idJornadaLaboral" });
            DropIndex("dbo.tblDetallesDeclaracionJurada", new[] { "idCategoria" });
            DropIndex("dbo.tblDetallesDeclaracionJurada", new[] { "IdEmpleadoEmpresa" });
            DropIndex("dbo.tblDetallesDeclaracionJurada", new[] { "IdDeclaracionJurada" });
            DropIndex("dbo.tblDeclaracionesJuradas", new[] { "idEmpresa" });
            DropIndex("dbo.tblLocalidades", new[] { "IdProvincia" });
            DropIndex("dbo.tblEmpresas", new[] { "IdActividad" });
            DropIndex("dbo.tblEmpresas", new[] { "IdLocalidad" });
            DropIndex("dbo.tblSueldosBasicos", new[] { "IdCategoria" });
            DropIndex("dbo.tblEmpleadosEmpresas", new[] { "IdJornada" });
            DropIndex("dbo.tblEmpleadosEmpresas", new[] { "IdCategoria" });
            DropIndex("dbo.tblEmpleadosEmpresas", new[] { "idEmpresa" });
            DropIndex("dbo.tblEmpleadosEmpresas", new[] { "idEmpleado" });
            DropIndex("dbo.tblEmpleados", new[] { "IdLocalidad" });
            DropIndex("dbo.tblAfiliados", new[] { "IdEmpleado" });
            DropTable("dbo.tblLicenciasLaborales");
            DropTable("dbo.tblLicenciasEmpleados");
            DropTable("dbo.tblLiquidacionesProporcionales");
            DropTable("dbo.tblLiquidacionesProporcionalesEmpleado");
            DropTable("dbo.tblDetallesDeclaracionJurada");
            DropTable("dbo.tblDeclaracionesJuradas");
            DropTable("dbo.tblJornadas");
            DropTable("dbo.tblProvincias");
            DropTable("dbo.tblLocalidades");
            DropTable("dbo.tblEmpresas");
            DropTable("dbo.tblSueldosBasicos");
            DropTable("dbo.tblCategorias");
            DropTable("dbo.tblEmpleadosEmpresas");
            DropTable("dbo.tblEmpleados");
            DropTable("dbo.tblAfiliados");
            DropTable("dbo.tblActividades");
        }
    }
}
