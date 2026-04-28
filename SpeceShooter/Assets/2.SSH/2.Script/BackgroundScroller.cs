using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2f;
    private float _height;
    private Vector3 _startPos;
    void Start()
    {
        _startPos = transform.position;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            _height = sr.bounds.size.y;
        }
    }
    void Update()
    {
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
        if (transform.position.y <= _startPos.y - _height)
        {
            transform.position = _startPos;
        }
    }
}
