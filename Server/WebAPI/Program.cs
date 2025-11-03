using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Mappings;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        ILoggerFactory loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddJsonConsole());
        builder.Services.AddControllers();
        var mappingConfig = new MapperConfiguration(
            cfg =>
            {
                cfg.AddProfile<ObjectToDTO>();
            },
            loggerFactory);
        var mapper = mappingConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}