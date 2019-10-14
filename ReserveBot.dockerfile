FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet restore ReserveBot/ReserveBot.fsproj --source https://api.nuget.org/v3/index.json 
RUN dotnet publish ReserveBot/ReserveBot.fsproj -c Release -o ../out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ReserveBot.dll"]