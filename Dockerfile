FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
RUN apt-get update -y
RUN apt-get install -y libc6-dev 
RUN apt-get install -y libgdiplus
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Causym.csproj", "Causym/"]

#Restore dependencies
RUN dotnet restore "Causym/Causym.csproj" -s https://www.myget.org/F/quahu/api/v3/index.json -s https://api.nuget.org/v3/index.json
COPY src/ Causym/
WORKDIR "Causym/"
RUN dotnet build "Causym.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "Causym.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Causym.dll"]