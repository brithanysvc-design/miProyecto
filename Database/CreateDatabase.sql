-- SQL Server

-- Crear la base de datos
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AlexaShoppingListDB')
BEGIN
    CREATE DATABASE AlexaShoppingListDB;
END
GO

USE AlexaShoppingListDB;
GO

-- Tabla de Listas de Compras
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ListasCompra')
BEGIN
    CREATE TABLE ListasCompra (
        IdLista UNIQUEIDENTIFIER PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        FechaObjetivo DATE NOT NULL,
        Estado INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FechaModificacion DATETIME2 NULL,
        CONSTRAINT CK_ListasCompra_Estado CHECK (Estado IN (1, 2))
    );

    -- Índices
    CREATE INDEX IX_ListasCompra_Nombre_Fecha_Estado 
        ON ListasCompra(Nombre, FechaObjetivo, Estado);
    
    CREATE INDEX IX_ListasCompra_FechaObjetivo 
        ON ListasCompra(FechaObjetivo);
END
GO

-- Tabla de Items de Lista
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemsLista')
BEGIN
    CREATE TABLE ItemsLista (
        IdItem UNIQUEIDENTIFIER PRIMARY KEY,
        IdLista UNIQUEIDENTIFIER NOT NULL,
        NombreProducto NVARCHAR(200) NOT NULL,
        Cantidad DECIMAL(10,2) NOT NULL,
        Unidad NVARCHAR(50) NULL,
        Estado INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FechaModificacion DATETIME2 NULL,
        CONSTRAINT FK_ItemsLista_ListasCompra 
            FOREIGN KEY (IdLista) REFERENCES ListasCompra(IdLista) ON DELETE CASCADE,
        CONSTRAINT CK_ItemsLista_Estado CHECK (Estado IN (1, 2)),
        CONSTRAINT CK_ItemsLista_Cantidad CHECK (Cantidad > 0)
    );

    -- Índices
    CREATE INDEX IX_ItemsLista_IdLista 
        ON ItemsLista(IdLista);
    
    CREATE INDEX IX_ItemsLista_IdLista_Estado 
        ON ItemsLista(IdLista, Estado);
END
GO

-- Datos de ejemplo para pruebas
-- Lista 1
DECLARE @IdLista1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO ListasCompra (IdLista, Nombre, FechaObjetivo, Estado, FechaCreacion)
VALUES (@IdLista1, 'Supermercado', CAST(GETDATE() AS DATE), 1, GETUTCDATE());

INSERT INTO ItemsLista (IdItem, IdLista, NombreProducto, Cantidad, Unidad, Estado, FechaCreacion)
VALUES 
    (NEWID(), @IdLista1, 'Leche', 2, 'litros', 1, GETUTCDATE()),
    (NEWID(), @IdLista1, 'Pan', 1, 'unidad', 1, GETUTCDATE()),
    (NEWID(), @IdLista1, 'Huevos', 12, 'unidades', 1, GETUTCDATE());

-- Lista 2
DECLARE @IdLista2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO ListasCompra (IdLista, Nombre, FechaObjetivo, Estado, FechaCreacion)
VALUES (@IdLista2, 'Farmacia', CAST(GETDATE() AS DATE), 1, GETUTCDATE());

INSERT INTO ItemsLista (IdItem, IdLista, NombreProducto, Cantidad, Unidad, Estado, FechaCreacion)
VALUES 
    (NEWID(), @IdLista2, 'Aspirinas', 1, 'caja', 1, GETUTCDATE()),
    (NEWID(), @IdLista2, 'Vitaminas', 1, 'frasco', 1, GETUTCDATE());

GO

PRINT 'Base de datos creada exitosamente';
