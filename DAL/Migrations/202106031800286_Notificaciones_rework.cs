namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Notificaciones_rework : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblNotificaciones", "idEmpresa", "dbo.tblEmpresas");
            DropIndex("dbo.tblNotificaciones", new[] { "idEmpresa" });
            CreateTable(
                "dbo.tblNotificacionesEmpresa",
                c => new
                    {
                        IdNotificacionEmpresa = c.Int(nullable: false, identity: true),
                        idEmpresa = c.Int(nullable: false),
                        IdNotificacion = c.Int(nullable: false),
                        Visto = c.Boolean(nullable: false),
                        FechaVisto = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdNotificacionEmpresa)
                .ForeignKey("dbo.tblEmpresas", t => t.idEmpresa, cascadeDelete: true)
                .ForeignKey("dbo.tblNotificaciones", t => t.IdNotificacion, cascadeDelete: true)
                .Index(t => t.idEmpresa)
                .Index(t => t.IdNotificacion);
            
            DropColumn("dbo.tblNotificaciones", "idEmpresa");
            DropColumn("dbo.tblNotificaciones", "Visto");
            DropColumn("dbo.tblNotificaciones", "FechaVisto");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblNotificaciones", "FechaVisto", c => c.DateTime());
            AddColumn("dbo.tblNotificaciones", "Visto", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblNotificaciones", "idEmpresa", c => c.Int(nullable: false));
            DropForeignKey("dbo.tblNotificacionesEmpresa", "IdNotificacion", "dbo.tblNotificaciones");
            DropForeignKey("dbo.tblNotificacionesEmpresa", "idEmpresa", "dbo.tblEmpresas");
            DropIndex("dbo.tblNotificacionesEmpresa", new[] { "IdNotificacion" });
            DropIndex("dbo.tblNotificacionesEmpresa", new[] { "idEmpresa" });
            DropTable("dbo.tblNotificacionesEmpresa");
            CreateIndex("dbo.tblNotificaciones", "idEmpresa");
            AddForeignKey("dbo.tblNotificaciones", "idEmpresa", "dbo.tblEmpresas", "IdEmpresa", cascadeDelete: true);
        }
    }
}
