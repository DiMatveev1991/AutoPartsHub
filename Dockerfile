FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore AutoPartsHub.sln
RUN dotnet publish src/AutoPartsHub.TelegramBot/AutoPartsHub.TelegramBot.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "AutoPartsHub.TelegramBot.dll"]
