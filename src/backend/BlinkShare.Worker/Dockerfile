FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY src/backend/BlinkShare.Api/BlinkShare.Api.csproj src/backend/BlinkShare.Api/
COPY src/backend/BlinkShare.Worker/BlinkShare.Worker.csproj src/backend/BlinkShare.Worker/
RUN dotnet restore src/backend/BlinkShare.Worker/BlinkShare.Worker.csproj

FROM restore AS build
WORKDIR /src

COPY src/backend/BlinkShare.Api/ src/backend/BlinkShare.Api/
COPY src/backend/BlinkShare.Worker/ src/backend/BlinkShare.Worker/

WORKDIR /src/src/backend/BlinkShare.Worker
RUN dotnet build BlinkShare.Worker.csproj -c Release --no-restore
RUN dotnet publish BlinkShare.Worker.csproj -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "BlinkShare.Worker.dll"]
