namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActividadesFix : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblActividades", "Nombre", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblActividades", "Nombre", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
