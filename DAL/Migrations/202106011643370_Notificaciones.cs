namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Notificaciones : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblAdjuntosNotificacion",
                c => new
                    {
                        IdAdjuntoNotificacion = c.Int(nullable: false, identity: true),
                        idNotificacion = c.Int(nullable: false),
                        Adjunto = c.String(),
                    })
                .PrimaryKey(t => t.IdAdjuntoNotificacion)
                .ForeignKey("dbo.tblNotificaciones", t => t.idNotificacion, cascadeDelete: true)
                .Index(t => t.idNotificacion);
            
            CreateTable(
                "dbo.tblNotificaciones",
                c => new
                    {
                        IdNotificacion = c.Int(nullable: false, identity: true),
                        idEmpresa = c.Int(nullable: false),
                        Fecha = c.DateTime(nullable: false),
                        Titulo = c.String(nullable: false, maxLength: 80),
                        Descripcion = c.String(nullable: false),
                        Visto = c.Boolean(nullable: false),
                        FechaVisto = c.DateTime(),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.IdNotificacion)
                .ForeignKey("dbo.tblEmpresas", t => t.idEmpresa, cascadeDelete: true)
                .Index(t => t.idEmpresa);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblAdjuntosNotificacion", "idNotificacion", "dbo.tblNotificaciones");
            DropForeignKey("dbo.tblNotificaciones", "idEmpresa", "dbo.tblEmpresas");
            DropIndex("dbo.tblNotificaciones", new[] { "idEmpresa" });
            DropIndex("dbo.tblAdjuntosNotificacion", new[] { "idNotificacion" });
            DropTable("dbo.tblNotificaciones");
            DropTable("dbo.tblAdjuntosNotificacion");
        }
    }
}
