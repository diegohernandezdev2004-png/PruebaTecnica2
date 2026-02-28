# Diagrama Entidad-Relación de la Base de Datos (PlataformaEducativa)

```mermaid
erDiagram
    Instructores {
        int Id PK "IDENTITY(1,1)"
        string Nombre "NVARCHAR(100) NOT NULL"
        string Apellidos "NVARCHAR(100) NOT NULL"
        string Email "NVARCHAR(100) NOT NULL UNIQUE"
        bit Activo "DEFAULT 1"
        datetime FechaCreacion "DEFAULT GETDATE()"
    }

    Cursos {
        int Id PK "IDENTITY(1,1)"
        string Nombre "NVARCHAR(200) NOT NULL"
        string Categoria "NVARCHAR(100) NOT NULL"
        decimal PrecioBase "DECIMAL(10,2) NOT NULL"
        int DuracionHoras "INT NOT NULL"
        int InstructorId FK "NOT NULL"
        bit Activo "DEFAULT 1"
        datetime FechaCreacion "DEFAULT GETDATE()"
    }

    Inscripciones {
        int Id PK "IDENTITY(1,1)"
        int CursoId FK "NOT NULL"
        int UsuarioId "NOT NULL"
        datetime FechaInscripcion "DEFAULT GETDATE()"
        decimal MontoPagado "DECIMAL(10,2) NOT NULL"
    }

    Instructores ||--o{ Cursos : "1 A MUCHOS"
    Cursos ||--o{ Inscripciones : "1 A MUCHOS"

    %% Constraints
    %% UNIQUE (Nombre, Categoria) in Cursos
```

## Detalles Relevantes
* **Instructores**: Guarda la información básica del instructor.
* **Cursos**: Contiene las métricas necesarias sobre el curso (Precio, Duración, Categoría), y tiene una llave foránea (`InstructorId`). También cuenta con un constraint que impide que un instructor tenga múltiples cursos con el mismo nombre y categoría (`UNIQUE (Nombre, Categoria)`).
* **Inscripciones**: Aunque el CRUD base no requiere endpoints de inscripciones completos, la tabla en la base de datos se relaciona para poder ejecutar los reportes (por ejemplo, contar `MontoPagado` o agrupar inscritos).
