# SmallEngineCS
Pequeña prueba de motorGrafico con C#

## Instalación de dependencias

Restaurar los paquetes NuGet del proyecto:
```powershell
dotnet restore
```

## Comandos

### Build
Compila el proyecto:
```powershell
dotnet build
```

Compilar en modo Release (optimizado):
```powershell
dotnet build -c Release
```

### Run
Ejecuta la aplicación:
```powershell
dotnet run
```

Ejecutar proyecto específico:
```powershell
dotnet run --project CoreEngine/CoreEngine.csproj
```

### Test
Ejecutar tests unitarios:
```powershell
dotnet test
```

Ejecutar tests con salida detallada:
```powershell
dotnet test --verbosity detailed
```
