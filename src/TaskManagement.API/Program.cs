using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagement.API.Middleware;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Mappings;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Repositories;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add MediatR with behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(TaskManagement.Application.Features.Tasks.Queries.GetAllTasksQuery).Assembly);
});

// Add pipeline behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateTaskCommandValidator).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:5015")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Add API versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Task Management API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling
app.UseGlobalExceptionHandling();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();

// Add health checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();