# FinSense API - Docker Deployment Guide

## Overview
This guide provides instructions for building and running the FinSense API with PostgreSQL using Docker and Docker Compose.

## Prerequisites

- **Docker**: 20.10 or later ([Install Docker Desktop](https://www.docker.com/products/docker-desktop))
- **Docker Compose**: 1.29 or later (included with Docker Desktop)
- **.env file**: Configuration file with your environment variables (see [Configuration](#configuration))

## Quick Start

### 1. Prepare Environment Variables

```bash
# Copy the example environment file
cp .env.example .env

# Edit .env with your actual values
# IMPORTANT: Change sensitive values like DB_PASSWORD and JWT_SECRET
nano .env  # or use your preferred editor
```

**Critical Configuration Values to Update:**

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_PASSWORD` | PostgreSQL password | Generate a strong password |
| `JWT_SECRET` | JWT signing secret (min 32 chars) | Generate using: `openssl rand -base64 32` |
| `CLAUDE_API_KEY` | Your Claude API key from Anthropic | Get from [console.anthropic.com](https://console.anthropic.com) |
| `ALLOWED_ORIGINS` | CORS allowed origins for frontend | `http://localhost:3000` |

### 2. Build and Start Services

```bash
# Build images and start services
docker-compose up -d

# View real-time logs
docker-compose logs -f api

# Check service status
docker-compose ps
```

### 3. Verify Everything is Running

```bash
# Check PostgreSQL health
docker-compose exec postgres pg_isready

# Test API health (should return 200 OK)
curl http://localhost:8080/health

# View API logs
docker-compose logs api
```

## Configuration Guide

### Environment Variables (.env)

#### Database Configuration
```
DB_USER=finsense_user
DB_PASSWORD=your_secure_password_here
DB_NAME=finsense_db
DB_PORT=5432
```

#### API Configuration
```
ASPNETCORE_ENVIRONMENT=Production
API_PORT=8080
API_HTTPS_PORT=8081
```

#### Claude AI Configuration
```
CLAUDE_API_KEY=your_claude_api_key
CLAUDE_MODEL=llama-3.3-70b-versatile
CLAUDE_MAX_RETRIES=2
CLAUDE_TIMEOUT=30
```

#### JWT Configuration
Generate a strong secret:
```bash
# Linux/Mac
openssl rand -base64 32

# Windows PowerShell
[System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString())) | ForEach-Object { $_.Substring(0, 32) }
```

```
JWT_SECRET=your_32_char_minimum_secret_here
JWT_ISSUER=FinSenseAPI
JWT_AUDIENCE=FinSenseClient
JWT_EXPIRY_DAYS=7
JWT_REFRESH_TOKEN_EXPIRY_DAYS=30
```

#### Rate Limiting
```
RATE_LIMIT_REQUESTS=20      # Requests per minute
RATE_LIMIT_CLAUDE=5         # Claude API calls per minute
```

#### CORS Configuration
```
ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001
```

## Common Docker Commands

### View Logs
```bash
# API logs
docker-compose logs api

# Database logs
docker-compose logs postgres

# Follow real-time logs
docker-compose logs -f

# View last 100 lines
docker-compose logs --tail=100
```

### Access Database
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U finsense_user -d finsense_db

# Common psql commands
\dt                    # List tables
\l                     # List databases
SELECT * FROM users;   # Query table
\q                     # Quit
```

### Manage Services
```bash
# Stop services
docker-compose stop

# Restart services
docker-compose restart

# Remove all containers and volumes (CAUTION: deletes data)
docker-compose down -v

# Rebuild images
docker-compose build --no-cache

# Start specific service
docker-compose up -d api
```

### Troubleshooting

#### Service won't start
```bash
# Check for port conflicts
netstat -ano | findstr :8080  # Windows
lsof -i :8080                  # Mac/Linux

# View full error logs
docker-compose logs api
```

#### Database connection issues
```bash
# Verify PostgreSQL is healthy
docker-compose ps postgres

# Check database logs
docker-compose logs postgres

# Manually test connection
docker-compose exec postgres psql -U finsense_user -c "SELECT 1"
```

#### Permission issues
```bash
# Fix volume permissions (Linux)
sudo chown -R 1000:1000 postgres_data

# On Docker Desktop, usually not needed
```

## Running Database Migrations

```bash
# Migrations run automatically on application startup
# If needed to manually run:
docker-compose exec api dotnet ef database update
```

## Health Checks

The docker-compose includes health checks:

```bash
# API endpoint (if configured)
curl http://localhost:8080/health

# Database health check runs automatically
docker-compose ps
```

## Production Deployment

### Before Going to Production

1. **Change all secrets:**
   - Use strong DB password
   - Generate new JWT_SECRET
   - Use production Claude API key

2. **Update allowed origins:**
   ```
   ALLOWED_ORIGINS=https://yourdomain.com
   ```

3. **Enable HTTPS:**
   - Generate SSL certificates
   - Update API container to use HTTPS port
   - Set `ASPNETCORE_ENVIRONMENT=Production`

4. **Database backups:**
   ```bash
   # Backup database
   docker-compose exec postgres pg_dump -U finsense_user finsense_db > backup.sql

   # Restore from backup
   docker-compose exec -T postgres psql -U finsense_user finsense_db < backup.sql
   ```

5. **Resource limits:**
   Update docker-compose.yml to add limits:
   ```yaml
   services:
	 api:
	   deploy:
		 resources:
		   limits:
			 cpus: '1'
			 memory: 512M
   ```

## Docker Image Structure

```
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
  └─ Builds the application

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
  └─ Final image with runtime only (smaller, more secure)
```

## Monitoring

### View resource usage
```bash
docker stats
```

### Container metrics
```bash
docker-compose ps
docker inspect finsense-api
```

## Cleanup

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove everything (careful!)
docker system prune -a
```

## Support & Debugging

For detailed logs with timestamps:
```bash
docker-compose logs --timestamps
```

To enter container shell:
```bash
docker-compose exec api /bin/sh
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker](https://hub.docker.com/_/postgres)
