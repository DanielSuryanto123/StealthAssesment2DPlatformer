namespace CoLab.Telemetry
{
    public enum EventType
    {
        MOVE_START,
        MOVE_STOP, 
        JUMP_SUCCESS,
        JUMP_FAIL,
        PULL_LEVER,
        INVALID_INTERACT,
        STEP_ON_BUTTON,
        PING_HELP, // untuk minta bantuan, munculkan simbol '?' diatas kepala
        PING_HERE, // untuk kasih tau posisi atau mengarahkan partner
        LEVEL_RESTART
    }
}