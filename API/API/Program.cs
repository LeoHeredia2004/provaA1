using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();


builder.Services.AddCors(options =>
    options.AddPolicy("Acesso Total",
        configs => configs
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod())
);
var app = builder.Build();


app.MapGet("/", () => "Prova A1");

//ENDPOINTS DE CATEGORIA
//GET: http://localhost:5273/api/categoria/listar
app.MapGet("/api/categoria/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Categorias.Any())
    {
        return Results.Ok(ctx.Categorias.ToList());
    }
    return Results.NotFound("Nenhuma categoria encontrada");
});

//POST: http://localhost:5273/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    ctx.Categorias.Add(categoria);
    ctx.SaveChanges();
    return Results.Created("", categoria);
});

//Atualizar categoria
app.MapPut("/api/categoria/alterar/{id}", async (string id, Categoria categoriaAtualizada, AppDataContext context) =>
{
    var categoria = await context.Categorias.FindAsync(id);
    if (categoria == null)
    {
        return Results.NotFound("Tarefa não encontrada.");
    }

    categoria.Nome = categoriaAtualizada.Nome;
    await context.SaveChangesAsync();
    return Results.Ok("Categoria atualizada.");
});

//ENDPOINTS DE TAREFA
//GET: http://localhost:5273/api/tarefas/listar
app.MapGet("/api/tarefas/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        return Results.Ok(ctx.Tarefas.Include(x => x.Categoria).ToList());
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

//POST: http://localhost:5273/api/tarefas/cadastrar
app.MapPost("/api/tarefas/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    Categoria? categoria = ctx.Categorias.Find(tarefa.CategoriaId);
    if (categoria == null)
    {
        return Results.NotFound("Categoria não encontrada");
    }
    tarefa.Categoria = categoria;
    ctx.Tarefas.Add(tarefa);
    ctx.SaveChanges();
    return Results.Created("", tarefa);
});

app.MapPut("/api/tarefas/alterar/{id}", async (string id, Tarefa tarefaAtualizada, AppDataContext context) =>
{
    var tarefa = await context.Tarefas.FindAsync(id);
    if (tarefa == null)
    {
        return Results.NotFound("Tarefa não encontrada.");
    }

    tarefa.Titulo = tarefaAtualizada.Titulo;
    tarefa.Descricao = tarefaAtualizada.Descricao;
    tarefa.Status = tarefaAtualizada.Status;
    await context.SaveChangesAsync();
    return Results.Ok("tarefa atualizada.");
});
//GET: http://localhost:5273/tarefas/naoconcluidas
app.MapGet("/api/tarefas/naoconcluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasConcluidas = await ctx.Tarefas
        .Where(t => t.Status == "Não iniciada")
        .ToListAsync();

    return Results.Ok(tarefasConcluidas);
});

//GET: http://localhost:5273/tarefas/concluidas
app.MapGet("/api/tarefas/concluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasConcluidas = await ctx.Tarefas
        .Where(t => t.Status == "concluida")
        .ToListAsync();

    return Results.Ok(tarefasConcluidas);
});


app.Run();
