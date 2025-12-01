# SEM3-AppServer

ASP.NET Core app-server til 3. semester projekt.

## Teknologier
- ASP.NET Core 8.0
- Blazor Server
- gRPC Client
- BCrypt for password hashing
- Session management

## Setup

1. Sørg for at DataServer kører på `localhost:9090`
2. Kør `dotnet restore` i AppServer mappen
3. Kør `dotnet run` for at starte serveren

Applikationen kører på:
- HTTP: http://localhost:5126
- HTTPS: https://localhost:7136

## Endpoints

### API
- POST `/api/auth/register` - Registrer ny bruger
- POST `/api/auth/login` - Login

### Pages
- `/register` - Registreringsside
- `/login` - Login side
- `/` - Forside

## Features
- Brugerregistrering med validering
- Login med session management
- gRPC kommunikation med DataServer
- BCrypt password hashing
