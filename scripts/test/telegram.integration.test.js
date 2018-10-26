const $ = require('shelljs');
const path = require('path');
require('../logging')

$.config.fatal = true
const root = path.resolve(`${__dirname}/../..`)

const image = process.env.image_tag || 'busvbot:solution'
console.info(`running Telegram bot integration tests using Docker image "${image}"`)

try {
    console.debug('starting test dependencies. docker-compose project: "tg"')
    $.cd(`${root}/test/TelegramTests`)
    $.exec(`docker-compose --project-name tg up -d --force-recreate --remove-orphans`)

    try {
        console.debug('running tests')

        const settings = JSON.stringify(JSON.stringify({
            Mongo: {
                ConnectionString: "mongodb://mongo/busvbot-telegram-tests"
            },
            Redis: {
                Configuration: "redis"
            }
        }))

        $.exec(
            `docker run --rm --tty ` +
            `--workdir /project/test/TelegramTests/ ` +
            `--env "BUSVBOT_SETTINGS=${settings}" ` +
            `--network tg_busvbot-network ` +
            `"${image}" ` +
            `dotnet test --no-build --configuration Release --verbosity normal`
        )
    } finally {
        console.debug('removing test dependency containers via docker-compose')
        $.exec(`docker-compose --project-name tg rm -fv`)
    }
} catch (e) {
    console.error(e)
    process.exit(1)
}

console.info(`✅ Telegram integration tests passed`)
