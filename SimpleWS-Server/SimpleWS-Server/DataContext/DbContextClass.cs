namespace SimpleWS_Server.DataContext
{
    using Microsoft.EntityFrameworkCore;
    using SimpleWS_Server.Model;

    public class DbContextClass : DbContext
    {
        public DbContextClass(DbContextOptions<DbContextClass> options) : base(options) { }
        public DbSet<Product> Products
        {
            get;
            set;
        }
    }
}
