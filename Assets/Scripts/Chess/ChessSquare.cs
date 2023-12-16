using UnityEngine;

public class ChessSquare : MonoBehaviour 
{
    public Color whiteColor = Color.white;
    public Color blackColor = Color.black;
    public string Id { get; private set; }
    public Vector2Int Position
    {
        get
        {
            var id = Id;
            return new Vector2Int(id[0] - 'a', id[1] - '1');
        }
    }

    public void Initialize(string id, Vector3 position, Team color)
    {
        Id = id;
        transform.localPosition = position;
        GetComponent<Renderer>().material.color = color == Team.White ? whiteColor : blackColor;
        gameObject.name = id;
        gameObject.tag = Team.White == color ? "whiteSquare" : "blackSquare";
    }
}