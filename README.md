# ChatGPTcorpus

This repository contains a small Vue frontend and an ASP.NET backend.

## Running with Docker

1. Review the values in `.env` and adjust them if needed (database credentials, ports, passphrase, etc.).
2. Build and launch the stack with `docker compose up --build`.
3. Open the frontend at `http://localhost:5173` (or the port you configured) and access the backend API at `http://localhost:8080`.
4. Stop the stack with `docker compose down` when you are done.

The compose stack starts three services: PostgreSQL, the ASP.NET backend, and the Vite dev server. All runtime configuration is provided through the shared `.env` file so you can tweak the setup from a single place.

## Securing the Frontend

The search page of the frontend can be protected with a shared passphrase. When using Docker, set the `VITE_ACCESS_PASSPHRASE` value in the root `.env` file. If you run the frontend directly with Vite, copy `frontend/.env.example` to `frontend/.env` and change the passphrase there. When enabled, users must enter the passphrase before the `/search` route is displayed.

## Securing the Backend

Set the `AccessPassphrase` configuration value for the ASP.NET backend (for example via the `AccessPassphrase` environment variable, which is wired up to the root `.env` file when using Docker). When set, the search API endpoints require the same passphrase in the `X-Access-Passphrase` request header.

See `frontend/README.md` for details on running the frontend.
