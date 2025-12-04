# SEM3-DataServer

Spring Boot data-server til 3. semester projekt.

## Teknologier
- Spring Boot 3.3.5
- gRPC
- JPA/Hibernate
- PostgreSQL
- Protocol Buffers

## Setup

1. Sørg for at PostgreSQL kører
2. Opdater `src/main/resources/application.properties` med dine database credentials
3. Kør `mvn clean install` for at generere gRPC stubs
4. Kør `mvn spring-boot:run` for at starte serveren

gRPC serveren kører på port 9090.

## Database

Forbinder til PostgreSQL database:
- Database: sem3
- User: sem3
- Password: sem3
