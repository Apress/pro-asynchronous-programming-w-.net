USE [pubs]
GO
/****** Object:  StoredProcedure [dbo].[GetAuthors]    Script Date: 30/01/2014 13:41:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetAuthors] 
AS
BEGIN
	SET NOCOUNT ON;

	Waitfor Delay '00:00:05';

	SELECT au_fname, au_lname from Authors;
END
