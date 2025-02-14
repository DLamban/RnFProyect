// se puede annadir --force para que sobreescriba los archivos

dotnet ef dbcontext scaffold "Data Source=C:\\dev\\games\\RnFProyect\\Core\\DB\\RnFDBSqlite.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir ./DB/Models --context-dir ./DB/Data --context GameDbContext --project ..\Core.csproj

// con force

dotnet ef dbcontext scaffold "Data Source=C:\\dev\\games\\RnFProyect\\Core\\DB\\RnFDBSqlite.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir ./DB/Models --context-dir ./DB/Data --context GameDbContext --project ..\Core.csproj --force