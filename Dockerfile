FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app/
RUN echo "Installing FFmpeg..." \
    && apt-get update \
    && apt-get install --assume-yes --quiet --no-install-recommends ffmpeg \
    && apt-get clean \
    && apt-get autoremove --assume-yes --quiet \
    && rm -rvf /var/lib/apt/lists/*


FROM microsoft/dotnet:2.1-sdk AS publish
ARG configuration=Release
COPY src src
RUN dotnet publish src/BusV.Telegram/BusV.Telegram.csproj --configuration ${configuration} --output /app/


FROM base AS final
WORKDIR /app/
COPY --from=publish /app /app
CMD ASPNETCORE_URLS=http://+:${PORT:-80} dotnet BusV.Telegram.dll


FROM microsoft/dotnet:2.1-sdk AS solution-build
ARG configuration=Debug
WORKDIR /project/
COPY . .
RUN dotnet build BusVbot.sln --configuration ${configuration}
