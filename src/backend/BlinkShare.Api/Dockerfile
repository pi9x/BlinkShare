FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY src/backend/BlinkShare.Api/BlinkShare.Api.csproj src/backend/BlinkShare.Api/
RUN dotnet restore src/backend/BlinkShare.Api/BlinkShare.Api.csproj

FROM restore AS build
WORKDIR /src

COPY src/backend/BlinkShare.Api/ src/backend/BlinkShare.Api/

WORKDIR /src/src/backend/BlinkShare.Api
RUN dotnet build BlinkShare.Api.csproj -c Release --no-restore
RUN dotnet publish BlinkShare.Api.csproj -c Release -o /app/publish --no-build
RUN dotnet tool install --tool-path /tools dotnet-ef --version 10.*

FROM build AS migrator
ENV PATH="/tools:${PATH}"
ENTRYPOINT ["dotnet", "ef"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BlinkShare.Api.dll"]
