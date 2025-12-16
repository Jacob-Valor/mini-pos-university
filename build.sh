#!/usr/bin/env bash
set -euo pipefail

# Colors (only when stdout is a TTY)
if [[ -t 1 ]]; then
    GREEN="$(printf '\033[0;32m')"
    RED="$(printf '\033[0;31m')"
    YELLOW="$(printf '\033[0;33m')"
    NC="$(printf '\033[0m')" # No Color
else
    GREEN=''
    RED=''
    YELLOW=''
    NC=''
fi

info() { printf "%b\n" "${GREEN}🚀 $*${NC}"; }
warn() { printf "%b\n" "${YELLOW}⚠️  $*${NC}"; }
success() { printf "%b\n" "${GREEN}✅ $*${NC}"; }
error() { printf "%b\n" "${RED}❌ $*${NC}" >&2; }

# Function to show usage
usage() {
    cat <<'EOF'
Usage: ./build.sh [options]

Options:
  --docker    Build using Docker Compose
  --help      Show this help message
EOF
}

# Parse arguments
BUILD_DOCKER=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --docker)
            BUILD_DOCKER=true
            shift
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

info "Starting Mini POS build..."

if [ "$BUILD_DOCKER" = true ]; then
    # Docker Build
    info "Building Docker environment..."

    if command -v docker-compose &> /dev/null; then
        DOCKER_COMPOSE=(docker-compose)
    elif command -v docker &> /dev/null && docker compose version &> /dev/null; then
        DOCKER_COMPOSE=(docker compose)
    else
        error "Docker Compose not found. Install Docker Desktop / docker-compose."
        exit 1
    fi

    "${DOCKER_COMPOSE[@]}" build
    success "Docker build completed!"
else
    # Native .NET Build

    # Check for .NET SDK
    if ! command -v dotnet &> /dev/null; then
        error ".NET SDK not found. Please install it first."
        exit 1
    fi

    # Restore dependencies
    info "Restoring dependencies..."
    dotnet restore

    # Build
    info "Building project..."
    dotnet build --no-restore

    success "Native build completed!"
fi
