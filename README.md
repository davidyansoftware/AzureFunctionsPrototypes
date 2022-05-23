# AzureFunctionsPrototypes

Playfab Title Id is passed in via playfab request and automatically parsed

DeveloperSecretKey is handled via environmental variable: PLAYFAB_DEV_SECRET_KEY

See official documentation: https://docs.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal

## Battle

Used in conjuction with https://github.com/davidyansoftware/UnityPlayfabPrototypes to handle battles between players in a server-authoritative manner.

Requests are executed with 2players playfabIds.

Battles are simulated by comparing players' power values.

Ratings are determined via an ELO matchmaking algorithm and persisted to the leaderboard.
