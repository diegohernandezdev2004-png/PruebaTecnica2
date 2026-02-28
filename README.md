# Proyecto API de Cursos

Plataforma educativa con servicio REST para la gestión de cursos online. Desarrollado en **C# (.NET 8)** siguiendo los principios **SOLID**, implementando el patrón **Repository y Service Layer**, validaciones con **FluentValidation** e inyección de dependencias.

## Requisitos Previos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)

## Configuración y Ejecución
1.  **Base de Datos**: 
    - Asegúrese de tener una base de datos creada en SQL Server (por defecto `PlataformaEducativa`).
    - Modifique la cadena de conexión en `CursosAPI/appsettings.json` o `CursosAPI/appsettings.Development.json` según su servidor SQL.
2.  **Scripts SQL**:
    - Ejecute el script SQL (incluido en el zip final o carpeta designada) en su base de datos para crear la estructura de tablas y los **Procedimientos Almacenados**.
    - *Nota: El script crea automáticamente la base de datos `PlataformaEducativa` si esta no existe, y la recrea desde cero para asegurar una ejecución completamente limpia y sin conflictos.*
    
3.  **Ejecución**:
    - Abra una terminal en la carpeta principal del proyecto (donde se encuentra `CursosAPI.sln`).
    - Ejecute las pruebas unitarias para validar funcionalidad:
      ```bash
      dotnet test
      ```
    - Ejecute la API:
      ```bash
      dotnet run --project CursosAPI/CursosAPI.csproj
      ```
    - Swagger estará disponible en: [https://localhost:7103/swagger](https://localhost:7103/swagger) (sujeto al puerto configurado).

**Base URL:** [https://localhost:7103/api/v1](https://localhost:7103/api/v1)

## Estructura de Capas
El proyecto usa una arquitectura en capas:
*   **CursosAPI.Domain**: Entidades, DTOs e Interfaces (contratos, sin dependencias externas).
*   **CursosAPI.Infrastructure**: Comunicación con BD. Uso directo de ADO.NET clásico y Procedimientos Almacenados mediante `SqlCommand`.
*   **CursosAPI.Service**: Lógica de Negocio, Validaciones (`FluentValidation`) y Cache In-Memory.
*   **CursosAPI**: Manejo de peticiones HTTP (Controladores), Autorización JWT y Middleware Global de Errores.

## Uso de Parámetros de Paginación, Filtro y Ordenamiento

Los endpoints GET (Listar y Reportes) aceptan parámetros en Query String para facilitar la navegación y búsqueda tabular:

| Parámetro  | Descripción | Valor por Defecto | Ejemplo |
| ---------  | ----------- | ----------------- | ------- |
| `page`     | Número de página actual. | **1** | `?page=2` |
| `pageSize` | Cantidad de registros por página. | **10** | `?pageSize=5` |
| `sortBy`   | Nombre de la columna por la cual ordenar. | **Nombre** | `?sortBy=Categoria` |
| `sortDir`  | Dirección del ordenamiento (`asc` para ascendente o `desc` para descendente). | **asc** | `?sortDir=desc` |
| `filtro`   | Término de búsqueda general. | *Ninguno* | `?filtro=programacion` |

**Columnas válidas para `sortBy`:**
Dependiendo del endpoint, se recomienda utilizar columnas como: `Nombre`, `Categoria`, `FechaCreacion`, `TotalInscritos`.

### Comportamiento por defecto
Si no se envían parámetros, la API demuestra su gran robustez al asignar valores predeterminados automáticos. Por ejemplo, al consultar los cursos sin enviar argumentos en la URL, **la API devuelve la primera página, ordenada por "Nombre" de forma ascendente, limitando los resultados a 10 registros.** (Es decir: `page=1`, `pageSize=10`, `sortDir=asc` y sin ningún filtro aplicado).

### Ejemplos de Consumo

*   **Ejemplo simple (sin parámetros):**
    Devuelve los primeros 10 cursos ordenados alfabéticamente por su nombre.
    ```http
    GET /api/v1/cursos
    ```

*   **Ejemplo avanzado (todos los parámetros):**
    Devuelve los cursos que contengan la palabra "C#", trayendo 5 registros correspondientes a la página 1, y ordenados por su nombre en orden descendente.
    ```http
    GET /api/v1/cursos?page=1&pageSize=5&sortBy=Nombre&sortDir=desc&filtro=C%23
    ```

*   **Reporte de Categorías (todas):**
    ```http
    GET /api/v1/reportes/cursos-por-categoria
    ```

*   **Reporte de Cursos más inscritos (Top 5):**
    ```http
    GET /api/v1/reportes/cursos-mas-inscritos?page=1&pageSize=5&sortBy=TotalInscritos&sortDir=desc
    ```

## Autenticación JWT y Roles
El proyecto utiliza JWT. Usuarios de ejemplo recomendados (deben crearse en BD o mockearse en los SPs):
*   **Admin**: `admin@demo.com` / `Admin.123` (Acceso total, CRUD + reportes).
*   **User**: `usuario@demo.com` / `User.123` (Solo lectura de reportes y Listado general).

### Endpoint de Autenticación
Para obtener un token JWT valido, se debe realizar una petición POST al endpoint de login enviando las credenciales en el body:

```http
POST /api/v1/auth/login
Content-Type: application/json

{
    "email": "admin@demo.com",
    "password": "Admin.123"
}
```

En **Swagger**, debe presionar el botón de "Authorize", poner `Bearer {token}` (ejemplo: `Bearer eyJhbGci...`) y presionar Apply. Todos los endpoints restringidos verificarán esto internamente usando atributos `[Authorize(Roles="X")]`.

## Tests Unitarios
Se ha alcanzado una cobertura recomendada en los métodos principales de `CursosAPI.Service` usando `xUnit` y `Moq`. Puede encontrarlos dentro de `CursosAPI.Tests`.
