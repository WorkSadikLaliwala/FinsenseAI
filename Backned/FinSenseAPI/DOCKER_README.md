# FinSense API - Docker Quick Reference

## 🚀 Quick Start (60 seconds)

### Windows PowerShell
```powershell
# Make sure you're in the FinSenseAPI directory
cd Backned\FinSenseAPI

# Run the startup script
.\start.ps1
```

### Mac/Linux
```bash
cd Backned/FinSenseAPI
chmod +x start.sh
./start.sh
```

### Manual Setup
```bash
# 1. Copy environment template
cp .env.example .env

# 2. Edit .env with your values (see DOCKER_DEPLOYMENT.md)
nano .env

# 3. Start services
docker-compose up -d

# 4. Check status
docker-compose ps
```

## ✅ Verify It Works

```bash
# Check if API is responding
curl http://localhost:8080/health

# View logs
docker-compose logs -f api

# See all services
docker-compose ps
```

## 🔧 Essential Commands

| Command | Purpose |
|---------|---------|
| `docker-compose up -d` | Start all services in background |
| `docker-compose down` | Stop and remove containers |
| `docker-compose ps` | Show service status |
| `docker-compose logs -f api` | View API logs in real-time |
| `docker-compose logs -f postgres` | View database logs in real-time |
| `docker-compose restart` | Restart all services |
| `docker-compose build --no-cache` | Rebuild images |

## 🗄️ Database Access

```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U finsense_user -d finsense_db

# Common commands in psql:
\dt              # List tables
SELECT * FROM users;  # Query
\q               # Quit
```

## 📁 File Structure

```
Backned/FinSenseAPI/
├── docker-compose.yml          # Service orchestration
├── .env.example                # Environment variable template
├── DOCKER_DEPLOYMENT.md        # Full deployment guide
├── start.sh                    # Linux/Mac startup script
├── start.ps1                   # Windows startup script
├── FinSenseAPI/
│   ├── Dockerfile             # API container image
│   ├── appsettings.Production.json
│   ├── appsettings.Development.json
│   └── FinSenseAPI.csproj
└── .dockerignore
```

## 🐛 Troubleshooting

### Port already in use
```bash
# Windows - Find process using port 8080
netstat -ano | findstr :8080

# Mac/Linux
lsof -i :8080

# Change port in .env or docker-compose.yml
```

### Database connection failed
```bash
# Check if PostgreSQL is running
docker-compose logs postgres

# Verify health
docker-compose exec postgres pg_isready

# Test connection manually
docker-compose exec postgres psql -U finsense_user -d finsense_db -c "SELECT 1"
```

### API won't start
```bash
# Check detailed logs
docker-compose logs api

# Rebuild images
docker-compose build --no-cache

# Restart
docker-compose up -d api
```

### Delete everything and start fresh
```bash
# WARNING: This deletes all data!
docker-compose down -v
docker-compose up -d
```

## 📊 Monitor Services

```bash
# Real-time resource usage
docker stats

# Container inspection
docker inspect finsense-api
docker inspect finsense-postgres
```

## 🔐 Security Reminders

- ✅ **Always change** `DB_PASSWORD` and `JWT_SECRET` in .env
- ✅ **Never commit** .env files (already in .gitignore)
- ✅ **Use strong secrets** - min 32 characters for JWT_SECRET
- ✅ **Update ALLOWED_ORIGINS** for your domain
- ✅ Use HTTPS in production (requires certificates)

## 📚 For More Details

See **DOCKER_DEPLOYMENT.md** for:
- Complete environment variable reference
- Production deployment checklist
- Database backup/restore procedures
- SSL/HTTPS configuration
- Resource limits and optimization
- Health checks and monitoring

## 🆘 Need Help?

1. Check logs: `docker-compose logs api`
2. Check health: `docker-compose ps`
3. Review: DOCKER_DEPLOYMENT.md
4. Debug: `docker-compose exec api /bin/sh`

---

**Status**: Ready for development and production deployment ✅
