# Containerization Lab Log

Commit this project to a GitHub repository, then create a Codespace from that
repository. The `.devcontainer` setup installs the .NET 8 SDK and Docker tools.
In the Codespace terminal, run `cp .env.example .env` and replace the sample
`SA_PASSWORD` in `.env` with a strong password. Keep `.env` private and replace
each checkpoint placeholder below with your actual output; do not paste
passwords or other secrets into this file.

## Part 0 — Environment setup

- `docker --version`:
- `docker compose version`:
- `docker run --rm hello-world` success message:
- `dotnet --version`:

## Part 1 — Prebuilt container

- Image:
- Container name:
- Port mapping:
- Confirmed the nginx welcome page and inspected `docker ps` / `docker logs web`:

## Part 2 — API image

- `docker image ls` result / final `myapi` image size:
- Kestrel startup line from `docker logs myapi`:
- `GET /api/health` result:

## Part 3 — SQL Server with Compose

- `docker compose ps` showing API and healthy database:
- `GET /api/items` result:
- Persistence check after `docker compose down` and `docker compose up`:

## Part 4 — Full stack and scaling

- `docker compose ps` showing nginx, three API replicas, db, and cache:
- `GET /api/health` results (include the instance values):
- Confirmed API requests appear in `docker compose logs -f api`:

The stack has six running containers when the API is scaled to three: nginx,
three API replicas, SQL Server, and Redis.

## Reflection

Write 3–5 sentences about what the multi-stage build and SQL Server health check
provided, and what you learned from scaling the API.
