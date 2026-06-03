using System;

namespace CoLab.Telemetry
{
    public class GameSession
    {
        private float startTime;
        private string sessionID;
        public string SessionID => sessionID;
        
        public GameSession(float startTime)
        {
            sessionID = "ses" + DateTime.Now.ToString("yyyyMMddHHmmss");
            this.startTime = startTime;
        }

        public float GetRelativeTimestamp(float time)
        {
            return time - startTime;
        }
    }
}