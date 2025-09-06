using DevConferenceAPI.Data;
using DevConferenceAPI.Manager;
using DevConferenceAPI.Services.Interfaces;
using DevConferenceAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString") ?? throw new InvalidOperationException("Connection string 'DevConferenceContext' not found.")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Repo
builder.Services.AddScoped<IConferences, ConferencesRepository>();


// Managers

builder.Services.AddScoped<ConferencesManager>();


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
