using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 200 * 1024 * 1024; // 200 MB
    options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
    
});
//SocketsHttpHandler.EnableMultipleHttp2Connections
builder.Services.AddMagicOnion();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    // Replace to this line instead of MapGrpcService<GreeterService>()
    endpoints.MapMagicOnionService();

});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();