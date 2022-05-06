FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../MesageOnionTest/MesageOnionTest/MesageOnionTest.csproj", "MesageOnionTest/"]
RUN dotnet restore "../MesageOnionTest/MesageOnionTest/MesageOnionTest.csproj"
COPY . .
WORKDIR "/src/MesageOnionTest"
RUN dotnet build "MesageOnionTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MesageOnionTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MesageOnionTest.dll"]
