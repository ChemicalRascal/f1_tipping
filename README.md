No time for a proper README, here's some bash commands for reference re. how to correctly do db migrations

I'm really sorry that I used a second dbcontext and I want EFCore to stop hurting me for it

Making that first migration:

`dotnet ef migrations add InitModelDatabase -o ModelMigrations -p F1Tipping.Postgres -s F1Tipping -c F1Tipping.Data.ModelDbContext -- --provider Postgres`

Once you've specified `-o <db>Migrations` in the init migration:

`dotnet ef migrations add MakeTipDeadlinesFixed -p F1Tipping.Postgres -s F1Tipping -c F1Tipping.Data.ModelDbContext -- --provider Postgres`

Deploying to your local instance:

`dotnet ef database update -p F1Tipping.Postgres -s F1Tipping -c F1Tipping.Data.ModelDbContext -- --provider Postgres`

Upscript for prod:

`dotnet ef migrations script InitModelDatabase MakeTipDeadlinesFixed --no-build -p F1Tipping.Postgres -s F1Tipping -c F1Tipping.Data.ModelDbContext -- --provider Postgres >> ../sql_migrations/"$(date +"%Y_%m_%d_%H%M%S").MakeTipDeadlinesFixed.ModelDb.sql"`
