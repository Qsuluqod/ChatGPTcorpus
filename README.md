# ChatGPTcorpus

This repository contains a small Vue frontend and an ASP.NET backend.

## Securing the Frontend

The search page of the frontend can be protected with a shared passphrase. Copy `frontend/.env.example` to `frontend/.env` and change the value of `VITE_ACCESS_PASSPHRASE` to the passphrase you want to require. When enabled, users must enter the passphrase before the `/search` route is displayed.

## Securing the Backend

Set the `AccessPassphrase` configuration value for the ASP.NET backend (for example via the `AccessPassphrase` environment variable). When set, the search API endpoints require the same passphrase in the `X-Access-Passphrase` request header.

See `frontend/README.md` for details on running the frontend.
