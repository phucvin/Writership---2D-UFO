using UnityEngine;

public static class Ops
{
    public struct PickUp
    {
        public CompletePlayerController Player;
        public CompletePickUp Item;
    }

    public struct Hit
    {
        public CompletePlayerController FromPlayer;
        public CompleteBullet WithBullet;
        public GameObject ToEnemy;
    }
}
