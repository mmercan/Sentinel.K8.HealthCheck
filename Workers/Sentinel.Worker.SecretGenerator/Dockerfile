FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/Workers/Sentinel.Worker.SecretGenerator
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["Workers/Sentinel.Worker.SecretGenerator/Sentinel.Worker.SecretGenerator.csproj", "Workers/Sentinel.Worker.SecretGenerator/"]
RUN dotnet restore "Workers/Sentinel.Worker.SecretGenerator/Sentinel.Worker.SecretGenerator.csproj"
COPY . .
WORKDIR "/src/Workers/Sentinel.Worker.SecretGenerator"
RUN dotnet build "Sentinel.Worker.SecretGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sentinel.Worker.SecretGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentinel.Worker.SecretGenerator.dll"]
