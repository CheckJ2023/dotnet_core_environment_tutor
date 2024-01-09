using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; //29加，跳96行

// //02加
// using System;
// using System.Threading.Tasks;
// using MySqlConnector;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using MySqlConnector;
// using System.Data;
// //02加

// using System;
// using System.Threading.Tasks;
// using MySqlConnector;
// namespace AzureMySqlExample
// {
//     static async Task Main(string[] args)
//     {
//         var builder = new MySqlConnectionStringBuilder
//         {
//             Server = "",
//             Database = "",
//             UserID = "",
//             Password = "",
//             SslMode = MySqlSslMode.Required,
//         };
//         using (var conn = new MySqlConnection(builder.ConnectionString))
//     }
// }

var builder = WebApplication.CreateBuilder(args);

//自加
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//自加

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

//自加
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
//自加

app.MapGet("/", () => "Hello World!");

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});


//修改
app.MapPost("/echo", async (MessageData messageData) =>
{
    if (messageData != null && !string.IsNullOrEmpty(messageData.Message))
    {
        var response = new
        {
            code = 0,
            message = $"原文response回去:{messageData.Message}"
        };
        return Results.Ok(response);
    }
    else
    {
        return Results.Text("錯誤");
    }
    // db.Todos.Add(todo);
    // await db.SaveChangesAsync();

    // return Results.Created($"/todoitems/{todo.Id}", todo);
});
//修改


//29加

/* 直接指定加載appsettings的部份先移除
var configuration = new ConfigurationBuilder()
.AddJsonFile("appsettings.json",optional:false,reloadOnChange: true)
.Build();
builder.Configuration.AddConfiguration(configuration);
*/

// var env = app.Environment;

var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "appsettings";

var configBuilder = new ConfigurationBuilder()
// .SetBasePath(env.ContentRootPath)
.SetBasePath(app.Environment.ContentRootPath)
.AddJsonFile($"appsettings.json",optional:false,reloadOnChange:true)
.AddJsonFile($"appsettings.{envName}.json",optional:true,reloadOnChange:true);

configBuilder.AddEnvironmentVariables();
var configuration = configBuilder.Build();

builder.Configuration.AddConfiguration(configuration);

app.MapGet("/config", (IConfiguration configuration) =>
{
    var defaultConnetion = configuration["ConnectionStrings:DefaultConnection"];
    var database = configuration["ConnectionStrings:Database"];
    //沒有特別設定時，development有相同的會先抓development.json的檔

    if (!string.IsNullOrEmpty(defaultConnetion) && !string.IsNullOrEmpty(database))
    {
        var response = new
        {
            DefaultCon = defaultConnetion,
            Databa = database
        };
        return Results.Ok(response);

    }
    else {
        return Results.Text("取JSON設定檔的API未成功");
    }
});


//02加

// // 在你的程式碼中加入登入的 API
// app.MapPost("/login", async (LoginData loginData, IConfiguration configuration) =>
// {
//     // 從 appsettings.json 中讀取資料庫連線字串
//     string connectionString = configuration.GetConnectionString("DefaultConnection");

//     using MySqlConnection connection = new MySqlConnection(connectionString);
//     MySqlCommand cmd = connection.CreateCommand();

//     try
//     {
//         connection.Open();

//         // 使用輸入的使用者名稱和密碼查詢資料庫
//         cmd.CommandText = "SELECT * FROM user WHERE username = @username AND password = @password";
//         cmd.Parameters.AddWithValue("@username", loginData.Username);
//         cmd.Parameters.AddWithValue("@password", loginData.Password);

//         MySqlDataReader reader = cmd.ExecuteReader();

//         if (reader.HasRows)
//         {
//             // 登入成功
//             var response = new
//             {
//                 code = 0,
//                 message = "登入成功"
//             };
//             return Results.Ok(response);
//         }
//         else
//         {
//             // 登入失敗
//             var response = new
//             {
//                 code = 1,
//                 message = "登入失敗"
//             };
//             return Results.Ok(response);
        
//         }
//     }
//     catch (Exception ex)
//     {
//         // 處理例外狀況
//         return Results.BadRequest(ex.Message);
//     }
//     finally
//     {
//         connection.Close();
//     }
// });


// //02加


app.Run();

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

public class MessageData
{
    public string Message { get; set; }
    //這裡的get為自動編譯生成，作用是將後端的值返回給前端。
    //而set則是負責存取前端傳傳過來的值，並將值set賦予後端使用。
}
