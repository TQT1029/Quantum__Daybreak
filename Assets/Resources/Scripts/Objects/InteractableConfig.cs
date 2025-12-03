using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class InteractableConfig : MonoBehaviour
{
    private Outline outline;

    private void Start()
    {
        if (outline == null)
            outline = GetComponent<Outline>();
    }

    // Hàm này sẽ được gọi khi người chơi bấm nút tương tác
    public void Interact()
    {

    }

    // Hàm này để hiện UI (ví dụ: dòng chữ "Ấn E để mở")
    public void ShowOutline(bool isShowing)
    {
        if (outline == null)
            outline = GetComponent<Outline>();
        outline.enabled = isShowing;
    }
}