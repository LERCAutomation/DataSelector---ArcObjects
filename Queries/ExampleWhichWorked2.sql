EXEC [dbo].[AFHLSelectSppSubset] @Schema = 'dbo', @SpeciesTable = 'TVERC_Spp_Full', @ColumnNames = 'TaxonGroup, TaxonName, CommonName', 
                        @WhereClause = 'TaxonGroup = ''Birds''', @GroupByClause = 'CommonName, TaxonName, TaxonGroup', 
                        @OrderByClause = 'CommonName', @UserId = 'Hester2'