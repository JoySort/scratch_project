FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RecognizerRunner/RecognizerRunner.csproj", "RecognizerRunner/"]
RUN dotnet restore "RecognizerRunner/RecognizerRunner.csproj"
COPY . .
WORKDIR "/src/RecognizerRunner"
RUN dotnet build "RecognizerRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecognizerRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecognizerRunner.dll"]
