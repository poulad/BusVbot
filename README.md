# 🚍 BusV Bot 🤖

[![tg badge]](https://t.me/BusVbot)
[![travis badge]](https://travis-ci.org/pouladpld/BusVbot)

[![BusVbot logo](./docs/logo.png)](https://t.me/BusVbot)

**BusV** is a [Telegram] chat bot 🤖 that helps you catch your bus 🚍 by providing you route information and bus predictions. Start chatting with it on Telegram to find out more: [`@BusVbot`](https://t.me/BusVbot).

This bot works best for passengers who take frequent routes and already know what bus they should take to reach their destinations.

[![BusVbot demo](./docs/demo.gif)](https://t.me/BusVbot)

Supporting 70+ transit agencies in North America, the real-time bus predictions are powered by [NextBus].

Here are the regions that agencies operate in:

| Canada | U.S. |
| :----- | :-- |
| Ontario, Quebec | California-Northern, California-Southern, District of Columbia, Florida, Georgia, Indiana, Iowa, Kentucky, Maryland, Massachusetts, Mississippi, Nevada, New Jersey, New York, North Carolina, Oregon, Pennsylvania, Texas, Virginia, Washington |

## Getting Started

> You can take a look at the Redis instance via:
> ```sh
> docker run -it --link busvbot-redis:redis --rm redis redis-cli -h redis -p 6379
> ```

## Build and Run

BusVbot is built using [`Telegram.Bot.Framework`](https://github.com/pouladpld/Telegram.Bot.Framework) as an ASP.NET Core app.

### Requirements

- Visual Studio 2017 or [.NET Core 1.1](https://github.com/dotnet/core/blob/master/release-notes/download-archive.md)
- Postgres database
- Telegram Bot API token

### Configurations

Make a copy of [appsettings.json](./src/BusVbot/appsettings.json) in project folder and name it `appsettings.Development.json`:

```bash
cp -v src/BusVbot/appsettings{,.Development}.json
```

Edit the `appsettings.Development.json` file and put your _bot name_,  _API token_, and _Postgres connection string_ values there.

> There are other options to provide the app with configurations. Have a look at first few lines of [Startup class](./src/BusVbot/Startup.cs).

> Note that `appsettings.Development.json` is gitignored so it is safe to store the app secrets there.

### Database

Run the Postgres database. Examples here are using docker containers.

```bash
# create and start the container
docker run -d -p 5432:5432 --name busvbot-postgres -e POSTGRES_PASSWORD=password -e POSTGRES_USER=busvbot -e POSTGRES_DB=busvbot postgres
```

> See [`docker-local.sh`](./src/scripts/docker-local.sh) for sample docker commands to use.

### Seed Data

While running in _Development_ environment, app makes sure database is seeded with data from NextBus before it starts the bot.

It might take a while to download all the data so in order to save time, use the [sql dump file](./src/BusVbot/Data/nextbus-dump.sql) and its `INSERT` statements instead.

Create schema:

```bash
cd src/BusVbot
dotnet restore
dotnet ef database update
```

Insert data:

```bash
# inside docker container, as postgres user
psql -U busvbot < nextbus-dump.sql
```

### Run

Run the app and start chatting with the bot on Telegram.

## Bot Configurations

### Commands

Set the commands for bot in a chat with `@BotFather`.

```text
bus - 🚍 Bus predictions
profile - 👤 View/Modify my profile
help - ❓ Help
```

## Tests

### Systems Integration Tests

[TelegramTests project] contains the Systems Integration test cases.
The HTTP requests to the Telegram Bot API are mocked using([Moq])
and [Docker Compose] is used to run containers for other dependencies.

Before running the tests, MongoDB and Redis should be up and running.

```sh
cd test/TelegramTests
# run the containers in the background
docker-compose --project-name tg up -d --force-recreate --remove-orphans

# optionally, see the logs while running the tests
# docker-compose --project-name tg logs --follow
```

Tests cases can be run now from the IDE or the console.

```sh
dotnet test --verbosity normal
```

Containers can be removed after the tests are done.

```sh
docker-compose --project-name tg rm --stop -fv
```

[tg badge]: https://img.shields.io/badge/@BusVbot-Telegram-blue.svg?style=flat-square
[travis badge]: https://img.shields.io/travis/poulad/BusVbot/master.svg?style=flat-square&label=Build
[Telegram]: https://www.telegram.org
[NextBus]: https://www.nextbus.com
[TelegramTests project]: ./test/TelegramTests
[Moq]: https://github.com/moq/moq4
[Docker Compose]: https://docs.docker.com/compose
