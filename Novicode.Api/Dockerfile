FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NoviCode.Api/NoviCode.Api.csproj", "NoviCode.Api/"]
COPY ["NoviCode.Core/NoviCode.Core.csproj", "NoviCode.Core/"]
COPY ["NoviCode.Gateway/NoviCode.Gateway.csproj", "NoviCode.Gateway/"]
COPY ["NoviCode.Data/NoviCode.Data.csproj", "NoviCode.Data/"]
RUN dotnet restore "NoviCode.Api/NoviCode.Api.csproj"
COPY . .
WORKDIR "/src/NoviCode.Api"
RUN dotnet build "./NoviCode.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NoviCode.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoviCode.Api.dll"]
