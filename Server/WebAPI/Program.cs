using System.Configuration;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using DBCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.Infrastructure;
using NLog.Web;
using WebAPI.Mappings;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var dataBaseConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrEmpty(dataBaseConnectionString))
            throw new ConfigurationErrorsException("PostgreSQL connection string not found");
        builder.Services.AddSingleton<NhibernateHelper>(service => new NhibernateHelper(dataBaseConnectionString));
        builder.Services.AddScoped<IUnitOfWork>(sp =>
            new UnitOfWork(sp.GetRequiredService<NhibernateHelper>().SessionFactory));
        builder.Services.AddScoped<IRepository>(sp =>
            new DbRepository(sp.GetRequiredService<IUnitOfWork>()));

        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
        if (string.IsNullOrEmpty(rabbitMqConnectionString))
            throw new ConfigurationErrorsException("RabbitMQ connection string not found");
        builder.Services.AddSingleton<RabbitMqConnection>(service =>
            RabbitMqConnection.CreateAsync(rabbitMqConnectionString).GetAwaiter().GetResult());
        builder.Services.AddScoped<RabbitMQClient>();

        builder.Services.AddScoped<MetricsService>();
        builder.Services.AddScoped<TeamService>();
        builder.Services.AddScoped<EmployeeService>();
        builder.Services.AddScoped<EmployeeStatsService>();
        
        
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
        
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                       ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        
        var corsPolicyName = "CorsPolicy";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyName,
                policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin();
                    policyBuilder.AllowAnyMethod();
                    policyBuilder.AllowAnyHeader();
                });
        });
        

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseForwardedHeaders();
        app.UseCors(corsPolicyName);
        // UseHttpsRedirection отключен: HTTPS управляется reverse-proxy,
        // а не API. Редирект ломает CORS preflight и вызывает Mixed Content.
        
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}