using UnityEngine;

// PATTERN: State Interface
// Dinh nghia cac phuong thuc ma moi trang thai (State) phai co
public interface IUnitState
{
    // Goi khi bat dau buoc vao trang thai
    void Enter(BaseUnit unit);

    // Goi lien tuc moi khung hinh 
    void Execute(BaseUnit unit);

    // Goi truoc khi thoat khoi trang thai de sang trang thai moi
    void Exit(BaseUnit unit);

    // Logic xu ly khi nguoi choi click vao mot o gach tren ban do
    void OnTileClicked(BaseUnit unit, Tile clickedTile);

    // Logic xu ly khi nguoi choi nhan chuot phai (thuong la de huy lenh)
    void OnRightClick(BaseUnit unit);
}