using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Ruta al JSON de productos
string filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data.json");
filePath = Path.GetFullPath(filePath);
Console.WriteLine($"📂 Leyendo JSON desde: {filePath}");

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

// Helpers de lectura/escritura
List<Item> ReadData() =>
    JsonSerializer.Deserialize<List<Item>>(File.ReadAllText(filePath), options) ?? new();

void WriteData(List<Item> items) =>
    File.WriteAllText(
        filePath,
        JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true })
    );

// --- tus rutas API ---
app.MapGet("/api/items", () => Results.Json(ReadData()));
app.MapGet("/api/items/{id:int}", (int id) => {
    var items = ReadData();
    var item = items.FirstOrDefault(i => i.Id == id);
    return item is null ? Results.NotFound() : Results.Json(item);
});

app.MapPost("/api/items", (Item newItem) => {
    var items = ReadData();
    newItem.Id = items.Count > 0 ? items.Max(i => i.Id) + 1 : 1;
    items.Add(newItem);
    WriteData(items);
    return Results.Created($"/api/items/{newItem.Id}", newItem);
});

app.MapPut("/api/items/{id:int}", (int id, Item updated) => {
    var items = ReadData();
    var index = items.FindIndex(i => i.Id == id);
    if (index == -1) return Results.NotFound();
    items[index].Nombre = updated.Nombre ?? items[index].Nombre;
    items[index].Precio = updated.Precio != 0 ? updated.Precio : items[index].Precio;
    WriteData(items);
    return Results.Json(items[index]);
});

app.MapDelete("/api/items/{id:int}", (int id) => {
    var items = ReadData().Where(i => i.Id != id).ToList();
    WriteData(items);
    return Results.Ok(new { message = "Eliminado correctamente" });
});

app.Run();

record Item
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public decimal Precio { get; set; }
}
