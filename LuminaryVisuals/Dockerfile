FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["LuminaryVisuals/LuminaryVisuals.csproj", "LuminaryVisuals/"]
RUN dotnet restore "LuminaryVisuals/LuminaryVisuals.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/LuminaryVisuals"

# Build
RUN dotnet build "LuminaryVisuals.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LuminaryVisuals.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files from build stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PORT=8080

# Start the app
ENTRYPOINT ["dotnet", "LuminaryVisuals.dll"]