// PATTERN: Context Repository
// Cung cap mot diem truy cap duy nhat vao Repository dang duoc su dung
public class DataStorageContext
{
    // Mac dinh su dung JsonMatchRepository
    public static IMatchRepository Repository { get; private set; } = new JsonMatchRepository();
}