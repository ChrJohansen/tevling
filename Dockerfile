# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Spur/Spur.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY Spur ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

VOLUME /app/storage
ENTRYPOINT ["dotnet", "Spur.dll"]
