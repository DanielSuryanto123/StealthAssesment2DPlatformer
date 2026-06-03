namespace CoLab.Telemetry
{
    public enum EventType
    {
        // Aksi Navigasi Dasar
        MOVE_START,
        MOVE_STOP, 
        JUMP_SUCCESS,
        JUMP_FAIL,
        LEVEL_RESTART, // untuk metrik Ketekunan (Perseverance)

        // Aksi Interaksi Objek
        PULL_LEVER,
        RELEASE_LEVER,
        INVALID_INTERACT, // mencoba berinteraksi dengan objek warna milik lawannya
        STEP_ON_BUTTON,
        STEP_OFF_BUTTON,

        // Aksi Sistem Sinyal / Komunikasi Non-Verbal
        PING_HELP, // untuk minta bantuan, munculkan simbol '?' diatas kepala
        PING_HERE, // untuk kasih tau posisi atau mengarahkan partner

        // Aksi Logika Otomatis dari Sistem (Stealth Assessment)
        SYNC_ACTION,   // P1 dan P2 menginjak tombol dengan selisih waktu <= 0.5 detik
        WAIT_PARTNER,  // Satu pemain diam > 3 detik saat partnernya bergerak
        RESCUE_TIME    // Durasi dari saat partner terjebak hingga tuas penyelamat ditarik
    }
}