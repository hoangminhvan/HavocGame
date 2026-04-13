// PATTERN: Repository Interface
// Dinh nghia cac phuong thuc co ban de quan ly du lieu tran dau
public interface IMatchRepository
{
    void SaveMatch(MatchSaveData matchData); // Luu tran dau
    MatchSaveData LoadMatch();               // Tai tran dau
    bool HasSavedMatch();                    // Kiem tra ton tai file luu
    void ClearSavedMatch();                  // Xoa file luu
}