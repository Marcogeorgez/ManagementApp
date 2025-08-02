FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ManagementApp/ManagementApp.csproj", "ManagementApp/"]
RUN dotnet restore "ManagementApp/ManagementApp.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/ManagementApp"

# Build
RUN dotnet build "ManagementApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ManagementApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files from build stage
COPY --from=publish /app/publish .

# Set environment variables for GC tuning
# Using workstation GC (more aggressive about releasing memory)
ENV DOTNET_gcServer=0
# Force immediate memory return to OS
ENV DOTNET_GCRetainVM=0
# Enable conservative memory mode (releases memory more aggressively)
ENV DOTNET_GCConserveMemory=7

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PORT=8080

# Start the app
ENTRYPOINT ["dotnet", "ManagementApp.dll"]
