-- 018_d78_roles_capability.sql
-- D-78 P1 (docs/design/D78-SPEC.md 2.1-2.6, D-80): the capability BeekeeperRegistration (district directorate of agriculture),
-- the organization roles RegistrationOfficer (beekeeper registration officer) and ComplianceOfficer, their names and
-- descriptions in five languages, and their default rights in RolePermissions:
--   * Registration Enter/Read  -> RegistrationOfficer (register beekeepers and apiaries)
--   * Evidence Approve         -> ComplianceOfficer (accept or reject a signed-photo registration, never one's own)
--   * read rights of the Compliance Officer: CCP records, intakes, samples, lab results, shipments, recall
-- Reference data only; no business row is changed. Insert-only and idempotent: a second run inserts nothing.
-- SQL Server 2019 (level 150). Run with sqlcmd -I -f 65001 (UTF-8).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL OR OBJECT_ID(N'dbo.ReferenceTranslations', N'U') IS NULL OR OBJECT_ID(N'dbo.OrgRoles', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 004, 009 and 013 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

BEGIN TRANSACTION;

INSERT dbo.Capabilities (Code, Description, SortOrder)
SELECT N'BeekeeperRegistration',
       N'Able to register beekeepers and their apiaries in its area, attest their identity and registry number, record their consent and issue their sign-in',
       11
WHERE NOT EXISTS (SELECT 1 FROM dbo.Capabilities WHERE Code = N'BeekeeperRegistration');

INSERT dbo.OrgRoles (Code, Description, SortOrder)
SELECT v.Code, v.Description, v.SortOrder
FROM (VALUES
        (N'RegistrationOfficer', N'Registers beekeepers and apiaries, attests identity and consent (district directorate)', 8),
        (N'ComplianceOfficer', N'Oversees approvals, sampling quality, evidence and data protection', 9)
     ) v (Code, Description, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrgRoles r WHERE r.Code = v.Code);

DECLARE @t TABLE (Entity varchar(40) NOT NULL, EntityKey nvarchar(200) NOT NULL, Field varchar(40) NOT NULL, Culture varchar(10) NOT NULL, Value nvarchar(400) NOT NULL);
INSERT @t (Entity, EntityKey, Field, Culture, Value) VALUES
    ('Capability', N'BeekeeperRegistration', 'Name', 'en', N'Beekeeper registration'),
    ('Capability', N'BeekeeperRegistration', 'Name', 'tr', N'Arıcı kaydı'),
    ('Capability', N'BeekeeperRegistration', 'Name', 'es', N'Registro de apicultores'),
    ('Capability', N'BeekeeperRegistration', 'Name', 'pl', N'Rejestracja pszczelarzy'),
    ('Capability', N'BeekeeperRegistration', 'Name', 'lv', N'Biškopju reģistrācija'),
    ('Capability', N'BeekeeperRegistration', 'Description', 'tr', N'Bölgesindeki arıcıları ve arılıklarını kaydedebilir, kimliklerini ve kayıt numaralarını doğrulayabilir, rızalarını kaydedebilir ve giriş bilgilerini verebilir'),
    ('Capability', N'BeekeeperRegistration', 'Description', 'es', N'Puede registrar a los apicultores de su zona y sus colmenares, certificar su identidad y número de registro, registrar su consentimiento y emitir su acceso'),
    ('Capability', N'BeekeeperRegistration', 'Description', 'pl', N'Może rejestrować pszczelarzy ze swojego obszaru i ich pasieki, poświadczać ich tożsamość i numer rejestrowy, rejestrować ich zgodę i wydawać dane logowania'),
    ('Capability', N'BeekeeperRegistration', 'Description', 'lv', N'Var reģistrēt savas teritorijas biškopjus un viņu dravas, apliecināt viņu identitāti un reģistra numuru, reģistrēt viņu piekrišanu un izsniegt pieteikšanās datus'),
    ('OrgRole', N'RegistrationOfficer', 'Name', 'tr', N'Arıcı Kayıt Görevlisi'),
    ('OrgRole', N'RegistrationOfficer', 'Name', 'es', N'Responsable de registro de apicultores'),
    ('OrgRole', N'RegistrationOfficer', 'Name', 'pl', N'Specjalista ds. rejestracji pszczelarzy'),
    ('OrgRole', N'RegistrationOfficer', 'Name', 'lv', N'Biškopju reģistrācijas speciālists'),
    ('OrgRole', N'ComplianceOfficer', 'Name', 'tr', N'Uyum Sorumlusu'),
    ('OrgRole', N'ComplianceOfficer', 'Name', 'es', N'Responsable de cumplimiento'),
    ('OrgRole', N'ComplianceOfficer', 'Name', 'pl', N'Specjalista ds. zgodności'),
    ('OrgRole', N'ComplianceOfficer', 'Name', 'lv', N'Atbilstības speciālists');

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT t.Entity, t.EntityKey, t.Field, t.Culture, t.Value, SYSUTCDATETIME()
FROM @t t
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r
                  WHERE r.Entity = t.Entity AND r.EntityKey = t.EntityKey AND r.Field = t.Field AND r.Culture = t.Culture);

-- default rights of the new roles
INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES
        ('Registration', 'Enter', N'RegistrationOfficer'), ('Registration', 'Read', N'RegistrationOfficer'),
        ('Evidence', 'Approve', N'ComplianceOfficer'),
        ('Intake', 'Read', N'ComplianceOfficer'), ('Samples', 'Read', N'ComplianceOfficer'), ('LabResults', 'Read', N'ComplianceOfficer'),
        ('Shipments', 'Read', N'ComplianceOfficer'), ('Recall', 'Read', N'ComplianceOfficer')
     ) v (Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);

-- the Compliance Officer reads the CCP records of the items its organization takes part in
INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT 'Ccp', CONVERT(varchar(20), c.Id), 'Read', N'ComplianceOfficer'
FROM dbo.CriticalControlPoints c
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = 'Ccp' AND p.AreaKey = CONVERT(varchar(20), c.Id) AND p.Action = 'Read' AND p.OrgRole = N'ComplianceOfficer');

COMMIT TRANSACTION;

PRINT N'018: capability BeekeeperRegistration, roles RegistrationOfficer and ComplianceOfficer, translations and default rights are in place.';
GO

SET NOEXEC OFF;
GO
