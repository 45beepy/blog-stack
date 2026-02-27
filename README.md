# Ultra-Light Zero-JS Blog

A full-stack, hyper-minimalist blog architecture prioritizing absolute performance and zero cost. 

## Tech Stack
* **Frontend:** [Astro](https://astro.build/) (Delivers 0kb of JavaScript to the client)
* **Backend:** [.NET 10 Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
* **Database:** [Turso / LibSQL](https://turso.tech/) (SQLite on the Edge)
* **Deployment:** Cloudflare Pages (Frontend) & Render via Docker (Backend)

## Project Structure
This is a monorepo containing both the client and the server code:
* `/frontend` - The Astro static site.
* `/backend` - The .NET Minimal API and Dockerfile.

## Local Development

### Prerequisites
* Node.js & npm
* .NET 10 SDK
* Turso CLI

### 1. Start the Backend
Navigate to the backend folder and start the .NET server:
\`\`\`bash
cd backend
dotnet run
\`\`\`
*(Note: Ensure your Turso URL and Auth Token are configured locally via `dotnet user-secrets` before running).*

### 2. Start the Frontend
In a separate terminal split, navigate to the frontend folder, install dependencies, and start the Astro dev server:
\`\`\`bash
cd frontend
npm install
npm run dev
\`\`\`

The frontend will be available at `http://localhost:4321` and will fetch data from the local .NET API.
