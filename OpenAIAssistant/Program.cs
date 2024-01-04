using OpenAIAssistant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Assistant>();
//增加跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("any", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
app.UseCors("any");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
