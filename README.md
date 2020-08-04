# Amersfoort RAID Bot

TODO: Everything that looks even remotely like documentation

TODO: Upgrade base technologies -> angular to V9, get rid of the way too simple legacy in-memory DB module, .NET Core -> 3.1, use proper JWT framework instead of this legacy crap

TODO: Refactor to have a clean architecture

Getting started:
- Install postgress locally
- Create a root user and remember the password
- Make sure node-sass builds and compiles (which is always a pain, ```npm install -g windows-build-tools``` on a admin powershell works wonders)
- cd PokemonGoRaidBot
- npm install
- npm run build:dev
- you can debug the solution from visual studio [make sure you have .NET Core 2.1]

## Configure it

Create a file called 'appsettings.Development.json' in the PokemonGoRaidBot folder with these contents

(keep in mind this is for debugging only)

```JSON
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=raidbot;Username=postgres;Password=YOURPOSTGRESSPASSWORD"
  },
  "Auth": {
    "Jwt": {
      "Issuer": "http://localhost:14600/",
      "Audience": "http://localhost:14600/",
      "TokenExpirationInMinutes": 60
    }
  },
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Debug",
        "System": "Information",
        "Microsoft": "Information"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Debug",
        "System": "Information",
        "Microsoft": "Information"
      }
    }
  },
  "StaticFiles": {
    "Headers": {
      "Cache-Control": "no-cache, no-store",
      "Pragma": "no-cache",
      "Expires": "-1"
    }
  },
  "RaidBot": {
    "BotKey": "YOUR TEST BOT API KEY",
    "PublicationChannel": "ID OF THE CHANNEL",
    "GoogleLocationAPIKey": "GET  A LOCATION API KEY FROM GOOGLE",
    "PogoAfoMappings": [
      {
        "Url": "https://api.pogoafo.nl/v3/raids.json?key=xxxx&gemeente=yyy",
        "Channel": "-1001150795507",
        "Targets": [
          {
            "Description":  "Level 5 raids only",
            "ChannelID": "-1001353939530",
            "Levels": [ 5 ],
            "ExRaidLevels": []
          },
          {
            "Description": "Level 3 raids only",
            "ChannelID": "-1001257287095",
            "Levels": [ 3 ],
            "ExRaidLevels": []
          }
        ]
      }
    ]
  }
}


```

#I18N

Two levels of i18n. For the angular app we just usse the default angular solution.

For the backend we usse gettext:

- [http://www.gnu.org/software/gettext/manual/html_node/xgettext-Invocation.html#xgettext-Invocation]
- [https://poedit.net/]
- [https://github.com/neris/NGettext]


Workflow: cd to the root of the project
  
  ```
  $ xgettext -D . -o raidbot.pot -p i18n -L 'C#' --from-code='UTF-8' `find . -name '*.cs' -print` 
  ```

Then use poedit to translate the messages. Save in i18n/<locale>/LC_MESSAGES/raidbot.po
