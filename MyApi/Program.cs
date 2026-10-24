using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString is not null)
{
    await Database.InitializeAsync(connectionString);
}

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();
    app.Logger.LogInformation(
        "Handled {Method} {Path} with status {StatusCode} on {Instance}",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        Environment.MachineName);
});

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    instance = Environment.MachineName
}));

app.MapGet("/api/items", async Task<IResult> () =>
{
    if (connectionString is null)
    {
        return Results.Problem(
            detail: "The DefaultConnection connection string is not configured.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var items = new List<Item>();
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new SqlCommand(
        "SELECT Id, Name FROM dbo.Items ORDER BY Id;",
        connection);
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        items.Add(new Item(reader.GetInt32(0), reader.GetString(1)));
    }

    return Results.Ok(items);
});

app.Run();

internal sealed record Item(int Id, string Name);

internal static class Database
{
    public static async Task InitializeAsync(string connectionString)
    {
        var databaseConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        await using (var connection = new SqlConnection(databaseConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                """
                IF DB_ID(N'MyApi') IS NULL
                    CREATE DATABASE [MyApi];
                """,
                connection);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException exception) when (exception.Number == 1801)
            {
                // Another API replica created the database at the same time.
            }
        }

        await using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                """
                BEGIN TRANSACTION;
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock
                    @Resource = N'MyApiSchemaInitialization',
                    @LockMode = N'Exclusive',
                    @LockOwner = N'Transaction',
                    @LockTimeout = 10000;
                IF @lockResult < 0
                    THROW 50001, 'Could not acquire the database initialization lock.', 1;

                IF OBJECT_ID(N'dbo.Items', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Items
                    (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Name nvarchar(200) NOT NULL
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM dbo.Items)
                BEGIN
                    INSERT INTO dbo.Items (Name)
                    VALUES (N'Notebook'), (N'Pencil'), (N'Backpack');
                END;

                COMMIT TRANSACTION;
                """,
                connection);

            await command.ExecuteNonQueryAsync();
        }
    }
}
