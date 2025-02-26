CREATE TABLE [dbo].[Users] (
  [Id]       INT           NOT NULL IDENTITY,
  [Name]     NVARCHAR(255) NOT NULL,
  [Email]    NVARCHAR(255) NOT NULL,
  [IsActive] BIT           NOT NULL
  CONSTRAINT PK_Users PRIMARY KEY ([Id])  -- Explicitly named Primary Key
)

GO
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT UQ_Users_Email UNIQUE ([Email]);
