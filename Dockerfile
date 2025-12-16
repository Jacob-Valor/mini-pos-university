# ================================================================================================
# Mini POS - Multi-stage Dockerfile
# ================================================================================================
# This Dockerfile builds the Mini POS Avalonia desktop application for containerized deployment.
# It uses multi-stage builds to optimize the final image size.
#
# Build: docker build -t mini-pos .
# Run:   docker run --rm -e DISPLAY=$DISPLAY -v /tmp/.X11-unix:/tmp/.X11-unix mini-pos
# ================================================================================================

# ================================================================================================
# Stage 1: Base Runtime Image
# ================================================================================================
# This stage sets up the base .NET runtime environment for the final application
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
LABEL stage=base
LABEL description="Base runtime environment for Mini POS application"

WORKDIR /app

# ================================================================================================
# Stage 2: Build Environment
# ================================================================================================
# This stage contains the full .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
LABEL stage=build
LABEL description="Build environment with .NET SDK"

WORKDIR /src

# Copy project file and restore dependencies
# This is done separately to leverage Docker layer caching
COPY ["mini_pos.csproj", "./"]
RUN dotnet restore "./mini_pos.csproj"

# Copy all source files and build the application
COPY . .
WORKDIR "/src/."
RUN dotnet build "mini_pos.csproj" \
    -c Release \
    -o /app/build

# ================================================================================================
# Stage 3: Publish
# ================================================================================================
# This stage publishes the application in release mode
FROM build AS publish
LABEL stage=publish
LABEL description="Publish stage for optimized build output"

RUN dotnet publish "mini_pos.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ================================================================================================
# Stage 4: Final Runtime Image
# ================================================================================================
# This stage creates the minimal final image with only runtime dependencies
FROM base AS final
LABEL stage=final
LABEL description="Final production-ready Mini POS image"
LABEL maintainer="Mini POS Development Team"
LABEL version="1.0"

WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# ================================================================================================
# Install Avalonia Dependencies for Linux
# ================================================================================================
# Avalonia UI requires X11 libraries and graphics support to run on Linux
# These packages provide the necessary graphical display capabilities
RUN apt-get update && apt-get install -y \
    # X11 core libraries
    libx11-6 \
    # Multi-monitor support
    libxrandr2 \
    libxinerama1 \
    # Cursor and input support
    libxcursor1 \
    libxi6 \
    # OpenGL support
    libgl1 \
    # Font rendering
    fontconfig \
    libfreetype6 \
    fonts-liberation \
    # Clean up apt cache to reduce image size
    && rm -rf /var/lib/apt/lists/*

# ================================================================================================
# Application Startup
# ================================================================================================
# Start the Mini POS application
# Note: For GUI applications, ensure DISPLAY environment variable is set
ENTRYPOINT ["dotnet", "mini_pos.dll"]