FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Sun/Sun.csproj", "Sun/"]
RUN dotnet restore "Sun/Sun.csproj"
COPY . .
WORKDIR "/src/Sun"
RUN dotnet build "Sun.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sun.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sun.dll"]
