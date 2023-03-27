FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["samples/Discord.Net.Host.Sample/Discord.Net.Host.Sample.csproj", "samples/Discord.Net.Host.Sample/"]
COPY ["src/Discord.Net.Host/Discord.Net.Host.csproj", "src/Discord.Net.Host/"]
RUN dotnet restore "samples/Discord.Net.Host.Sample/Discord.Net.Host.Sample.csproj"
COPY . .
WORKDIR "/src/samples/Discord.Net.Host.Sample"
RUN dotnet build "Discord.Net.Host.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discord.Net.Host.Sample.csproj" -c Release -o /app/publish

FROM base AS dependencies
WORKDIR /app
RUN apt-get update && apt-get install -y ffmpeg libopus-dev libsodium-dev
RUN ln -s "/usr/lib/x86_64-linux-gnu/libsodium.so" "./libsodium.so"
RUN ln -s "/usr/lib/x86_64-linux-gnu/libopus.so" "./opus.so"

FROM dependencies AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discord.Net.Host.Sample.dll"]
