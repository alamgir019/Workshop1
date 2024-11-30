using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SimpleWS_Server.DataContext;
using SimpleWS_Server.Service;

namespace SimpleWS_Server
{
    public static class ConfigurationService
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            services.AddScoped<ICacheService, CacheService>();
            services.AddDbContext<DbContextClass>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "RedisCacheDemo",
                    Version = "v1"
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddResponseCaching();
            services.AddControllers();
        }
    }
}
