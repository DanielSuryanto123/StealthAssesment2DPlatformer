-- Jalankan sekali di Supabase Dashboard > SQL Editor.
-- Satu baris = satu pemain pada satu stage (semua percobaan digabung).
create table public.telemetry_stage_logs (
    id          bigint generated always as identity primary key,
    created_at  timestamptz not null default now(),
    session_id  text  not null,
    student_id  text  not null,
    player_id   text  not null,
    stage_id    text  not null,
    raw_logs    jsonb not null,
    -- Kirim ulang baris yang sama dijawab 409, dan game menganggapnya sudah terkirim.
    unique (session_id, student_id, player_id, stage_id)
);

-- Key publishable ada di dalam build game, jadi hanya boleh insert.
alter table public.telemetry_stage_logs enable row level security;

revoke all on public.telemetry_stage_logs from anon, authenticated;
grant insert on public.telemetry_stage_logs to anon;

create policy "game boleh insert"
    on public.telemetry_stage_logs
    for insert to anon
    with check (true);
