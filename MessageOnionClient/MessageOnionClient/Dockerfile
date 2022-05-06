FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../MessageOnionClient/MessageOnionClient/MessageOnionClient.csproj", "MessageOnionClient/"]
RUN dotnet restore "../MessageOnionClient/MessageOnionClient/MessageOnionClient.csproj"
COPY . .
WORKDIR "/src/MessageOnionClient"
RUN dotnet build "MessageOnionClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessageOnionClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageOnionClient.dll"]
