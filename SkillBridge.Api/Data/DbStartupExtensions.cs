using Microsoft.EntityFrameworkCore;

namespace SkillBridge.Api.Data;

public static class DbStartupExtensions
{
    public static async Task EnsureCompatibilityAsync(this AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // EnsureCreated does not apply incremental schema changes on existing databases.
        // These guarded ALTER statements keep older local databases compatible.
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE users ADD COLUMN IF NOT EXISTS "Phone" character varying(30);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS "Location" character varying(120);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS "Bio" character varying(1000);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS "Education" character varying(200);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS "CareerGoal" character varying(200);
            """,
            cancellationToken);
    }
}
