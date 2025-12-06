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
