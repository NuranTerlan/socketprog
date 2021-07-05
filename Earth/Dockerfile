FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Earth/Earth.csproj", "Earth/"]
RUN dotnet restore "Earth/Earth.csproj"
COPY . .
WORKDIR "/src/Earth"
RUN dotnet build "Earth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Earth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Earth.dll"]
