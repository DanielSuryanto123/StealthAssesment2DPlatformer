# Telemetry Perspective Taking: Keputusan Scope, Competency, Task, dan Evidence Model

| | |
|---|---|
| Tanggal | 2026-11-25 |
| Status | Scope diputuskan; ambang masih sementara; sudah diimplementasikan dan lolos test PlayMode serta upload uji (lihat bagian 7) |
| Branch | `feature/telemetry-perspective-taking` |
| Kerangka | Evidence-Centered Design; indikator sosial mengacu pada ATC21S (rujukan perlu dicek ke sumber asli) |

## 1. Keputusan

Pencatatan telemetry dikerucutkan menjadi **satu strand, Perspective Taking**, dengan dua indikator: **Adaptive Responsiveness (AR)** dan **Audience Awareness (AA)**.

| | Isi |
|---|---|
| Masuk scope | Perspective Taking: AR1–AR4, AA1–AA3 |
| Keluar scope | Participation, Social Regulation, Task Regulation, Learning and Knowledge Building, Penalaran |
| Tujuan | Memperkecil scope pencatatan |
| Alasan akademik | _(diisi tim riset)_ |

Konsekuensi yang diketahui:

- Bukti AR lebih kuat daripada AA. Jenis ping (`PING_HELP` / `PING_HERE`) dipilih sistem lewat `PlayerController.GetPingEventType`, bukan oleh pemain, sehingga isi pesan tidak bisa menjadi bukti AA.
- Dari pemeriksaan kode, belum terlihat informasi asimetris antar pemain: mode kamera BlindSync tidak dipanggil dari kode dan `CorridorReveal` (Level 7) membuka area untuk kedua pemain.
- Komunikasi lisan antar pemain tidak tertangkap log.

## 2. Competency Model

```
Perspective Taking
├── Adaptive Responsiveness (AR)
│   ├── AR1  Merespons ping partner
│   ├── AR2  Kecepatan menolong partner
│   ├── AR3  Menyusul partner saat sinkronisasi
│   └── AR4  Menyesuaikan setelah menarik lever terlalu cepat
└── Audience Awareness (AA)
    ├── AA1  Perilaku setelah ping diabaikan
    ├── AA2  Relevansi posisi ping
    └── AA3  Menunggu partner di titik bersama
```

| ID | Klaim tentang pemain |
|---|---|
| AR | Memperhatikan kontribusi dan kondisi partner, lalu menerima atau menyesuaikan tindakannya (bukan mengabaikan). |
| AR1 | Menanggapi sinyal yang dikirim partner. |
| AR2 | Segera bertindak ketika partner terjebak. |
| AR3 | Menyelaraskan aksinya dengan aksi partner. |
| AR4 | Mengubah waktu aksinya setelah kondisi partner ternyata belum siap. |
| AA | Menyesuaikan perilakunya agar sesuai dengan kebutuhan partner. |
| AA1 | Mengubah cara memberi sinyal ketika sinyal sebelumnya tidak ditanggapi. |
| AA2 | Memberi sinyal di tempat yang berguna bagi partner. |
| AA3 | Memberi partner waktu untuk sampai dan siap. |

Setiap pemain dinilai terpisah.

## 3. Task Model

Mekanik dan aksi di game yang memunculkan bukti.

| ID | Mekanik | Kesempatan muncul ketika | Aksi pemain | Level | Script |
|---|---|---|---|---|---|
| AR1 | Ping | Partner mengirim ping | Mendekati posisi ping | 1–8 | `PlayerController` |
| AR1 | Lever pintu | Partner mengirim ping | Menarik lever | 2, 8 | `LeverController` |
| AR1 | Switch sinkron | Partner mengirim ping | Menekan switch | 3, 4, 8 | `Level3SyncSwitch`, `BlindSyncSwitch` |
| AR1 | Lever penyelamat | Partner mengirim ping | Menarik lever | 5, 6, 8 | `RescueLever`, `Level6DeadzoneLever` |
| AR1 | Tombol platform | Partner mengirim ping | Memanggil platform | 6, 7, 8 | `Level6PlatformCallButton`, `Level7PlatformCallButton` |
| AR1 | Tombol puzzle | Partner mengirim ping | Menekan tombol | 7 | `Level7Button` |
| AR2 | Pitfall | P2 terjebak | P1 menarik lever penyelamat | 5 | `PitfallRescueManager`, `RescueLever` |
| AR2 | Deadzone | P1 terjebak | P2 menarik lever deadzone | 6 | `Level6DeadzoneTrigger`, `Level6DeadzoneLever` |
| AR2 | Deadzone | P2 terjebak | P1 menarik lever deadzone | 8 | `Level6DeadzoneTrigger`, `Level6DeadzoneLever` |
| AR3 | Blind sync (window 5 detik) | Partner menekan switch lebih dulu | Menekan switch pasangannya | 4 | `BlindSyncManager` |
| AR3 | Switch sinkron (tanpa window) | Partner menekan switch lebih dulu | Menekan switch pasangannya | 3, 8 | `Level3SyncManager`, `Level3SyncSwitch` |
| AR4 | Pitfall dan platform penyelamat | P1 menarik lever saat P2 terjebak tetapi belum di platform | Menunggu P2 siap, lalu menarik lagi | 5 | `PitfallRescueManager.ActivateRescue` |
| AA1 | Ping | Ping pemain tidak ditanggapi | Pindah posisi atau mendekat, lalu ping lagi | 1–8 | `PlayerController` |
| AA2 | Ping | Pemain mengirim ping | Memilih tempat mengirim ping (ping muncul di posisi pengirim) | 1–8 | `PlayerController.DoPing` |
| AA3 | Portal akhir | Pemain tiba di portal lebih dulu | Tetap di portal sampai partner datang | 1–8 | `EndPortal` |
| AA3 | Switch sinkron | Pemain tiba atau menekan lebih dulu | Bertahan sampai partner siap | 3, 4, 8 | `Level3SyncSwitch`, `BlindSyncSwitch` |
| AA3 | Platform | Pemain tiba di platform lebih dulu | Menunggu partner naik | 6, 7, 8 | `Level6MovingPlatform`, `Level7MovingPlatform` |

### Cakupan per level

| Level | AR1 | AR2 | AR3 | AR4 | AA1 | AA2 | AA3 |
|---|---|---|---|---|---|---|---|
| 1 | ✓ | | | | ✓ | ✓ | ✓ |
| 2 | ✓ | | | | ✓ | ✓ | ✓ |
| 3 | ✓ | | ✓ | | ✓ | ✓ | ✓ |
| 4 | ✓ | | ✓ | | ✓ | ✓ | ✓ |
| 5 | ✓ | P1 | | P1 | ✓ | ✓ | ✓ |
| 6 | ✓ | P2 | | | ✓ | ✓ | ✓ |
| 7 | ✓ | | | | ✓ | ✓ | ✓ |
| 8 | ✓ | P1 | ✓ | | ✓ | ✓ | ✓ |

AR2 dan AR4 tidak seimbang antar pemain: P1 punya dua kesempatan AR2 (Level 5, 8), P2 satu (Level 6), dan hanya P1 yang bisa mendapat bukti AR4.

## 4. Evidence Model

### 4.1 Aturan umum

1. Tidak ada kesempatan berarti **tidak ada bukti** (node kosong), bukan kategori Rendah.
2. **Satu observasi per node per level per pemain.**
3. Game mencatat data mentah; kategori dihitung saat analisis.
4. Simbol `N`, `X`, `T`, `d`, `k` adalah ambang yang **belum ditetapkan**.

### 4.2 Aturan bukti

| ID | Pemain yang dinilai | Yang diukur | Data log | Cara hitung per level | Rendah | Sedang | Tinggi |
|---|---|---|---|---|---|---|---|
| AR1 | Penerima ping | Ditanggapi atau tidak, dan waktu respons | `PING_*`, `PING_RESPONSE` | Median waktu respons; ping tanpa respons dihitung diabaikan | Diabaikan | Merespons > `T` detik | Merespons ≤ `T` detik |
| AR2 | Penolong | Selang dari partner terjebak sampai ditolong | `RESCUE_START`, `RESCUE_TIME`, `PULL_LEVER` | Median `RESCUE_TIME.duration` | Lambat | Sedang | Cepat |
| AR3 | Pemain yang menyusul | Berhasil menyusul, jumlah gagal, selisih waktu tekan | `STEP_ON_BUTTON`, `STEP_OFF_BUTTON`, `SYNC_ACTION`, `SYNC_FAIL` | Jumlah `SYNC_FAIL` sebelum `SYNC_ACTION` | ≥ `k` gagal atau tidak pernah berhasil | Berhasil setelah < `k` gagal | Berhasil pada percobaan pertama |
| AR4 | Penolong (P1) | Jumlah tarikan terlalu cepat sebelum berhasil | `RESCUE_ATTEMPT_EARLY`, `RESCUE_TIME` | Hitung percobaan dini; kosong jika tidak ada | Terus mengulang atau tidak berhasil | Berhasil setelah beberapa percobaan dini | Berhasil tepat setelah satu percobaan dini |
| AA1 | Pengirim ping | Perubahan posisi antara ping yang diabaikan dan ping berikutnya | `PING_*` (`player_pos`, `partner_pos`, objek), `PING_RESPONSE` | Pasangan pertama "diabaikan lalu ping lagi" | Pindah < `d` | Pindah ≥ `d` tetapi tidak mendekat | Mendekat ke partner atau target |
| AA2 | Pengirim ping | Jarak posisi ping ke objek yang dibutuhkan partner | `PING_*` (`object_id`, `object_distance`) | Median jarak ke target | Jauh | Cukup dekat | Tepat di target |
| AA3 | Pemain yang tiba lebih dulu | Apakah dan berapa lama bertahan sampai partner siap | `PORTAL_ENTER`, `PORTAL_EXIT`, `STEP_ON_BUTTON`, `STEP_OFF_BUTTON`, `WAIT_PARTNER` | Hasil tunggu pertama di tiap level | Pergi sebelum partner datang | Menunggu sebentar, pergi sebelum partner siap | Menunggu sampai partner siap |

Definisi pendukung:

- **Respons ping (AR1):** dalam `N` detik partner mendekati posisi ping minimal `X` unit, atau berinteraksi dengan objek di sekitar posisi itu. Jendela juga tertutup saat pengirim mengirim ping baru (cooldown ping 3 detik). Jika level berakhir sebelum jendela tertutup, ping tidak dihitung.
- **`SYNC_FAIL`:** di Level 4 tercatat atas nama penekan pertama; saat analisis, bukti AR3 diberikan kepada **partnernya**.
- **Sync di Level 3 dan 8:** percobaan gagal adalah penekan pertama meninggalkan switch (`STEP_OFF_BUTTON`) sebelum partner menekan.

### 4.3 Daftar event yang dicatat

Status terhadap versi sebelum implementasi: **Ada** = sudah dicatat; **Ubah** = diubah; **Baru** = ditambahkan. Seluruh perubahan di tabel ini dan di 4.4 sudah ada di kode.

| Event | Status | Dicatat ketika | `player_id` | Perubahan yang diperlukan | Node |
|---|---|---|---|---|---|
| `PING_HELP`, `PING_HERE` | Ubah | Pemain mengirim ping | Pengirim | Tambah `object_id`, `object_distance` | AR1, AA1, AA2 |
| `PING_RESPONSE` | Baru | Respons ping terpenuhi | Penerima | `duration` = waktu respons | AR1, AA1 |
| `RESCUE_START` | Ada | Pemain terjebak | Korban | | AR2 |
| `RESCUE_TIME` | Ubah | Penyelamatan berhasil | Penolong | Level 5 saat ini mencatat korban | AR2, AR4 |
| `RESCUE_ATTEMPT_EARLY` | Baru | Lever ditarik saat korban belum di platform | Penolong | Cabang gagal di `ActivateRescue` | AR4 |
| `PULL_LEVER` | Ubah | Lever ditarik | Penarik | Baru ada di `RescueLever`; tambah di `LeverController`, `Level6DeadzoneLever` | AR1, AR2 |
| `STEP_ON_BUTTON` | Ubah | Switch atau tombol ditekan | Penekan | Tambah `object_id` | AR1, AR3, AA3 |
| `STEP_OFF_BUTTON` | Ubah | Switch dilepas atau ditinggalkan | Penekan | Baru ada di Level 4; tambah di Level 3, 8 | AR3, AA3 |
| `SYNC_ACTION` | Ubah | Sinkronisasi berhasil | Pemain yang menyusul | `duration` = selisih waktu tekan; Level 3, 8 saat ini mencatat kedua pemain | AR3 |
| `SYNC_FAIL` | Ubah | Sinkronisasi gagal | Penekan pertama | Tambah di Level 3, 8 | AR3 |
| `CALL_PLATFORM` | Baru | Platform dipanggil | Pemanggil | | AR1 |
| `PORTAL_ENTER`, `PORTAL_EXIT` | Baru | Pemain masuk atau keluar portal | Pemain | | AA3 |
| `WAIT_PARTNER` | Ubah | Diam > 3 detik saat partner bergerak | Yang diam | Tambah `object_id` titik bersama | AA3 |

**Event pendukung (usulan, menunggu persetujuan):** `MOVE_START` dan `MOVE_STOP` dipertahankan selama pilot. Tanpa data gerak, ambang `N`, `X`, dan `d` tidak bisa dihitung ulang setelah data terkumpul.

**Berhenti dicatat:** `JUMP_SUCCESS`, `JUMP_FAIL`, `LEVEL_RESTART`, `HAZARD_ZONE_ENTERED`, `INVALID_INTERACT`, `RELEASE_LEVER`. Cukup hapus pemanggilannya; nilai enum dibiarkan.

### 4.4 Field log

| Field | Status | Isi |
|---|---|---|
| `stage_id` | Ada | Level |
| `timestamp` | Ada | Detik sejak sesi dimulai |
| `event_type` | Ada | Nama event |
| `duration` | Ubah | Disimpan untuk `WAIT_PARTNER`, `RESCUE_TIME`, `PING_RESPONSE`, `SYNC_ACTION` (juga `LEVEL_RESTART`, yang tidak lagi dicatat) |
| `player_id` | Ada | Pemain pelaku |
| `player_pos`, `partner_pos` | Ada | Posisi saat event |
| `isTrapped` | Ada | Status terjebak pelaku |
| `object_id` | Baru | Objek terkait (switch, lever, tombol, platform, portal); untuk ping, objek interaktif terdekat |
| `object_distance` | Baru | Jarak pelaku ke objek tersebut |

## 5. Prasyarat sebelum pengambilan data

- [x] Pengiriman data ke tabel `telemetry_stage_logs` diterima server (`supabase/telemetry_stage_logs.sql`).
- [x] Flag `Level3SyncManager` di-reset saat switch ditinggalkan.
- [x] `duration` disimpan untuk event baru (lihat 4.4).
- [x] Test PlayMode di bagian 7 lulus.
- [ ] Uji main manual oleh dua orang.
- [ ] Sesi dimulai dari MainMenu (`AnalyticsManager` hanya ada di MainMenu, Level6, dan Result).

## 6. Keputusan yang masih terbuka

| # | Pertanyaan |
|---|---|
| 1 | Nilai ambang `N`, `X`, `T`, `d`, `k`, serta batas cepat/lambat AR2 dan jauh/dekat AA2 (pakar atau persentil data pilot) |
| 2 | Daftar target yang relevan per level untuk AA2 |
| 3 | AR4: pemain yang tidak pernah terlalu cepat dibiarkan kosong (aturan saat ini) atau dinilai Tinggi |
| 4 | Apakah pemain memilih sendiri jenis ping (dua tombol) |
| 5 | Apakah peran penyelamat dibuat seimbang antar pemain |
| 6 | Cara mendeteksi pemain pergi dari platform untuk AA3 (belum ada event keluar area platform) |
| 7 | Persetujuan mempertahankan `MOVE_START` / `MOVE_STOP` selama pilot |
| 8 | Apakah window sync Level 4 tetap 5 detik (komentar di `EventType.cs` menyebut 0,5 detik) |
| 9 | Level 8 memiliki `PitfallRescueManager` tanpa trigger dan lever: dilengkapi atau dihapus |

## 7. Implementasi dan pengujian

### Ambang sementara

Dapat diubah di Inspector `AnalyticsManager`.

| Parameter | Nilai | Arti |
|---|---|---|
| `pingResponseWindow` (`N`) | 5 detik | Batas waktu respons ping |
| `pingResponseDistance` (`X`) | 2 unit | Minimal partner mendekat |
| `nearbyObjectRadius` | 3 unit | Radius "objek di sekitar ping" dan titik bersama `WAIT_PARTNER` |

`object_id` berisi path objek di hierarki beserta urutannya, misalnya `Lever[3]`.

### Keterbatasan yang diketahui

- Level 6 dan 8: pemain yang terjebak tidak bisa ping karena `Level6DeadzoneTrigger` mematikan `PlayerController`. Tombol ping yang ditekan saat terjebak baru terkirim setelah dibebaskan.
- Partner yang sudah berada kurang dari `X` unit dari posisi ping hanya bisa dianggap merespons lewat interaksi.
- Partner yang terbawa platform mendekati posisi ping ikut dihitung merespons.
- Level 1–6 dan 8 memiliki dua komponen `EndPortal` pada objek portal; hanya yang pertama mencatat.

### Status pengujian (2026-11-25)

| Pemeriksaan | Hasil |
|---|---|
| Kompilasi script game dan test (.NET SDK terhadap DLL Unity) | Lolos |
| Komponen pencatat per level (analisis file scene dan prefab) | Sesuai task model |
| Test evidence PlayMode lewat `unity test` | 24 lulus, 0 gagal |
| End-to-end Level 1–8 sampai `Result` dengan upload (sesi `ses20261125224721`) | Lulus: 16 file (8 stage × 2 pemain) diterima, antrean kosong |

Catatan: Windows Smart App Control memblokir `NiceIO.dll` milik Unity 6000.4.5f1; test baru bisa jalan setelah dimatikan.

Cakupan: `PING_HELP` tidak teruji. Test memindahkan pemain langsung ke objek, jadi tidak membuktikan puzzle bisa diselesaikan secara normal.

Menjalankan test (Editor harus tertutup):

```
unity test . --mode PlayMode --filter "CoLab.Telemetry.EvidenceTests"
```

Test ada di `Assets/Apps/Tests/PlayMode/TelemetryEvidenceTests.cs`: memuat scene asli, memindahkan pemain ke tiap objek, menyuntikkan input, lalu memeriksa log. Hasil JSON disimpan di `Logs/evidence/`. Test mengosongkan API key sehingga tidak mengunggah data, kecuali `PlayAllLevels_UploadsToSupabase` bila environment variable `TELEMETRY_UPLOAD=1` (baris uji ber-`student_id` `TEST_E2E_*`).

Selalu pakai `--filter` tersebut: `SendData_SuccessfullyPushesData` di test lama mengirim data ke Supabase production.

Pengolahan lanjutan dengan Bayesian network (struktur, CPT, validasi) belum termasuk dokumen ini.
