#!/bin/bash
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to show usage
usage() {
    echo "Usage: ./build.sh [options]"
    echo "Options:"
    echo "  --docker    Build using Docker (docker-compose)"
    echo "  --help      Show this help message"
}

# Parse arguments
BUILD_DOCKER=false

for arg in "$@"
do
    case $arg in
        --docker)
        BUILD_DOCKER=true
        shift
        ;;
        --help)
        usage
        exit 0
        ;;
        *)
        # Unknown option
        ;;
    esac
done

echo -e "${GREEN}Starting Mini POS Build Process...${NC}"

if [ "$BUILD_DOCKER" = true ]; then
    # Docker Build
    echo -e "${GREEN}Building Docker environment...${NC}"
    
    if ! command -v docker-compose &> /dev/null; then
        echo -e "${RED}Error: docker-compose not found. Please install it first.${NC}"
        exit 1
    fi

    docker-compose build
    echo -e "${GREEN}Docker build completed successfully!${NC}"
else
    # Native .NET Build
    
    # Check for .NET SDK
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}Error: .NET SDK not found. Please install it first.${NC}"
        exit 1
    fi

    # Restore dependencies
    echo -e "${GREEN}Restoring dependencies...${NC}"
    dotnet restore

    # Build
    echo -e "${GREEN}Building project...${NC}"
    dotnet build --no-restore

    echo -e "${GREEN}Native build completed successfully!${NC}"
fi
