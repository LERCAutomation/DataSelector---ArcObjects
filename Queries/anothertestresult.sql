-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE HLTestExtract 
	-- Add the parameters for the stored procedure here
	@Columns varchar (255) = 'TaxonName, CommonName, TaxonClass', 
	@WhereClause varchar (255) = 'TaxonName = ''Birds''', 
	@GroupBy varchar (255) = 'CommonName',
	@OrderBy varchar (255) = 'CommonName'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @Columns FROM dbo.TVERC_Spp_Full
	WHERE @WhereClause
	GROUP BY @GroupBy
	ORDER BY @OrderBy;

END
GO
