using System;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Posisi dari player, hanya menyimpan koordinat x dan y
    /// </summary>
    [Serializable]
    public struct Position
    {
        public float x;
        public float y;

        public Position(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}