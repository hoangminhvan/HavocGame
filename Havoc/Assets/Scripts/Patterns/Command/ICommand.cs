// PATTERN: Command Interface
// Giao dien co ban cho tat ca cac hanh dong co the thuc thi va hoan tac
public interface ICommand
{
    // Ham chay logic hanh dong (vi du: thuc hien di chuyen)
    void Execute();

    // Ham dao nguoc logic hanh dong (vi du: quay lai vi tri cu)
    void Undo();
}