using AuditApi.Data;
using AuditApi.models;
using AuditApi.Services;
/*
using AuditApi.Services;
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var mongoCfg = builder.Configuration.GetSection("MongoDb");
builder.Services.Configure<MongoDbSettings>(mongoCfg);
var settings = mongoCfg.Get<MongoDbSettings>();


var rabbitCfg = builder.Configuration.GetSection("RabbitMQ");
builder.Services.Configure<RabbitMQSettings>(rabbitCfg);


BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Configure MongoDB client with retry settings
var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(30);

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoClientSettings));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<Activity>("Activities"));
builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();

builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
