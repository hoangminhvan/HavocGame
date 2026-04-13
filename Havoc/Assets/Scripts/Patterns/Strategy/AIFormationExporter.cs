using System.Collections.Generic;

// PATTERN: Context Strategy
// Lop nay su dung Strategy de thuc hien viec lay du lieu ma khong can quan tam format la gi
public class AIFormationExporter
{
    private IAIFormationExportStrategy currentStrategy;

    // Cho phep thay doi chien luoc bat cu luc nao (Doi tu JSON sang CSV...)
    public void SetStrategy(IAIFormationExportStrategy strategy)
    {
        currentStrategy = strategy;
    }

    // Thuc thi viec lay du lieu dua tren chien luoc da duoc thiet lap.
    public List<PlacedUnitInfo> ExecuteExport()
    {
        if (currentStrategy == null)
        {
            return new List<PlacedUnitInfo>();
        }

        // Goi phuong thuc cua interface, logic cu the se nam o lop Concrete Strategy tuong ung
        return currentStrategy.ExportRandomFormation();
    }
}