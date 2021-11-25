FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/Workers/Sentinel.Worker.Scaler
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["Workers/Sentinel.Worker.Scaler/Sentinel.Worker.Scaler.csproj", "Workers/Sentinel.Worker.Scaler/"]
RUN dotnet restore "Workers/Sentinel.Worker.Scaler/Sentinel.Worker.Scaler.csproj"
COPY . .
WORKDIR "/src/Workers/Sentinel.Worker.Scaler"
RUN dotnet build "Sentinel.Worker.Scaler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sentinel.Worker.Scaler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentinel.Worker.Scaler.dll"]
