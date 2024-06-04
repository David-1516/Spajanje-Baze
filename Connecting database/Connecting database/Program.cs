using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Collage.Repository.Interface;
using Collage.Repository;
using Collage.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.json");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add configuration for database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register your repositories
IServiceCollection serviceCollection = builder.Services.AddTransient<StudentRepository, StudentRepository>(provider =>
    new StudentRepository(connectionString));

// Register your services
builder.Services.AddTransient<StudentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
