# Plantilla .NET Clean Architecture con Identity y JWT

## Descripción del Repositorio  
**Plantilla de API en .NET con Clean Architecture, Identity y JWT**  
Una solución robusta y escalable que implementa Clean Architecture con autenticación segura mediante ASP.NET Core Identity y JWT. Ideal para iniciar proyectos empresariales con sistema de autenticación completo listo para producción.

## Características Clave ✨
- ✅ **Clean Architecture** con separación clara de capas (Core, Application, Infrastructure, Presentation)
- 🔒 **Autenticación JWT** con refresh tokens y renovación automática
- 👤 Gestión de usuarios con **ASP.NET Core Identity**
- 🔄 Refresh tokens almacenados en **cookies HttpOnly Secure**
- 🧪 Patrón Repository con Unit of Work
- 🐳 Soporte para **Docker** y PostgreSQL
- 📊 Documentación API con Swagger/OpenAPI
- 🛡️ Configuración de seguridad mejorada (CORS, HTTPS, protección CSRF)

## Estructura del Proyecto 🗂️
```
src/
├── Core/              # Modelos de dominio, interfaces, DTOs
├── Application/       # Casos de uso, servicios, validadores
├── Infrastructure/    # Identity, JWT, base de datos, repositorios
├── Presentation/      # Controladores, middleware, configuración
tests/                 # Pruebas unitarias y de integración
```

## Tecnologías Utilizadas 🛠️
- .NET 9
- ASP.NET Core Identity
- Entity Framework Core
- PostgreSQL
- JWT Authentication

## Configuración Rápida ⚡

### Requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Pasos de Instalación
1. Clonar repositorio:
   ```bash
   git clone https://github.com/tu-usuario/tu-repo.git
   cd tu-repo
   ```

2. Configurar base de datos (PostgreSQL con Docker):
   ```bash
   docker run --name myapp-db -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres
   ```

3. Configurar conexión en `appsettings.json`:
   ```json,
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=mysecretpassword;"
   }
   ```

4. Aplicar migraciones:
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Api
   ```

5. Ejecutar la aplicación:
   ```bash
   dotnet run --project src/Api
   ```

## Uso de la API 🔌

### Autenticación
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "P@ssw0rd123",
  "username": "nuevo_usuario"
}
```

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "P@ssw0rd123"
}
```

### Endpoints Protegidos
```http
GET /api/protected
Authorization: Bearer <access_token>
```

## Configuración de Cookies 🍪
Las cookies de refresh token se configuran automáticamente con:
- **HttpOnly**: Protección contra XSS
- **Secure**: Solo se transmiten por HTTPS
- **SameSite=Strict**: Protección contra CSRF
- **Expiración**: 7 días
- **Path restringido**: Solo accesible por endpoints de autenticación


## Contribución 🤝
¡Las contribuciones son bienvenidas! Por favor lee las [guías de contribución](CONTRIBUTING.md) antes de enviar un PR.
