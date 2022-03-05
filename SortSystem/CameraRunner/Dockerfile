FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CameraRunner/CameraRunner.csproj", "CameraRunner/"]
RUN dotnet restore "CameraRunner/CameraRunner.csproj"
COPY . .
WORKDIR "/src/CameraRunner"
RUN dotnet build "CameraRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CameraRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CameraRunner.dll"]
