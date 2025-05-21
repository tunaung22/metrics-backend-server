# Metrics Backend Server

## Environment variables

add following to .env in Metrics.Web directory
or use env.sample as reference

```ini
    ALLOWED_ORIGINS=http://127.0.0.1:5100,http://localhost:5100,https://127.0.0.1:5100,https://localhost:5100
    PG_HOST=[postgres-host]
    PG_PORT=[postgres-port]
    PG_DB_NAME=[postgres-db-name]
    PG_DB_USER=[postgres-username]
    PG_DB_PASSWORD=[postgres-password]
    PG_DB_SCHEMA=[postgresdb-schema-name]
```

## Development environment setup

1. Create a copy of `env.sample` (src/Metrics.Web/env.sample) file and rename as `.env`
2. Modify the content with your environment.
3. Create a database with a name as the same as the PG_DB_NAME value in .env
4. In terminal, run `dotnet run migratedb` to run the migrations.
5. In terminal, run `dotnet run inituser` to create initial user (follow the instructions in the console).
6. In terminal, run `dotnet watch` to start the development server.

## Publish

- Configuration: `-c Release`
- Windows x64: `-r win-x64`
- Linux x64: `-r linux-x64`
- `--self-contained false`
- Output directory: ` -o C:\app\publish`
- Version suffix: `--version-suffix BETA`

### Instruction

#### Build a release

> cd src\Metrics.Web \
> dotnet publish -c Release -r win-x64 --self-contained false -o C:\app\publish
