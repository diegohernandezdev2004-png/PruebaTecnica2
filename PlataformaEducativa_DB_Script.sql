-- Crear base de datos automática (si existe, la elimina y la recrea)
IF DB_ID('PlataformaEducativa') IS NOT NULL
BEGIN
    ALTER DATABASE PlataformaEducativa SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PlataformaEducativa;
END
GO

CREATE DATABASE PlataformaEducativa;
GO

USE PlataformaEducativa;
GO

-- 1. CREACIÓN DE TABLAS --

CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL, 
    Rol NVARCHAR(50) NOT NULL DEFAULT 'User',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Instructores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Cursos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    Categoria NVARCHAR(100) NOT NULL,
    PrecioBase DECIMAL(18,2) NOT NULL,
    DuracionHoras INT NOT NULL,
    InstructorId INT NOT NULL FOREIGN KEY REFERENCES Instructores(Id),
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Inscripciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    CursoId INT NOT NULL FOREIGN KEY REFERENCES Cursos(Id),
    FechaInscripcion DATETIME NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Activa' -- Activa, Cancelada, Completada
);
GO

-- 2. PROCEDIMIENTOS ALMACENADOS --

-- Autenticar Usuario
CREATE OR ALTER PROCEDURE sp_Usuarios_Autenticar
    @Email NVARCHAR(200),
    @Password NVARCHAR(500)
AS
BEGIN
    -- Nota: En producción esto debería comparar Hashes (ej. BCrypt).
    SELECT Id, Email, Rol FROM Usuarios 
    WHERE Email = @Email AND PasswordHash = @Password;
END
GO

-- CRUD Instructores
CREATE OR ALTER PROCEDURE sp_Instructores_ContarCursosActivos
    @InstructorId INT
AS
BEGIN
    SELECT COUNT(*) FROM Cursos WHERE InstructorId = @InstructorId AND Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_Instructores_Crear
    @Nombre NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Email NVARCHAR(200),
    @Activo BIT,
    @Id INT OUTPUT
AS
BEGIN
    INSERT INTO Instructores (Nombre, Apellidos, Email, Activo)
    VALUES (@Nombre, @Apellidos, @Email, @Activo);
    
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_Instructores_Actualizar
    @Id INT,
    @Nombre NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Email NVARCHAR(200),
    @Activo BIT
AS
BEGIN
    UPDATE Instructores 
    SET Nombre = @Nombre, Apellidos = @Apellidos, Email = @Email, Activo = @Activo
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Instructores_Eliminar
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Cursos WHERE InstructorId = @Id)
    BEGIN
        THROW 50001, 'No se puede eliminar el instructor porque tiene cursos asignados.', 1;
    END

    DELETE FROM Instructores WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Instructores_Listar
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'Id',
    @SortDir NVARCHAR(4) = 'asc',
    @Filtro NVARCHAR(100) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SELECT @TotalCount = COUNT(*) FROM Instructores 
    WHERE (@Filtro IS NULL OR Nombre LIKE '%' + @Filtro + '%' OR Apellidos LIKE '%' + @Filtro + '%' OR Email LIKE '%' + @Filtro + '%');

    DECLARE @Sql NVARCHAR(MAX);
    SET @Sql = N'
    SELECT Id, Nombre, Apellidos, Email, Activo
    FROM Instructores
    WHERE (@Filtro IS NULL OR Nombre LIKE ''%'' + @Filtro + ''%'' OR Apellidos LIKE ''%'' + @Filtro + ''%'' OR Email LIKE ''%'' + @Filtro + ''%'')
    ORDER BY ' + @SortBy + ' ' + @SortDir + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @Sql, 
         N'@PageNumber INT, @PageSize INT, @Filtro NVARCHAR(100)', 
         @PageNumber, @PageSize, @Filtro;
         
    -- Devolver TotalCount como Resultset 2
    SELECT @TotalCount AS TotalCount;
END
GO

CREATE OR ALTER PROCEDURE sp_Instructores_ObtenerPorId
    @Id INT
AS
BEGIN
    SELECT Id, Nombre, Apellidos, Email, Activo FROM Instructores WHERE Id = @Id;
END
GO

-- CRUD Cursos
CREATE OR ALTER PROCEDURE sp_Cursos_ExisteNombreCategoria
    @Nombre NVARCHAR(200),
    @Categoria NVARCHAR(100),
    @ExcludeId INT = NULL
AS
BEGIN
    SELECT COUNT(*) 
    FROM Cursos 
    WHERE Nombre = @Nombre AND Categoria = @Categoria 
    AND (@ExcludeId IS NULL OR Id <> @ExcludeId);
END
GO

CREATE OR ALTER PROCEDURE sp_Cursos_Crear
    @Nombre NVARCHAR(200),
    @Categoria NVARCHAR(100),
    @PrecioBase DECIMAL(18,2),
    @DuracionHoras INT,
    @InstructorId INT,
    @Activo BIT,
    @Id INT OUTPUT
AS
BEGIN
    INSERT INTO Cursos (Nombre, Categoria, PrecioBase, DuracionHoras, InstructorId, Activo)
    VALUES (@Nombre, @Categoria, @PrecioBase, @DuracionHoras, @InstructorId, @Activo);
    
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_Cursos_Actualizar
    @Id INT,
    @Nombre NVARCHAR(200),
    @Categoria NVARCHAR(100),
    @PrecioBase DECIMAL(18,2),
    @DuracionHoras INT,
    @InstructorId INT,
    @Activo BIT
AS
BEGIN
    UPDATE Cursos 
    SET Nombre = @Nombre, Categoria = @Categoria, PrecioBase = @PrecioBase, 
        DuracionHoras = @DuracionHoras, InstructorId = @InstructorId, Activo = @Activo
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Cursos_Eliminar
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Inscripciones WHERE CursoId = @Id)
    BEGIN
        THROW 50002, 'No se puede eliminar el curso porque tiene inscripciones.', 1;
    END

    DELETE FROM Cursos WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Cursos_Listar
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'Id',
    @SortDir NVARCHAR(4) = 'asc',
    @Filtro NVARCHAR(100) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SELECT @TotalCount = COUNT(*) FROM Cursos 
    WHERE (@Filtro IS NULL OR Nombre LIKE '%' + @Filtro + '%' OR Categoria LIKE '%' + @Filtro + '%');

    DECLARE @Sql NVARCHAR(MAX);
    SET @Sql = N'
    SELECT Id, Nombre, Categoria, PrecioBase, DuracionHoras, InstructorId, Activo
    FROM Cursos
    WHERE (@Filtro IS NULL OR Nombre LIKE ''%'' + @Filtro + ''%'' OR Categoria LIKE ''%'' + @Filtro + ''%'')
    ORDER BY ' + @SortBy + ' ' + @SortDir + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @Sql, 
         N'@PageNumber INT, @PageSize INT, @Filtro NVARCHAR(100)', 
         @PageNumber, @PageSize, @Filtro;
         
    -- Devolver TotalCount como Resultset 2
    SELECT @TotalCount AS TotalCount;
END
GO

CREATE OR ALTER PROCEDURE sp_Cursos_ObtenerPorId
    @Id INT
AS
BEGIN
    SELECT Id, Nombre, Categoria, PrecioBase, DuracionHoras, InstructorId, Activo FROM Cursos WHERE Id = @Id;
END
GO

-- Reportes
CREATE OR ALTER PROCEDURE sp_Reportes_CursosPorCategoria
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'Categoria',
    @SortDir NVARCHAR(4) = 'asc',
    @Filtro NVARCHAR(100) = NULL
AS
BEGIN
    -- Tabla temporal para almacenar los agregados
    CREATE TABLE #TempData
    (
        Categoria NVARCHAR(100),
        TotalCursos INT,
        PrecioPromedio DECIMAL(18,2)
    );

    INSERT INTO #TempData
    SELECT Categoria, COUNT(*) AS TotalCursos, AVG(PrecioBase) AS PrecioPromedio
    FROM Cursos
    WHERE @Filtro IS NULL OR Categoria LIKE '%' + @Filtro + '%'
    GROUP BY Categoria;

    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM #TempData;

    -- Devolver Paginado
    DECLARE @Sql NVARCHAR(MAX);
    SET @Sql = N'
    SELECT Categoria, TotalCursos, PrecioPromedio
    FROM #TempData
    ORDER BY ' + @SortBy + ' ' + @SortDir + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @Sql, N'@PageNumber INT, @PageSize INT', @PageNumber, @PageSize;

    -- Devolver TotalCount
    SELECT @TotalCount AS TotalCount;

    DROP TABLE #TempData;
END
GO

CREATE OR ALTER PROCEDURE sp_Reportes_CursosMasInscritos
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'TotalInscritos',
    @SortDir NVARCHAR(4) = 'desc',
    @Filtro NVARCHAR(100) = NULL
AS
BEGIN
    CREATE TABLE #TempData
    (
        CursoId INT,
        NombreCurso NVARCHAR(200),
        TotalInscritos INT
    );

    INSERT INTO #TempData
    SELECT c.Id, c.Nombre, COUNT(i.Id)
    FROM Cursos c
    LEFT JOIN Inscripciones i ON c.Id = i.CursoId AND i.Estado = 'Activa'
    WHERE (@Filtro IS NULL OR c.Nombre LIKE '%' + @Filtro + '%')
    GROUP BY c.Id, c.Nombre;

    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM #TempData;

    DECLARE @Sql NVARCHAR(MAX);
    SET @Sql = N'
    SELECT CursoId, NombreCurso, TotalInscritos
    FROM #TempData
    ORDER BY ' + @SortBy + ' ' + @SortDir + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @Sql, N'@PageNumber INT, @PageSize INT', @PageNumber, @PageSize;

    SELECT @TotalCount AS TotalCount;
    DROP TABLE #TempData;
END
GO

-- 3. DATOS DE PRUEBA --

INSERT INTO Usuarios (Email, PasswordHash, Rol) VALUES 
('admin@demo.com', 'Admin.123', 'Admin'),
('usuario@demo.com', 'User.123', 'User');

INSERT INTO Instructores (Nombre, Apellidos, Email, Activo) VALUES
('Juan', 'Perez', 'juan@instructores.com', 1),
('Maria', 'Gomez', 'maria@instructores.com', 1);

INSERT INTO Cursos (Nombre, Categoria, PrecioBase, DuracionHoras, InstructorId, Activo) VALUES
('C# Avanzado', 'Programacion', 199.99, 40, 1, 1),
('SQL Server T-SQL', 'Base de Datos', 99.50, 20, 1, 1),
('Scrum Master', 'Agilidad', 250.00, 15, 2, 1);

INSERT INTO Inscripciones (UsuarioId, CursoId) VALUES
(2, 1),
(2, 2);
GO
