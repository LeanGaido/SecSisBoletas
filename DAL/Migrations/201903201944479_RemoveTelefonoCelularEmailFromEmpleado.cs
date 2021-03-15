namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTelefonoCelularEmailFromEmpleado : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tblEmpleados", "Telefono");
            DropColumn("dbo.tblEmpleados", "Celular");
            DropColumn("dbo.tblEmpleados", "Email");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblEmpleados", "Email", c => c.String(nullable: false));
            AddColumn("dbo.tblEmpleados", "Celular", c => c.Int(nullable: false));
            AddColumn("dbo.tblEmpleados", "Telefono", c => c.Int(nullable: false));
        }
    }
}
