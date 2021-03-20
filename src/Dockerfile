#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FileWatcher.csproj", "."]
RUN dotnet restore "./FileWatcher.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FileWatcher.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "FileWatcher.csproj" -c Debug -r linux-x64 -p:PublishSingleFile=true --self-contained true -o /app/publish

FROM base AS final
RUN apt-get update && apt-get install -y --no-install-recommends libc6 libgcc1 libgssapi-krb5-2 libicu63 libssl1.1 libstdc++6 zlib1g && apt-get clean
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/FileWatcher"]