FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Backend/Gateway/ src/Backend/Gateway/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/Backend/Gateway/Gateway.API/Gateway.API.csproj \
        --source https://api.nuget.org/v3/index.json && \
    dotnet publish src/Backend/Gateway/Gateway.API/Gateway.API.csproj \
        --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
