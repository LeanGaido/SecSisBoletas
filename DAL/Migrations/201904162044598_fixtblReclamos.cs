namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixtblReclamos : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblReclamos", "Nombre", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Empresa", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Telefono", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Email", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Mensaje", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblReclamos", "Mensaje", c => c.String());
            AlterColumn("dbo.tblReclamos", "Email", c => c.String(maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Telefono", c => c.String(maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Empresa", c => c.String(maxLength: 50));
            AlterColumn("dbo.tblReclamos", "Nombre", c => c.String(maxLength: 50));
        }
    }
}
