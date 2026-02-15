using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.API.Common.Data
{
    // EF Core detecta automáticamente cualquier clase que implemente
    // IDesignTimeDbContextFactory<T> y la usa cuando no puede resolver
    // el contexto a través del sistema de DI normal.
    // Esto ocurre cuando corrés Add-Migration o Update-Database desde
    // la Package Manager Console, antes de que Program.cs esté corriendo.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Construimos la configuración manualmente leyendo el appsettings.json.
            // En Program.cs esto lo hace el framework automáticamente, pero acá
            // en tiempo de diseño tenemos que hacerlo nosotros explícitamente.
            // SetBasePath le dice dónde buscar el archivo — la carpeta del proyecto.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Ahora leemos el connection string del mismo lugar que Program.cs,
            // así hay una sola fuente de verdad para ese valor.
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
