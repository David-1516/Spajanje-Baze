using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Collage.Repository.Interface;
using Collage.Repository;
using Collage.Service;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{

    containerBuilder.RegisterInstance(connectionString).Named<string>("DefaultConnection");


    containerBuilder.RegisterType<StudentRepository>()
        .As<IStudentRepository>()
        .WithParameter(new NamedParameter("connectionString", connectionString))
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<StudentService>().As<IStudentService>().InstancePerLifetimeScope();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
