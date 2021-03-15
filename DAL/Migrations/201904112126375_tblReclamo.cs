namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tblReclamo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reclamo",
                c => new
                    {
                        IdReclamo = c.Int(nullable: false, identity: true),
                        Nombre = c.String(maxLength: 50),
                        Empresa = c.String(maxLength: 50),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 50),
                        Mensaje = c.String(),
                    })
                .PrimaryKey(t => t.IdReclamo);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Reclamo");
        }
    }
}
