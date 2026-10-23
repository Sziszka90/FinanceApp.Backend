-- 1. Create a SQL Server login
CREATE LOGIN FinanceAppDB_Login
WITH PASSWORD = 'Secret12345!', -- Change this to a secure password
     CHECK_EXPIRATION = OFF,
     CHECK_POLICY = ON;

-- 2. Use the target database
USE FinanceAppDB;


-- 3. Create a database user mapped to the login
CREATE USER FinanceAppDB_User
FOR LOGIN FinanceAppDB_Login
WITH DEFAULT_SCHEMA = dbo;

-- 4. Grant roles/permissions
-- Basic read/write access
ALTER ROLE db_datareader ADD MEMBER FinanceAppDB_User;
ALTER ROLE db_datawriter ADD MEMBER FinanceAppDB_User;

ALTER ROLE db_ddladmin ADD MEMBER FinanceAppDB_User;
ALTER ROLE db_owner ADD MEMBER FinanceAppDB_User;

-- Optionally grant additional permissions, e.g.:
-- EXEC sp_addrolemember 'db_ddladmin', 'FinanceAppDB_User'; -- for DDL (create tables, etc.)
-- EXEC sp_addrolemember 'db_owner', 'FinanceAppDB_User'; -- full control (use with caution)
