FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj .
RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet build

FROM build AS publish
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-runtime AS runtime
COPY --from=publish /app/out ./

ENTRYPOINT ["dotnet", "docker-backups.dll"]