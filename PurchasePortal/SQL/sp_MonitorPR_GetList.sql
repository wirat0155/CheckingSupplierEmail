CREATE OR ALTER PROCEDURE sp_MonitorPR_GetList
    @startDate DATETIME,
    @endDate DATETIME,
    @convertType INT, -- 1 = Converted, 2 = Not Converted, Else = All
    @start INT,
    @length INT,
    @searchValue NVARCHAR(255) = NULL,
    @sortColumn NVARCHAR(50) = 'PORQ_LastChangeDate',
    @sortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);
    DECLARE @params NVARCHAR(MAX);
    DECLARE @joinClause NVARCHAR(MAX);
    DECLARE @whereClause NVARCHAR(MAX) = '';
    DECLARE @poSelect NVARCHAR(MAX) = ', MAX(po.POM_PurchorderID) AS PONumber';
    
    -- 1. Determine Join Strategy based on @convertType
    IF @convertType = 1 -- Converted
    BEGIN
        SET @joinClause = 'INNER JOIN [iERP85].[dbo].[vw_mfc_rptPOPrint] po ON PORequisition.PORQ_RequisitionNumber = po.PRNbr';
    END
    ELSE
    BEGIN
        SET @joinClause = 'LEFT JOIN [iERP85].[dbo].[vw_mfc_rptPOPrint] po ON PORequisition.PORQ_RequisitionNumber = po.PRNbr';
        
        IF @convertType = 2 -- Not Converted
        BEGIN
            SET @whereClause = ' AND po.POM_PurchorderID IS NULL';
        END
    END

    -- 2. Build Dynamic Where Clause for Search
    IF @searchValue IS NOT NULL AND @searchValue <> ''
    BEGIN
        SET @whereClause = @whereClause + ' AND (
            PORequisition.PORQ_RequisitionNumber LIKE @searchValue OR 
            PORequisition.PORQ_Notes LIKE @searchValue OR
            PORequisition.PORQ_M_Department LIKE @searchValue OR
            PORequisition.PORQ_M_Division LIKE @searchValue OR 
            PORequisition.PORQ_M_Remark LIKE @searchValue OR
            PORequisition.PORQ_M_QuotationNo LIKE @searchValue OR
            PORequisition.PORQ_M_ShipToDesc LIKE @searchValue
        )';
    END

    -- 3. Construct the Main Query using CTE for Paging
    SET @sql = N'
    WITH CTE_Data AS (
        SELECT 
            PORequisition.PORQ_RequisitionNumber,
            SUM(PORequisitionLine.PORQL_TotalCost) AS ERPAmount,
            PORequisition.PORQ_Notes,
            PORequisition.PORQ_DateSubmitted,
            PORequisition.PORQ_LastChangeDate,
            PORequisition.PORQ_M_Department,
            PORequisition.PORQ_M_Division,
            PORequisition.PORQ_M_Remark,
            PORequisition.PORQ_M_QuotationNo,
            PORequisition.PORQ_M_ShipToDesc,
            (ISNULL(e.EMP_FirstName, '''') + '' '' + ISNULL(e.EMP_LastName, '''')) AS LastChangeBy
            ' + @poSelect + '
        FROM PORequisition
        INNER JOIN PORequisitionLine
            ON PORequisition.PORQ_RecordID = PORequisitionLine.PORQL_PORQ_RecordID
        LEFT JOIN [dbo].[EMP] e 
            ON PORequisition.PORQ_EMP_RecordID_ChangedBy = e.EMP_RecordID
        ' + @joinClause + '
        WHERE 1=1 
          AND PORequisition.PORQ_DateSubmitted >= @startDate 
          AND PORequisition.PORQ_DateSubmitted < @endDate
          ' + @whereClause + '
        GROUP BY 
            PORequisition.PORQ_RequisitionNumber,
            PORequisition.PORQ_Notes,
            PORequisition.PORQ_DateSubmitted,
            PORequisition.PORQ_LastChangeDate,
            PORequisition.PORQ_M_Department,
            PORequisition.PORQ_M_Division,
            PORequisition.PORQ_M_Remark,
            PORequisition.PORQ_M_QuotationNo,
            PORequisition.PORQ_M_ShipToDesc,
            e.EMP_FirstName,
            e.EMP_LastName
    ),
    CTE_RowNumber AS (
        SELECT *, ROW_NUMBER() OVER (ORDER BY ' + @sortColumn + ' ' + @sortDirection + ') AS RowNum
        FROM CTE_Data
    ),
    CTE_Count AS (
        SELECT COUNT(*) AS TotalCount FROM CTE_Data
    )
    SELECT * 
    FROM CTE_RowNumber
    CROSS JOIN CTE_Count
    WHERE RowNum BETWEEN @start + 1 AND @start + @length
    OPTION (RECOMPILE);';

    SET @params = N'@startDate DATETIME, @endDate DATETIME, @start INT, @length INT, @searchValue NVARCHAR(255)';

    EXEC sp_executesql @sql, @params, @startDate, @endDate, @start, @length, @searchValue;
END
