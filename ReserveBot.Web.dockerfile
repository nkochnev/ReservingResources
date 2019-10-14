FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet restore ReserveBot.Web/ReserveBot.Web.csproj --source https://api.nuget.org/v3/index.json 
RUN dotnet publish ReserveBot.Web/ReserveBot.Web.csproj -c Release -o ../out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ReserveBot.Web.dll"]