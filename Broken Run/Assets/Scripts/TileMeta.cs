using UnityEngine;

public enum TileKind { Flat, UpSlope, DownSlope }

public class TileMeta : MonoBehaviour
{
    public TileKind kind = TileKind.Flat;
}
