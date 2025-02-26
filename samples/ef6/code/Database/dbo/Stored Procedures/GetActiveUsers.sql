CREATE PROCEDURE [dbo].[GetActiveUsers] AS
BEGIN
  SET NOCOUNT ON;

  SELECT 
    Id,
    Name,
    Email,
    IsActive
  FROM [dbo].[Users] 
  WHERE IsActive = 1
END
