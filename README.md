# ProDuck

## Docker Compose

### To Rebuild / Update

1. `git pull`
1. `docker compose down`
1. `docker compose build`
1. `docker compose up -d`

### Setup

1. Change `docker-compose.yml` environment:

    1. `MYSQL_ROOT_PASSWORD`
    1. `MYSQL_PASSWORD`
    1. `Database:ConnectionString`
    1. `JwtSettings:SigningKey`

1. `docker compose up -d`

## Svelte Apps

### 1. Building

1. Change `fetchServer` url to `192.168...`
1. `npm run build`
1. Change port on `build/index.js` as desired

### 2. PM2 Setup
1. pm2 `ecosystem.config.js` configuration file:

    ```
    module.exports = {
    apps : [{
        name   : "produck-console",
        script : "../produck-console/build/index.js",
        env_production: {
        NODE_ENV: "production",
        ORIGIN: "http://192.168.1.2:8080",
        PORT: "8080"
        }
    },
    {
        name : "produck-pos",
        script: "../produck-pos/build/index.js",
        env_production: {
        NODE_ENV: "production",
        ORIGIN: "http://192.168.1.2:8000",
        PORT: "8000"
        }
    }]
    }
    ```

1. `pm2 delete all`
1. `pm2 startOrReload ecosystem.config.js --update-env`
1. `pm2 save`
1. `pm2 startup`
