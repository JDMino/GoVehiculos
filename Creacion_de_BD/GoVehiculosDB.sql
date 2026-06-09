----------- ==========================================
-- 1. MÓDULO GEOGRÁFICO Y DIRECCIONES
-- ==========================================

CREATE TABLE Provincia (
    id_provincia INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE Localidad (
    id_localidad INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    provincia_id INT NOT NULL,
    CONSTRAINT FK_Localidad_Provincia FOREIGN KEY (provincia_id) REFERENCES Provincia(id_provincia)
);
GO

CREATE TABLE Direccion (
    id_direccion INT IDENTITY(1,1) PRIMARY KEY,
    calle NVARCHAR(150) NOT NULL, 
    numero INT NOT NULL,
    piso_depto NVARCHAR(50) NULL,
    codigo_postal NVARCHAR(20) NULL,
    localidad_id INT NOT NULL,
    CONSTRAINT FK_Direccion_Localidad FOREIGN KEY (localidad_id) REFERENCES Localidad(id_localidad)
);
GO

CREATE TABLE Ubicacion (
    id_ubicacion INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    direccion_id INT NOT NULL,
    CONSTRAINT FK_Ubicacion_Direccion FOREIGN KEY (direccion_id) REFERENCES Direccion(id_direccion)
);
GO

-- ==========================================
-- 2. MÓDULO DE USUARIOS
-- ==========================================

CREATE TABLE Rol (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Usuario (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    apellido NVARCHAR(100) NOT NULL,
    dni NVARCHAR(20) NOT NULL UNIQUE,
    email NVARCHAR(150) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    telefono NVARCHAR(20) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT GETDATE(),
    activo BIT NOT NULL DEFAULT 1,
    bloqueado BIT NOT NULL DEFAULT 0,
    rol_id INT NOT NULL,
    direccion_id INT NULL,
    verificado BIT NOT NULL DEFAULT 1,
    fecha_baja DATETIME NULL,
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (rol_id) REFERENCES Rol(id_rol),
    CONSTRAINT FK_Usuario_Direccion FOREIGN KEY (direccion_id) REFERENCES Direccion(id_direccion)
);
GO

-- ==========================================
-- 3. MÓDULO DE VEHÍCULOS
-- ==========================================

CREATE TABLE Marca (
    id_marca INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE Modelo (
    id_modelo INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    marca_id INT NOT NULL,
    CONSTRAINT FK_Modelo_Marca FOREIGN KEY (marca_id) REFERENCES Marca(id_marca)
);
GO

CREATE TABLE Vehiculo (
    id_vehiculo INT IDENTITY(1,1) PRIMARY KEY,
    socio_id INT NULL,
    patente NVARCHAR(20) NOT NULL UNIQUE,
    anio INT NOT NULL,
    tipo NVARCHAR(50) NOT NULL,
    estado NVARCHAR(50) NOT NULL DEFAULT 'disponible',
    estado_mecanico NVARCHAR(50) NOT NULL DEFAULT 'bueno',
    kilometraje DECIMAL(10,2) NOT NULL DEFAULT 0,
    precio_por_dia DECIMAL(10,2) NOT NULL,
    mantenimiento_a_cargo_de NVARCHAR(50) NOT NULL,
    seguro_vigente BIT NOT NULL DEFAULT 1,
    documentacion_vigente BIT NOT NULL DEFAULT 1,
    activo BIT NOT NULL DEFAULT 1,
    imagen_url NVARCHAR(255) NULL,
    modelo_id INT NOT NULL,
    ubicacion_actual_id INT NULL,
    CONSTRAINT FK_Vehiculo_Modelo FOREIGN KEY (modelo_id) REFERENCES Modelo(id_modelo),
    CONSTRAINT FK_Vehiculo_Ubicacion FOREIGN KEY (ubicacion_actual_id) REFERENCES Ubicacion(id_ubicacion),
    CONSTRAINT FK_Vehiculo_Socio FOREIGN KEY (socio_id) REFERENCES Usuario(id_usuario),
    CONSTRAINT CHK_Vehiculo_Estado CHECK (estado IN ('disponible','reservado','en_uso','mantenimiento','fuera_de_servicio')),
    CONSTRAINT CHK_Vehiculo_Mantenimiento CHECK (mantenimiento_a_cargo_de IN ('empresa','socio'))
);
GO

-- ==========================================
-- MANTENIMIENTO
-- ==========================================

CREATE TABLE Mantenimiento (
    id_mantenimiento INT IDENTITY(1,1) PRIMARY KEY,
    vehiculo_id INT NOT NULL,
    empleado_id INT NULL,
    tipo NVARCHAR(30) NOT NULL,
    descripcion NVARCHAR(500) NOT NULL,
    estado NVARCHAR(20) NOT NULL DEFAULT 'pendiente',
    prioridad NVARCHAR(20) NOT NULL DEFAULT 'media',
    fecha_programada DATE NULL,
    fecha_realizacion DATE NULL,
    costo DECIMAL(10,2) NOT NULL DEFAULT 0,
    realizado_por NVARCHAR(20) NOT NULL,
    disponibilizado BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Mantenimiento_Vehiculo FOREIGN KEY (vehiculo_id) REFERENCES Vehiculo(id_vehiculo),
    CONSTRAINT FK_Mantenimiento_Usuario FOREIGN KEY (empleado_id) REFERENCES Usuario(id_usuario)
);
GO

UPDATE Mantenimiento SET disponibilizado = 1 WHERE estado = 'finalizado';
GO


-- ==========================================
-- 4. MÓDULO DE MULTAS
-- ==========================================

CREATE TABLE Incidencia (
    id_incidencia INT IDENTITY(1,1) PRIMARY KEY,
    usuario_id INT NOT NULL,
    vehiculo_id INT NOT NULL,
    tipo NVARCHAR(50) NOT NULL,
    descripcion NVARCHAR(MAX) NOT NULL,
    nivel_gravedad NVARCHAR(20) NOT NULL DEFAULT 'media',
    fecha_reporte DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Incidencia_Usuario FOREIGN KEY (usuario_id) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Incidencia_Vehiculo FOREIGN KEY (vehiculo_id) REFERENCES Vehiculo(id_vehiculo)
);
GO

CREATE TABLE Multa (
    id_multa INT IDENTITY(1,1) PRIMARY KEY,
    incidencia_id INT NOT NULL UNIQUE,
    tipo NVARCHAR(50) NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    estado NVARCHAR(50) NOT NULL DEFAULT 'pendiente',
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Multa_Incidencia FOREIGN KEY (incidencia_id) REFERENCES Incidencia(id_incidencia)
);
GO

CREATE TABLE Penalizacion (
    id_penalizacion INT IDENTITY(1,1) PRIMARY KEY,
    multa_id INT NULL,
    tipo NVARCHAR(50) NOT NULL,
    motivo NVARCHAR(255) NOT NULL,
    fecha_inicio DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_fin DATETIME NULL,
    estado NVARCHAR(50) NOT NULL DEFAULT 'activa',
    CONSTRAINT FK_Penalizacion_Multa FOREIGN KEY (multa_id) REFERENCES Multa(id_multa)
);
GO

-- ==========================================
-- ROLES
-- ==========================================
INSERT INTO Rol (nombre) VALUES 
('Cliente'),
('Socio'),
('Empleado'),
('Administrador');
GO

-- ==========================================
-- DATOS DE PRUEBA
-- ==========================================

-- Provincias
INSERT INTO Provincia (nombre) VALUES
('Chaco'),
('Corrientes'),
('Buenos Aires');

-- Localidades
INSERT INTO Localidad (nombre, provincia_id) VALUES
('Barranqueras', 1),
('Resistencia', 1),
('Corrientes Capital', 2),
('La Plata', 3);

-- Direcciones
INSERT INTO Direccion (calle, numero, piso_depto, codigo_postal, localidad_id) VALUES
('Av. 9 de Julio', 100, NULL, '3500', 2),
('San Martín', 250, '1A', '3503', 1),
('Belgrano', 500, NULL, '3400', 3),
('Diagonal 80', 1200, NULL, '1900', 4);

-- Marcas
INSERT INTO Marca (nombre) VALUES
('Toyota'),
('Ford'),
('Chevrolet'),
('Volkswagen');

-- Modelos
INSERT INTO Modelo (nombre, marca_id) VALUES
('Corolla', 1),
('Hilux', 1),
('Fiesta', 2),
('Focus', 2),
('Onix', 3),
('Cruze', 3),
('Golf', 4),
('Polo', 4);

-- Ubicaciones
INSERT INTO Ubicacion (nombre, direccion_id) VALUES
('Sucursal Centro', 1),
('Sucursal Norte', 2),
('Sucursal Este', 3),
('Sucursal Sur', 4);

-- Vehículos
INSERT INTO Vehiculo (
    socio_id, patente, anio, tipo, estado, estado_mecanico,
    kilometraje, precio_por_dia, mantenimiento_a_cargo_de,
    seguro_vigente, documentacion_vigente, activo,
    imagen_url, modelo_id, ubicacion_actual_id
) VALUES
(NULL, 'ABC123', 2022, 'SUV', 'disponible', 'bueno', 15000, 12000, 'empresa', 1, 1, 1, NULL, 2, 1),
(NULL, 'XYZ789', 2021, 'Sedan', 'disponible', 'bueno', 30000, 10000, 'empresa', 1, 1, 1, NULL, 1, 2),
(NULL, 'LMN456', 2023, 'Pickup', 'mantenimiento', 'regular', 5000, 18000, 'empresa', 1, 1, 1, NULL, 2, 3),
(NULL, 'QWE321', 2020, 'Hatchback', 'reservado', 'bueno', 45000, 8000, 'socio', 1, 1, 1, NULL, 5, 4),
(NULL, 'JKL654', 2024, 'Sedan', 'disponible', 'bueno', 0, 15000, 'empresa', 1, 1, 1, NULL, 6, 1),
(NULL, 'POI987', 2019, 'SUV', 'fuera_de_servicio', 'malo', 60000, 7000, 'socio', 0, 0, 1, NULL, 7, 2);
GO
