using Libsql.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Fetch the secrets from the configuration
var tursoUrl = builder.Configuration["Turso:Url"];
var tursoToken = builder.Configuration["Turso:AuthToken"];

// 2. Pass them to the Turso Client
var dbClient = await DatabaseClient.Create(opts => {
    opts.Url = tursoUrl; 
    opts.AuthToken = tursoToken;
});

await dbClient.Execute("CREATE TABLE IF NOT EXISTS Posts (Id INTEGER PRIMARY KEY, Title TEXT, Content TEXT)");
await dbClient.Execute("INSERT OR IGNORE INTO Posts (Id, Title, Content) VALUES (1, 'First Post', 'Hello from Turso!')");

// 3. Create an API endpoint to fetch the posts
app.MapGet("/api/posts", async () =>
{
    var result = await dbClient.Execute("SELECT * FROM Posts");
    
    // LibSQL returns custom types, so we map them to a standard anonymous object
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

// 1. Remove IDatabaseClient db from the parentheses here
app.MapPost("/api/posts", async (CreatePostRequest req) =>
{
    // Basic validation
    if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Content))
    {
        return Results.BadRequest(new { error = "Title and Content are required." });
    }

    // Notice we are using dbClient here to match line 11!
    await dbClient.Execute(
        "INSERT INTO Posts (Title, Content) VALUES (?, ?)",
        req.Title,
        req.Content
    );

    return Results.Ok(new { message = "Post published successfully!" });
});
app.Run();

public record CreatePostRequest(string Title, string Content);
