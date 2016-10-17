USE [pubs]
GO
/****** Object:  StoredProcedure [dbo].[GetTitles]    Script Date: 30/01/2014 13:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTitles] 
AS
BEGIN
	SET NOCOUNT ON;

	Waitfor Delay '00:00:15';

	SELECT title, price from titles;
END