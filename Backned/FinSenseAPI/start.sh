#!/bin/bash
# FinSense API - Docker Startup Script

set -e

echo "========================================="
echo "FinSense API - Docker Setup"
echo "========================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
	echo "❌ Docker is not installed. Please install Docker Desktop."
	echo "   Download from: https://www.docker.com/products/docker-desktop"
	exit 1
fi

echo "✅ Docker found: $(docker --version)"
echo ""

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
	if docker compose version &> /dev/null; then
		DOCKER_COMPOSE_CMD="docker compose"
	else
		echo "❌ Docker Compose is not installed."
		exit 1
	fi
else
	DOCKER_COMPOSE_CMD="docker-compose"
fi

echo "✅ Docker Compose found"
echo ""

# Check if .env file exists
if [ ! -f ".env" ]; then
	echo "⚠️  .env file not found!"
	echo "Creating .env from .env.example..."

	if [ ! -f ".env.example" ]; then
		echo "❌ .env.example not found!"
		exit 1
	fi

	cp .env.example .env
	echo "✅ Created .env file"
	echo ""
	echo "⚠️  IMPORTANT: Please edit .env and update:"
	echo "   - DB_PASSWORD (PostgreSQL password)"
	echo "   - JWT_SECRET (min 32 characters - use: openssl rand -base64 32)"
	echo "   - CLAUDE_API_KEY (your Claude API key)"
	echo ""
	echo "Then run this script again."
	exit 0
fi

echo "✅ .env file found"
echo ""

# Check for required values in .env
echo "Validating .env configuration..."

if grep -q "CLAUDE_API_KEY=your_claude_api_key_here" ".env"; then
	echo "❌ CLAUDE_API_KEY not configured in .env"
	exit 1
fi

if grep -q "JWT_SECRET=your_super_secret" ".env"; then
	echo "⚠️  Warning: Using default JWT_SECRET. This is NOT secure for production!"
fi

echo "✅ .env configuration looks good"
echo ""

# Build and start services
echo "Building Docker images..."
echo ""
$DOCKER_COMPOSE_CMD build --pull

echo ""
echo "Starting services..."
echo ""
$DOCKER_COMPOSE_CMD up -d

echo ""
echo "========================================="
echo "✅ Services started successfully!"
echo "========================================="
echo ""
echo "Waiting for services to be ready..."
sleep 5

# Check service health
echo ""
echo "Service Status:"
$DOCKER_COMPOSE_CMD ps

echo ""
echo "API Health Check:"
if curl -s http://localhost:8080/health > /dev/null 2>&1; then
	echo "✅ API is healthy and responding"
else
	echo "⏳ API is starting up... check logs with: $DOCKER_COMPOSE_CMD logs -f api"
fi

echo ""
echo "📋 Useful Commands:"
echo "   View API logs:       $DOCKER_COMPOSE_CMD logs -f api"
echo "   Stop services:       $DOCKER_COMPOSE_CMD stop"
echo "   Restart services:    $DOCKER_COMPOSE_CMD restart"
echo "   Access database:     $DOCKER_COMPOSE_CMD exec postgres psql -U finsense_user -d finsense_db"
echo ""
echo "📚 For more information, see: DOCKER_DEPLOYMENT.md"
echo ""
