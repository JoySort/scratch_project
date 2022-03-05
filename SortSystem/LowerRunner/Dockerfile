FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LowerRunner/LowerRunner.csproj", "LowerRunner/"]
RUN dotnet restore "LowerRunner/LowerRunner.csproj"
COPY . .
WORKDIR "/src/LowerRunner"
RUN dotnet build "LowerRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LowerRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LowerRunner.dll"]
