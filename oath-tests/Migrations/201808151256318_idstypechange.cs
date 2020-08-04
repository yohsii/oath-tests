namespace oath_tests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class idstypechange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "TwitterId", c => c.Long());
            AlterColumn("dbo.AspNetUsers", "FacebookId", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "FacebookId", c => c.Int());
            AlterColumn("dbo.AspNetUsers", "TwitterId", c => c.Int());
        }
    }
}
