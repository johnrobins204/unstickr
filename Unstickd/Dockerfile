# BASE IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# INSTALL LITESTREAM
# We use the specific version for amd64 Linux (typical container env)
ADD https://github.com/benbjohnson/litestream/releases/download/v0.3.13/litestream-v0.3.13-linux-amd64.tar.gz /tmp/litestream.tar.gz
RUN tar -C /usr/local/bin -xzf /tmp/litestream.tar.gz

# BUILD IMAGE
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Unstickd.csproj", "./"]
RUN dotnet restore "Unstickd.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Unstickd.csproj" -c Release -o /app/build

# PUBLISH IMAGE
FROM build AS publish
RUN dotnet publish "Unstickd.csproj" -c Release -o /app/publish

# FINAL IMAGE
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY litestream.yml /etc/litestream.yml

# ENTRYPOINT
# We wrap the dotnet command with litestream replicate
# This ensures that if the app runs, the DB is being replicated.
# Ideally, you would have a litestream.yml configuration file copied in as well.
ENTRYPOINT ["litestream", "replicate", "-exec", "dotnet Unstickd.dll"]
