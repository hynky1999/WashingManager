FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# Restore
WORKDIR /src
COPY ["App/App.csproj", "./App.csproj"]
RUN dotnet restore 


# Build
COPY ["App", "."]
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY App/certs ./certs
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.dll"]
