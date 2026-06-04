using System;

namespace CoLab.Telemetry
{
    public class GameSession
    {
        private float startTime;
        private string session_id;
        public string SessionID => session_id;
        
        public GameSession(float startTime)
        {
            session_id = "ses" + DateTime.Now.ToString("yyyyMMddHHmmss");
            this.startTime = startTime;
        }

        public float GetRelativeTimestamp(float time)
        {
            return time - startTime;
        }
    }
}