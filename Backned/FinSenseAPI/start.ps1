# FinSense API - Docker Startup Script (Windows)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "FinSense API - Docker Setup (Windows)" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is installed
try {
	$dockerVersion = docker --version
	Write-Host "✅ Docker found: $dockerVersion" -ForegroundColor Green
}
catch {
	Write-Host "❌ Docker is not installed. Please install Docker Desktop." -ForegroundColor Red
	Write-Host "   Download from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
	exit 1
}

Write-Host ""

# Check if docker-compose is available
try {
	$composeVersion = docker-compose --version
	Write-Host "✅ Docker Compose found: $composeVersion" -ForegroundColor Green
}
catch {
	Write-Host "❌ Docker Compose is not installed." -ForegroundColor Red
	exit 1
}

Write-Host ""

# Check if .env file exists
if (-not (Test-Path ".env")) {
	Write-Host "⚠️  .env file not found!" -ForegroundColor Yellow
	Write-Host "Creating .env from .env.example..."

	if (-not (Test-Path ".env.example")) {
		Write-Host "❌ .env.example not found!" -ForegroundColor Red
		exit 1
	}

	Copy-Item ".env.example" ".env"
	Write-Host "✅ Created .env file" -ForegroundColor Green
	Write-Host ""
	Write-Host "⚠️  IMPORTANT: Please edit .env and update:" -ForegroundColor Yellow
	Write-Host "   - DB_PASSWORD (PostgreSQL password)" -ForegroundColor Yellow
	Write-Host "   - JWT_SECRET (min 32 characters)" -ForegroundColor Yellow
	Write-Host "   - CLAUDE_API_KEY (your Claude API key)" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "To generate JWT_SECRET, run in PowerShell:" -ForegroundColor Cyan
	Write-Host '   [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString())) | Select-Object -First 32' -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Then run this script again."
	exit 0
}

Write-Host "✅ .env file found" -ForegroundColor Green
Write-Host ""

# Check for required values in .env
Write-Host "Validating .env configuration..." -ForegroundColor Cyan

$envContent = Get-Content ".env" -Raw

if ($envContent -match "CLAUDE_API_KEY=your_claude_api_key_here") {
	Write-Host "❌ CLAUDE_API_KEY not configured in .env" -ForegroundColor Red
	exit 1
}

if ($envContent -match "JWT_SECRET=your_super_secret") {
	Write-Host "⚠️  Warning: Using default JWT_SECRET. This is NOT secure for production!" -ForegroundColor Yellow
}

Write-Host "✅ .env configuration looks good" -ForegroundColor Green
Write-Host ""

# Build and start services
Write-Host "Building Docker images..." -ForegroundColor Cyan
Write-Host ""
docker-compose build --pull

Write-Host ""
Write-Host "Starting services..." -ForegroundColor Cyan
Write-Host ""
docker-compose up -d

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "✅ Services started successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check service health
Write-Host ""
Write-Host "Service Status:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "API Health Check:" -ForegroundColor Cyan
try {
	$response = Invoke-WebRequest -Uri "http://localhost:8080/health" -ErrorAction SilentlyContinue
	if ($response.StatusCode -eq 200) {
		Write-Host "✅ API is healthy and responding" -ForegroundColor Green
	}
}
catch {
	Write-Host "⏳ API is starting up... check logs with: docker-compose logs -f api" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 Useful Commands:" -ForegroundColor Cyan
Write-Host "   View API logs:       docker-compose logs -f api" -ForegroundColor White
Write-Host "   Stop services:       docker-compose stop" -ForegroundColor White
Write-Host "   Restart services:    docker-compose restart" -ForegroundColor White
Write-Host "   Access database:     docker-compose exec postgres psql -U finsense_user -d finsense_db" -ForegroundColor White
Write-Host ""
Write-Host "📚 For more information, see: DOCKER_DEPLOYMENT.md" -ForegroundColor Cyan
Write-Host ""
