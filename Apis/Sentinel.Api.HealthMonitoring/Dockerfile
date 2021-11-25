FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/Apis/Sentinel.Api.HealthMonitoring
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["Apis/Sentinel.Api.HealthMonitoring/Sentinel.Api.HealthMonitoring.csproj", "Apis/Sentinel.Api.HealthMonitoring/"]
RUN dotnet restore "Apis/Sentinel.Api.HealthMonitoring/Sentinel.Api.HealthMonitoring.csproj"
COPY . .
WORKDIR "/src/Apis/Sentinel.Api.HealthMonitoring"
RUN dotnet build "Sentinel.Api.HealthMonitoring.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sentinel.Api.HealthMonitoring.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentinel.Api.HealthMonitoring.dll"]
