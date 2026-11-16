# Сборка из папки Backend:  docker build -t behaatel-api Backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src ./src
RUN dotnet publish src/BehaAtel.WebApi/BehaAtel.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
# Render передаёт порт в переменной PORT
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} exec dotnet BehaAtel.WebApi.dll"]
