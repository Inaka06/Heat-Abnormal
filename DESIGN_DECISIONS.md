# Heat Abnormal — Design Decisions

Dokumen ini mencatat keputusan default yang diambil saat dokumen desain belum menentukan detail implementasi.

1. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Arah efek waktu pembangunan pada P8-T3/P9-T3 ambigu — Event alam dan pilihan event rakyat memakai perubahan periode: nilai positif memundurkan progress, nilai negatif memajukan progress — Representasi ini konsisten dengan progress berbasis periode terpakai dan mencegah progress melebihi batas.
2. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Modal event perlu menghentikan atau membiarkan waktu berjalan — `GameClock` di-pause selama modal event terbuka, lalu resume setelah tombol `Lanjut` — Pemain mendapat waktu yang adil untuk membaca dan memilih dialog.
3. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Nominal request penambahan dana tidak ditentukan — Sedikit=10%, Sedang=20%, Banyak=35% dari `biayaDipilih` pembangkit — Persentase mudah dipahami pemain dan disimpan sebagai konstanta yang mudah di-tweak.
4. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Cooldown request dana tidak ditentukan — Maksimal satu request per periode — Mencegah spam dan membuat pilihan tier menjadi keputusan strategis.
5. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Cooldown retry penyambungan tidak ditentukan — Retry boleh dilakukan setelah hasil sebelumnya selesai, tanpa limit jumlah retry — Ini mempertahankan kemungkinan comeback dan tetap dibatasi oleh game over dana.
6. `Assets/_HeatAbnormal/Scripts/Formulas/GameFormulas.cs` — Makna `P` pada formula peluang penyambungan ambigu — `P` ditafsirkan sebagai total lobby tiga politikus, bukan kekuatan politik yang sedang berjalan — Ini mengikuti keterangan Step 5 yang menyebut total lobby.
7. `Assets/_HeatAbnormal/Scripts/UI/PembangunanLoopController.cs` — Detail transisi request penyambungan tidak menentukan delay — Dipakai delay UI satu detik dengan status `PENDING` — Memberi feedback proses tanpa mengubah hasil kalkulasi.
