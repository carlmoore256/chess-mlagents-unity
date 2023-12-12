using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTarget : MonoBehaviour
{
    public Color whiteWinColor;
    public Color blackWinColor;
    public Color neutralColor;

    private Renderer _renderer;

    private Coroutine _colorCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = neutralColor;
    }

    public void WinForTeam(Team team)
    {
        if (team == Team.White)
        {
            _renderer.material.color = whiteWinColor;
        }
        else
        {
            _renderer.material.color = blackWinColor;
        }
        if (_colorCoroutine != null)
            StopCoroutine(_colorCoroutine);
        _colorCoroutine = CoroutineHelpers.DelayedAction(() => {
            _renderer.material.color = neutralColor;
        }, 0.4f, this);
    }


    
}
