# Use .NET 10 runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Use .NET 10 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["mini_pos.csproj", "./"]
RUN dotnet restore "./mini_pos.csproj"

# Copy the rest of the files and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "mini_pos.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mini_pos.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install necessary libraries for Avalonia on Linux
RUN apt-get update && apt-get install -y \
    libx11-6 \
    libxrandr2 \
    libxinerama1 \
    libxcursor1 \
    libxi6 \
    libgl1-mesa-glx \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "mini_pos.dll"]