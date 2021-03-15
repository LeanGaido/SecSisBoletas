namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixLengthRazonSocialNombreFantasia : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblEmpresas", "RazonSocial", c => c.String(nullable: false));
            AlterColumn("dbo.tblEmpresas", "NombreFantasia", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblEmpresas", "NombreFantasia", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.tblEmpresas", "RazonSocial", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
