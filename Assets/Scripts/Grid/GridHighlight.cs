using UnityEngine;
using VContainer;
using MessagePipe;

public class GridHighlight : MonoBehaviour
{
    private GridSystem gridSystem;
    private ISubscriber<MoveEvent> moveSubscriber;
    private GameObject highlightQuad;

    [Inject]
    public void Construct(GridSystem gridSystem, ISubscriber<MoveEvent> moveSubscriber)
    {
        this.gridSystem = gridSystem;
        this.moveSubscriber = moveSubscriber;
    }

    private void Start()
    {
        CreateHighlightQuad();
        moveSubscriber.Subscribe(OnMoveEvent);
    }

    private void OnMoveEvent(MoveEvent e)
    {
        Vector2Int cell = gridSystem.WorldToGrid(e.Destination);

        if (gridSystem.IsInBounds(cell))
        {
            Vector3 worldPos = gridSystem.GridToWorld(cell);
            worldPos.y = 0.01f;
            highlightQuad.transform.position = worldPos;
            highlightQuad.SetActive(true);
        }
        else
        {
            highlightQuad.SetActive(false);
        }
    }

    private void CreateHighlightQuad()
    {
        highlightQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        highlightQuad.name = "HighlightQuad";
        highlightQuad.transform.SetParent(transform);
        highlightQuad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        highlightQuad.transform.localScale = Vector3.one * gridSystem.cellSize * 0.95f;
        Destroy(highlightQuad.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(1f, 1f, 0f, 0.4f);
        highlightQuad.GetComponent<Renderer>().material = mat;
        highlightQuad.SetActive(false);
    }
}