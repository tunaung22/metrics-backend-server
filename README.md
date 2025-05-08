# Metrics Backend Server

## URL ROUTES

...

### Razor Pages

...

## Environment variable requirements

add following to .env in Metrics.Web directory
or use env.sample as reference

```ini
    ALLOWED_ORIGINS=http://127.0.0.1:5173,http://localhost:5000,https://127.0.0.1:5000,https://localhost:5000
    PG_HOST=[postgres-host]
    PG_PORT=[postgres-port]
    PG_DB_NAME=[postgres-db-name]
    PG_DB_USER=[postgres-username]
    PG_DB_PASSWORD=[postgres-password]
    PG_DB_SCHEMA=[postgresdb-schema-name]
```

## Publish

> dotnet publish -c Release -r win-x64 --self-contained false -o C:\app\publish
> dotnet publish -c Release -r linux-x64 /p:PublishTrimmed=true --version-suffix BETA --output ../release

### Instruction

> cd src\Metrics.Web
> dotnet publish -c Release -r win-x64 --self-contained false -o C:\app\publish
