#!/usr/bin/env bash
# ==============================================================================
# 🛠️ Mini POS System Build Script
# ==============================================================================
set -euo pipefail

# ------------------------------------------------------------------------------
# 🎨 Formatting & Colors
# ------------------------------------------------------------------------------
if [[ -t 1 ]]; then
    BOLD="$(printf '\033[1m')"
    GREEN="$(printf '\033[32m')"
    RED="$(printf '\033[31m')"
    YELLOW="$(printf '\033[33m')"
    BLUE="$(printf '\033[34m')"
    NC="$(printf '\033[0m')" # No Color
else
    BOLD="" GREEN="" RED="" YELLOW="" BLUE="" NC=""
fi

# ------------------------------------------------------------------------------
# 📝 Logging Functions
# ------------------------------------------------------------------------------
info()    { printf "%b\n" "${BLUE}ℹ️  $*${NC}"; }
success() { printf "%b\n" "${GREEN}✅ $*${NC}"; }
warn()    { printf "%b\n" "${YELLOW}⚠️  $*${NC}"; }
error()   { printf "%b\n" "${RED}❌ $*${NC}" >&2; }
header()  { printf "\n%b\n" "${BOLD}== $* ==${NC}"; }

# ------------------------------------------------------------------------------
# 📖 Usage
# ------------------------------------------------------------------------------
usage() {
    cat <<EOF
${BOLD}Usage:${NC} ./build.sh [options]

${BOLD}Options:${NC}
  --clean           Clean artifacts before building
  --release         Build in Release configuration (default is Debug)
  --docker          Build using Docker Compose
  --help, -h        Show this help message

${BOLD}Examples:${NC}
  ./build.sh --clean --release    # Clean build in Release mode
  ./build.sh --docker             # Build Docker containers
EOF
}

# ------------------------------------------------------------------------------
# 🔧 Main Execution
# ------------------------------------------------------------------------------

# Defaults
BUILD_DOCKER=false
DO_CLEAN=false
CONFIGURATION="Debug"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --docker)
            BUILD_DOCKER=true
            shift
            ;;
        --clean)
            DO_CLEAN=true
            shift
            ;;
        --release)
            CONFIGURATION="Release"
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

header "🚀 Starting Mini POS Build Process"

# ------------------------------------------------------------------------------
# 🐳 Docker Build Strategy
# ------------------------------------------------------------------------------
if [ "$BUILD_DOCKER" = true ]; then
    info "Strategy: Docker Compose"

    # Detect Docker Compose command
    if command -v docker-compose &> /dev/null; then
        DOCKER_COMPOSE=(docker-compose)
    elif command -v docker &> /dev/null && docker compose version &> /dev/null; then
        DOCKER_COMPOSE=(docker compose)
    else
        error "Docker Compose not found. Please install Docker Desktop or docker-compose."
        exit 1
    fi

    # Clean if requested
    if [ "$DO_CLEAN" = true ]; then
        info "Cleaning Docker containers and artifacts..."
        "${DOCKER_COMPOSE[@]}" down --rmi local --volumes --remove-orphans
    fi

    # Build
    info "Building containers..."
    "${DOCKER_COMPOSE[@]}" build

    success "Docker build completed successfully!"

# ------------------------------------------------------------------------------
# 🖥️ Native .NET Build Strategy
# ------------------------------------------------------------------------------
else
    info "Strategy: Native .NET SDK ($CONFIGURATION)"

    # Check for .NET SDK
    if ! command -v dotnet &> /dev/null; then
        error ".NET SDK not found. Please install it first: https://dotnet.microsoft.com/download"
        exit 1
    fi

    # Clean if requested
    if [ "$DO_CLEAN" = true ]; then
        info "Cleaning project..."
        dotnet clean --configuration "$CONFIGURATION" --verbosity quiet
    fi

    # Restore
    info "Restoring dependencies..."
    dotnet restore --verbosity minimal

    # Build
    info "Compiling project..."
    dotnet build --no-restore --configuration "$CONFIGURATION"

    success "Native build ($CONFIGURATION) completed successfully!"
fi
