FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/Workers/Sentinel.Worker.Core
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["Workers/Sentinel.Worker.Core/Sentinel.Worker.Core.csproj", "Workers/Sentinel.Worker.Core/"]
RUN dotnet restore "Workers/Sentinel.Worker.Core/Sentinel.Worker.Core.csproj"
COPY . .
WORKDIR "/src/Workers/Sentinel.Worker.Core"
RUN dotnet build "Sentinel.Worker.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sentinel.Worker.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN useradd -r -u 1001 -g root nonroot
RUN chown -R nonroot /app
USER nonroot

ENTRYPOINT ["dotnet", "Sentinel.Worker.Core.dll"]
