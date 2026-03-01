using Libsql.Client;

var builder = WebApplication.CreateBuilder(args);

// 1. --- ADD CORS POLICY ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // Allows your localhost AND your Cloudflare domain
              .AllowAnyHeader()    // Allows the custom "X-API-Key" header
              .AllowAnyMethod();   // Allows POST requests
    });
});
// --------------------------

var app = builder.Build();

// 2. --- ACTIVATE CORS ---
app.UseCors("AllowAll");
// ------------------------

// Fetch the secrets from the configuration
var tursoUrl = builder.Configuration["Turso:Url"];
var tursoToken = builder.Configuration["Turso:AuthToken"];

// Pass them to the Turso Client
var dbClient = await DatabaseClient.Create(opts => {
    opts.Url = tursoUrl; 
    opts.AuthToken = tursoToken;
});

await dbClient.Execute("CREATE TABLE IF NOT EXISTS Posts (Id INTEGER PRIMARY KEY, Title TEXT, Content TEXT)");
await dbClient.Execute("INSERT OR IGNORE INTO Posts (Id, Title, Content) VALUES (1, 'First Post', 'Hello from Turso!')");

// Create an API endpoint to fetch the posts
app.MapGet("/api/posts", async () =>
{
    var result = await dbClient.Execute("SELECT * FROM Posts");
    
    var posts = result.Rows.Select(row => {
        var rowArray = row.ToArray();
        return new {
            Id = rowArray[0] is Integer id ? id.Value : 0,
            Title = rowArray[1] is Text title ? title.Value : "",
            Content = rowArray[2] is Text content ? content.Value : ""
        };
    });

    return Results.Ok(posts);
});

// The Secure POST endpoint
app.MapPost("/api/posts", async (CreatePostRequest req, IConfiguration config, HttpRequest request) =>
{
    // --- SECURITY CHECK ---
    var expectedApiKey = config["AdminApiKey"];
    if (!request.Headers.TryGetValue("X-API-Key", out var extractedApiKey) || extractedApiKey != expectedApiKey)
    {
        return Results.Unauthorized(); 
    }

    // Basic validation
    if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Content))
    {
        return Results.BadRequest(new { error = "Title and Content are required." });
    }

    // Insert into Turso
    await dbClient.Execute(
        "INSERT INTO Posts (Title, Content) VALUES (?, ?)",
        req.Title,
        req.Content
    );

    // Give Turso 2 seconds to sync globally before triggering the build
    await Task.Delay(2000);

    // Trigger the Cloudflare Webhook
    var webhookUrl = config["Cloudflare:WebhookUrl"] ?? config["Cloudflare__WebhookUrl"];
    if (!string.IsNullOrWhiteSpace(webhookUrl))
    {
        using var http = new HttpClient();
        await http.PostAsync(webhookUrl, new StringContent("")); 
    }

    return Results.Ok(new { message = "Post published and site rebuilding!" });
});

app.Run();

// Type declarations MUST sit at the very end of the file!
public record CreatePostRequest(string Title, string Content);
