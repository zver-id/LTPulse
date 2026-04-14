FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

COPY bin/* .
ENTRYPOINT ["dotnet", "WebAPI.dll"]