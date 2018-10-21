FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app/


FROM microsoft/dotnet:2.1-sdk AS web-app-build
ARG configuration=Debug
WORKDIR /project/
COPY src src
RUN dotnet build src/BusV.Telegram/BusV.Telegram.csproj --configuration ${configuration}


FROM web-app-build AS publish
WORKDIR /project/
RUN dotnet publish src/BusV.Telegram/BusV.Telegram.csproj --configuration Release --output /app/


FROM base AS final
WORKDIR /app/
COPY --from=publish /app /app
CMD ASPNETCORE_URLS=http://+:${PORT:-80} dotnet BusV.Telegram.dll


FROM microsoft/dotnet:2.1-sdk AS solution-build
ARG configuration=Debug
WORKDIR /project/
COPY . .
RUN dotnet build BusVbot.sln --configuration ${configuration}
