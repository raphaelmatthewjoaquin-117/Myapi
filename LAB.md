# Containerization Lab Log

Entries marked `Expected` are reference results, not captured output. Replace
them with what you observe before submitting. Version numbers, image sizes,
generated IDs, container names, and replica hostnames vary by machine. In the
Codespace terminal, run `cp .env.example .env` and replace the sample
`SA_PASSWORD` in `.env` with a strong password. Do not commit `.env` or paste
passwords into this file.

## Part 0 — Environment setup

- `docker --version`: `Docker version 29.9.2-2, build a82e528901260c8df6073311379eac65f92db204`.
- `docker compose version`: `Docker Compose version v2.40.3`.
- `docker run --rm hello-world`: Success; printed `Hello from Docker!`.
- `dotnet --version`: `8.0.415`.

## Part 1 — Prebuilt container

- Image: `nginx:latest` (nginx `1.31.8`); container: `web`; port mapping: host `8081` to container `80`.
- `docker ps`: container `web` was `Up` with `0.0.0.0:8081->80/tcp` and `[::]:8081->80/tcp`.
- `docker logs web`: nginx reported `Configuration complete; ready for start up` and started worker processes.
- `curl -I http://localhost:8081`: `HTTP/1.1 200 OK`; `Server: nginx/1.31.8`.
- Browser check: Not recorded.

## Part 2 — API image

- `docker build -t myapi:1.0 .`: succeeded.
- `docker image ls`: `myapi:1.0` showed `340MB` disk usage and `95.2MB` content size; `mcr.microsoft.com/dotnet/sdk:8.0` showed `1.24GB` disk usage and `334MB` content size.
- `docker logs myapi`: `Now listening on: http://[::]:8080`.
- `GET /api/health`: HTTP `200 OK`; response was `{"status":"ok","instance":"13fe684c0d47"}`.

## Part 3 — SQL Server with Compose

- `docker compose up -d --build`: succeeded; API image built and all four services started.
- `docker compose ps`: `myapi-db-1` is `Up (healthy)`; `myapi-api-1`, `myapi-nginx-1`, and `myapi-cache-1` are `Up`. Only nginx publishes a host port (`80:80`).
- `GET /api/items`: HTTP `200 OK` via nginx/1.29.8; returned `[{"id":1,"name":"Notebook"},{"id":2,"name":"Pencil"},{"id":3,"name":"Backpack"}]`.
- Persistence check: after `docker compose down` (without `-v`) and `docker compose up -d`, the database returned healthy and the same three items remained. An immediate request briefly got `502` because nginx reached the API before Kestrel was listening; retrying after API startup returned HTTP `200 OK` with the same rows.

## Part 4 — Full stack and scaling

- `docker compose up -d --build --scale api=3`: succeeded.
- `docker compose ps`: six containers; `myapi-api-1`, `myapi-api-2`, `myapi-api-3`, `myapi-cache-1`, `myapi-db-1` (`healthy`), and `myapi-nginx-1`. Only nginx publishes host port `80`.
- Ten `GET /api/health` requests returned HTTP `200` and showed all three instances: `97c5d4723e80`, `06ef253cb508`, and `d0234a41daef`.
- `docker compose logs --tail=50 api`: showed requests handled by `api-1`, `api-2`, and `api-3`, with each instance ID logged.
- `docker stats --no-stream`: API replicas used about `32–36 MiB` each; nginx `2.5 MiB`; Redis `3.1 MiB`; SQL Server `729.8 MiB`.
- `docker compose down`: removed the six containers and default network; named SQL volume was not removed.

The stack has six running containers when the API is scaled to three: nginx,
three API replicas, SQL Server, and Redis.

## Reflection

docker is a call tool that large companies use for a bunch of stuff such as testing to deployment, but it is a lenghty process to set up, but once that done you just need to execute a few lines of code and you are done

