namespace oath_tests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TwitterId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "FacebookId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FacebookId");
            DropColumn("dbo.AspNetUsers", "TwitterId");
        }
    }
}
