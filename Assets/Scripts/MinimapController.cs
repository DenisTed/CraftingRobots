using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public GameObject islandDotPrefab;
    public RectTransform minimapPanel;
    public int mapSize = 100;
    public int islandSize = 10;

    private Image[,] dots;

    void Start()
    {
        int count = mapSize / islandSize;
        dots = new Image[count, count];
    }

    public void UpdateDot(int x, int y, bool active)
    {
        if (dots == null || x < 0 || y < 0 || x >= dots.GetLength(0) || y >= dots.GetLength(1))
            return;

        if (dots[x, y] == null)
        {
            var dot = Instantiate(islandDotPrefab, minimapPanel);
            dot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * 10, y * 10);
            dots[x, y] = dot.GetComponent<Image>();
        }

        dots[x, y].color = active ? Color.green : Color.gray;
    }
}
