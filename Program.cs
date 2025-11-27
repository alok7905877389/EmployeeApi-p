using Employees.Api.Data;
using Employees.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=YOUR_SERVER;Database=EmployeesDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapPost("/employees", async (ApplicationDbContext db, Employee emp) =>
{
    if (string.IsNullOrWhiteSpace(emp.Name))
        return Results.BadRequest(new { error = "Name is required." });

    var exists = await db.Employees.AnyAsync(e => e.Email == emp.Email);
    if (exists)
        return Results.Conflict(new { error = "Email already exists." });

    db.Employees.Add(emp);
    await db.SaveChangesAsync();

    return Results.Created($"/employees/{emp.Id}", emp);
});

app.MapGet("/employees", async (ApplicationDbContext db) =>
{
    var list = await db.Employees.ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/employees/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var emp = await db.Employees.FindAsync(id);
    return emp is not null ? Results.Ok(emp) : Results.NotFound(new { error = "Employee not found." });
});

app.Run();
