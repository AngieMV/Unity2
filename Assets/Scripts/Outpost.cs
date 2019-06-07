using System.Collections.Generic;
using UnityEngine;

public class Outpost : MonoBehaviour
{
    public static List<Outpost> OutpostList = new List<Outpost>();

    public float CaptureValue { get; private set; }
    public int CurrentTeam { get; private set; }

    [Tooltip("Time is in Seconds")]
    [SerializeField]
    private float _CaptureTime = 5f;

    private SkinnedMeshRenderer _FlagRenderer;

    private void OnEnable()
    {
        OutpostList.Add(this);
        _FlagRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void OnDisable()
    {
        OutpostList.Remove(this);
    }

    private void OnTriggerStay(Collider other) //every physics tick -> 50 t/s
    {
        var unit = other.GetComponent<Unit>();
        if(unit == null || unit.IsAlive == false) return;

        if(unit.TeamNumber == CurrentTeam)
        {
            CaptureValue += Time.fixedDeltaTime / _CaptureTime;
            if(CaptureValue > 1f) CaptureValue = 1f;
        }
        else
        {
            CaptureValue -= Time.fixedDeltaTime / _CaptureTime;
            if(CaptureValue <= 0)
            {
                CaptureValue = 0f;
                CurrentTeam = unit.TeamNumber;
            }
        }
    }

    private void Update()
    {
        var teamColor = GameManager.Instance.TeamColors[CurrentTeam];
        _FlagRenderer.material.color = Color.Lerp(Color.white, teamColor, CaptureValue);
    }


}
