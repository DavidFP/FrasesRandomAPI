## FrasesRandomAPI

API minimalista en ASP.NET Core 10 que expone un CRUD de frases almacenadas en SQLite mediante Entity Framework Core.

### Requisitos

- .NET SDK 10.0 o superior.

### Puesta en marcha

```bash
dotnet restore
dotnet run --project FrasesRandomAPI
```

La base de datos `quotes.db` se crea automáticamente en la carpeta de la API la primera vez que se ejecuta. El Swagger UI queda disponible en `https://localhost:5001/swagger` (o el puerto que asigne Kestrel).

### Modelo de datos

| Campo | Tipo | Descripción |
| --- | --- | --- |
| `id` | entero | Identificador autoincremental |
| `autor` | texto (120) | Nombre de la persona que dijo la frase |
| `texto` | texto (1000) | Contenido de la frase |
| `fecha` | fecha | Fecha asociada a la frase |

### Endpoints

- `GET /api/quotes` – devuelve todas las frases.
- `GET /api/quotes/{id}` – obtiene una frase específica; 404 si no existe.
- `POST /api/quotes` – crea una frase.
- `PUT /api/quotes/{id}` – actualiza una frase existente.
- `DELETE /api/quotes/{id}` – elimina una frase.

#### Ejemplo de payload para POST/PUT

```json
{
  "autor": "Albert Einstein",
  "texto": "No tengo talentos especiales. Solo soy apasionadamente curioso.",
  "fecha": "2024-01-01"
}
```

Las validaciones básicas aseguran que los campos obligatorios tengan contenido y cumplan los límites de longitud. Cualquier error se devuelve como `400 Bad Request` con detalles en el cuerpo.

### Documentación interactiva

Swagger UI recoge resúmenes y descripciones para cada operación. Puedes probar los endpoints desde `/swagger` o descargar el documento OpenAPI en `/swagger/v1/swagger.json`.

### Tests y benchmarks

El repositorio incluye pruebas de unidad (`tests/FrasesRandomAPI.UnitTests`) e integración (`tests/FrasesRandomAPI.IntegrationTests`) escritas con xUnit y ejecutadas mediante `dotnet test`. Puedes lanzar cada paquete o toda la solución:

```bash
dotnet test tests/FrasesRandomAPI.UnitTests/FrasesRandomAPI.UnitTests.csproj
dotnet test tests/FrasesRandomAPI.IntegrationTests/FrasesRandomAPI.IntegrationTests.csproj
# o
dotnet test FrasesRandomAPI.slnx
```

Los benchmarks (`tests/FrasesRandomAPI.Benchmarks`) usan BenchmarkDotNet para medir el rendimiento de los endpoints principales. Se ejecutan en modo Release:

```bash
dotnet run --project tests/FrasesRandomAPI.Benchmarks --configuration Release
```
