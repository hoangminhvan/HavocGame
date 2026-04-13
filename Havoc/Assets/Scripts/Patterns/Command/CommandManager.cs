using System.Collections.Generic;
using UnityEngine;

// PATTERN: Invoker
// Quan ly viec thuc thi va luu tru lich su cac lenh de thuc hien Undo
public class CommandManager : MonoBehaviour
{
    public static CommandManager Instance { get; private set; }

    // Su dung Stack (Ngan xep) de luu lich su: Lenh vao sau cung se duoc Undo dau tien (LIFO)
    private readonly Stack<ICommand> undoStack = new Stack<ICommand>();

    private void Awake()
    {
        Instance = this;
    }

    // Thuc thi lenh va day vao lich su
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        undoStack.Push(command);
    }

    // Lay lenh gan nhat tu lich su va goi ham Undo cua no
    public void UndoCommand()
    {
        if (undoStack.Count > 0)
        {
            ICommand command = undoStack.Pop();
            command.Undo();
        }
        else
        {
            Debug.LogWarning("[CommandManager] Khong co hanh dong nao de hoan tac trong luot nay.");
        }
    }

    // Xoa lich su khi ket thuc luot (Khong cho phep Undo hanh dong cua luot truoc)
    public void ClearHistory()
    {
        undoStack.Clear();
    }
}