using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;


namespace RaceDay.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            //make the RaceDay database available to the API
            builder.Services.AddDbContext<RaceDayDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("RaceDay")));
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //activate the controller files that we will create for our endpoints
            app.MapControllers();


            app.MapControllers();
            app.Run();
        }
    }
}
