FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RecognizeRunner/RecognizeRunner.csproj", "RecognizeRunner/"]
RUN dotnet restore "RecognizeRunner/RecognizeRunner.csproj"
COPY . .
WORKDIR "/src/RecognizeRunner"
RUN dotnet build "RecognizeRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecognizeRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecognizeRunner.dll"]
