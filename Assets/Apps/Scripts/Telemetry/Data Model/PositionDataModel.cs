using System;

namespace CoLab.Telemetry
{
    /// <summary>
    /// Posisi dari player, hanya menyimpan koordinat x dan y
    /// </summary>
    [Serializable]
    public struct PositionDataModel
    {
        public float x;
        public float y;

        public PositionDataModel(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}