# SkillAlexa

API REST para habilidades de Alexa desarrollada en .NET 9.0.

## Requisitos

- .NET 9.0 SDK
- SQL Server

## Instalación

```bash
dotnet restore
dotnet build
```

## Ejecución

```bash
cd SkillAlexa.API
dotnet run
```

## Estructura del proyecto

- **SkillAlexa.API** - Capa de presentación (API REST)
- **SkillAlexa.BC** - Capa de lógica de negocio
- **SkillAlexa.BW** - Capa de workflows de negocio
- **SkillAlexa.DA** - Capa de acceso a datos
