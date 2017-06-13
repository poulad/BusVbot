# myTTCBot

[![Build Status](https://travis-ci.org/pouladpld/myTTCBot.svg?branch=master)](https://travis-ci.org/pouladpld/myTTCBot)

A Telegram chat bot that makes sure you won't miss your bus in Toronto

## Screenshots

--> Add examples here <--

## Getting Started

### Requirements

- Visual Studio 2017 or [.NET Core 1.1](https://www.microsoft.com/net/download/core#/current).
- PostgreSQL database
- Telegram bot API Token

> Talk to **[BotFather](http://t.me/botfather)** to get a token from Telegram for your bot. This token is your bot's secret. Keep it safe and never commit it to git.

### Configurations

Make a copy of [appsettings.json](src/MyTTCBot/appsettings.json) in project folder and name it `appsettings.Development.json`:

```bash
cd src\MyTTCBot
copy appsettings.json appsettings.Development.json
```

At minimum, put the _bot name_ and _API token_ values in that file.

> There are other options to provide the app with configurations. Have a look at first few lines of [Startup class](src/MyTTCBot/Startup.cs).

> Note that `appsettings.Development.json` will be gitignored so it is safe to store the app secrets there.

### Running

Run Postgres database in a docker container:

```bash
docker run --name myttcbot-postgres -p 5432:5432 -e POSTGRES_USER=myttcbot -e POSTGRES_PASSWORD=password -d postgres
```

By running the app in VS without webhooks, bot starts getting updates and processing them. If webhooks are enabled, navigate to [http://localhost:5000/botname/apitoken/me](http://localhost:5000/botname/apitoken/me) and see the bot in action.
