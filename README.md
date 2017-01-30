# myTTCBot

## Requirements
#### .NET Core
This is an **ASP.NET Core 1.1** web application so you need to [install .NET Core](https://www.microsoft.com/net/download/core#/current).

#### API Token
Talk to **[BotFather](http://t.me/botfather)** to get a token from Telegram for your bot.

> This token is your bot's secret. Keep it safe and never commit it to git.

## Running the bot

#### Configurations
Make a new file in project folder and name it `appsettings.development.json`. Populate it according to this format:
```json
{
  "ApiToken": "",
  "BotName": ""
}
```
Optionally, you could have these configurations as Environment Variables prefixed with `MyTTCBot`. Just have a look at the first few lines of `Startup.cs`.
> Note that `appsettings.development.json` will be gitignored so it is safe to store your information there.

#### Compilation
Open a terminal in project's folder and run the following commands:
```bash
~$ dotnet restore
~$ dotnet run
```
Application is running now. Navigate to <http://localhost:5000/botname/apitoken> and see the your bot responding messages.
