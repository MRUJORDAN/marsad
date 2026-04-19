FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MorsadBackend.Api.csproj .
RUN dotnet restore MorsadBackend.Api.csproj

COPY . .
RUN dotnet publish MorsadBackend.Api.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MorsadBackend.Api.dll"]
