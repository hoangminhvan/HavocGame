using System.Collections.Generic;

// PATTERN: Strategy Interface
// Dinh nghia chung cho tat ca cac cach thuc lay du lieu doi hinh AI.
public interface IAIFormationExportStrategy
{
    // Moi lop ke thua phai tra ve mot danh sach cac don vi (unit) da duoc sap xep.
    List<PlacedUnitInfo> ExportRandomFormation();
}