FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS develop
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src/TestCases/BaseApp
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/
COPY ["TestCases/BaseApp/BaseApp.csproj", "TestCases/BaseApp/"]
RUN dotnet restore "TestCases/BaseApp/BaseApp.csproj"
COPY . .
WORKDIR "/src/TestCases/BaseApp"
RUN dotnet build "BaseApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BaseApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaseApp.dll"]
