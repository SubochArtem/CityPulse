FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY src/Backend/Services/Feedback/ src/Backend/Services/Feedback/
RUN --mount=type=secret,id=github_token \
    --mount=type=cache,target=/root/.nuget/packages \
    GITHUB_TOKEN="$(cat /run/secrets/github_token)" \
    dotnet publish src/Backend/Services/Feedback/Feedback.API/Feedback.API.csproj \
        --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Feedback.API.dll"]
