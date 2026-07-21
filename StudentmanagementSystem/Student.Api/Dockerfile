# Multi-stage Dockerfile for Student Management API

# Base runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8043

# SDK stage for building and publishing
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Student.Api/Student.Api.csproj", "Student.Api/"]
COPY ["Student.Core/Student.Core.csproj", "Student.Core/"]
COPY ["Student.Infrastructure/Student.Infrastructure.csproj", "Student.Infrastructure/"]
RUN dotnet restore "./Student.Api/Student.Api.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/Student.Api"
RUN dotnet build "./Student.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Student.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Student.Api.dll"]