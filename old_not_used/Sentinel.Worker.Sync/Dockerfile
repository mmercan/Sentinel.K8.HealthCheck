FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/Workers/Sentinel.Worker.Sync
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["Workers/Sentinel.Worker.Sync/Sentinel.Worker.Sync.csproj", "Workers/Sentinel.Worker.Sync/"]
RUN dotnet restore "Workers/Sentinel.Worker.Sync/Sentinel.Worker.Sync.csproj"
COPY . .
WORKDIR "/src/Workers/Sentinel.Worker.Sync"
RUN dotnet build "Sentinel.Worker.Sync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sentinel.Worker.Sync.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentinel.Worker.Sync.dll"]
