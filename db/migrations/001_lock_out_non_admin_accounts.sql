-- 001_lock_out_non_admin_accounts.sql
-- Decision D-09 (docs/DECISIONS.md): only administrators log in during this release (D-01).
-- Beekeeper logins were created with the business number as the password, so they are guessable.
-- This data script locks out every account that is not in the Administrator role, using the
-- standard ASP.NET Identity lockout (LockoutEnabled = 1, LockoutEnd = DateTimeOffset.MaxValue).
-- It also rotates the security stamp so any existing sign-in cookie of those accounts is rejected.
--
-- Idempotent: only rows that are not yet locked out permanently are touched, so re-running
-- the script changes nothing. Data only, no schema change. SQL Server 2019 (level 150) compatible.
-- To reverse for a single account later: set LockoutEnd = NULL for that user (after giving it a
-- strong password).

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
   OR OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    RAISERROR(N'Identity tables not found; nothing to do.', 10, 1);
    RETURN;
END;

-- D-98: a new installation (db/install) has no accounts at all, so there is nothing to lock and no administrator yet; the
-- first administrator is created afterwards (docs/KURULUM-TR.md, part B). A database with any account behaves as before.
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers)
BEGIN
    PRINT N'No user accounts yet (new installation); nothing to lock.';
    RETURN;
END;

-- Refuse to run if no administrator exists; locking everyone out would leave no way in.
IF NOT EXISTS (
    SELECT 1
    FROM dbo.AspNetUserRoles ur
    JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
    WHERE r.NormalizedName = N'ADMINISTRATOR')
BEGIN
    RAISERROR(N'No user in the Administrator role; aborting.', 16, 1);
    RETURN;
END;

DECLARE @Forever datetimeoffset(7) = '9999-12-31T23:59:59.9999999+00:00';

BEGIN TRANSACTION;

UPDATE u
SET    u.LockoutEnabled   = 1,
       u.LockoutEnd       = @Forever,
       u.SecurityStamp    = CONVERT(nvarchar(36), NEWID()),
       u.ConcurrencyStamp = CONVERT(nvarchar(36), NEWID())
FROM   dbo.AspNetUsers u
WHERE  NOT EXISTS (
           SELECT 1
           FROM dbo.AspNetUserRoles ur
           JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
           WHERE ur.UserId = u.Id
             AND r.NormalizedName = N'ADMINISTRATOR')
  AND  (u.LockoutEnabled = 0 OR u.LockoutEnd IS NULL OR u.LockoutEnd < @Forever);

PRINT CONCAT(N'Non-admin accounts locked out: ', @@ROWCOUNT);

COMMIT TRANSACTION;
