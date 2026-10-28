# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SolicitudesApi.csproj ./
RUN dotnet restore SolicitudesApi.csproj

COPY . .
RUN dotnet publish SolicitudesApi.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN useradd -m -u 10001 appuser
USER appuser

COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "SolicitudesApi.dll"]
