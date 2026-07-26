using FoodOrderAPI.Data;
using FoodOrderAPI.Services;
using Microsoft.EntityFrameworkCore;
using FoodOrderAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database 
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlite("Data Source=foodorder.db"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();