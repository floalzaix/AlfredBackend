FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend.sln ./
COPY backend.api/backend.api.csproj backend.api/
COPY backend.bll/backend.bll.csproj backend.bll/
COPY backend.dal/backend.dal.csproj backend.dal/
COPY backend.models/backend.models.csproj backend.models/
COPY Backend.Tests/Backend.Tests.csproj Backend.Tests/
RUN dotnet restore backend.api/backend.api.csproj
COPY . .
RUN dotnet publish backend.api/backend.api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "backend.api.dll"]
