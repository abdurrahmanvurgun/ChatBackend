# Git Kullanım Rehberi (Proje için)

Bu rehber, projede çalışırken sık ihtiyaç duyulacak Git komutlarını, iş akışını ve yaygın hataların çözümlerini içerir. Adımlar ve örnekler macOS / zsh varsayımıyla verilmiştir.

## 1. Başlangıç: Kullanıcı bilgileri
Kendi bilgisayarınızda git commit'leri doğru author ile yapmak için global kimliğinizi ayarlayın:

```bash
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```

Eğer daha önce commit yapıp otomatik atanan author bilgisini düzeltmek isterseniz:

```bash
# En son commit'in yazarını değiştir
git commit --amend --reset-author
```

## 2. Temel iş akışı (feature branch ile)
- Ana dalı güncelleyin:

```bash
git checkout master
git pull origin master
```

- Yeni bir feature branch oluşturun:

```bash
git checkout -b feat/my-feature
```

- Değişiklik yapın, dosyaları ekleyin ve commit atın:

```bash
git add .
git commit -m "Açıklayıcı commit mesajı"
```

- Uzaktaki repoya gönderin:

```bash
git push -u origin feat/my-feature
```

- GitHub/GitLab üzerinde PR/MR oluşturun ve kod incelemesi sonrası `master`a merge edin.

## 3. Commit düzenleme / squash
- Commit mesajınızı düzenlemek için:

```bash
git commit --amend
```

- Birden çok commit'i tek commit'te birleştirmek için rebase interaktif:

```bash
git rebase -i HEAD~3
```

## 4. Yaygın hatalar ve çözümleri
- "Your name and email address were configured automatically..." mesajı
  - `git config --global user.name`/`user.email` ile düzeltin veya `git commit --amend --reset-author` çalıştırın.

- Pre-commit hook hatası
  - Hata mesajını okuyun (lint, test vs.). Geçici olarak hook'u atlamak için:

```bash
git commit --no-verify -m "WIP"
```

- Merge conflict
  - Çakışan dosyaları açıp çözün, sonra:

```bash
git add <file>
git rebase --continue   # veya git merge --continue
```

- "Nothing to commit, working tree clean" ama değişiklik görünmüyor
  - Yeni dosya eklediyseniz `git add` yapmayı unutmayın.

## 5. Geri alma
- Son commit'i geri almak (local):

```bash
git reset --soft HEAD~1  # commit'i geri alır, değişiklikleri sahnede bırakır
git reset --hard HEAD~1  # commit'i ve değişiklikleri tamamen siler
```

- Uzakta gönderilen commit'i geri almak
  - Eğer public branch ise `revert` kullanın:

```bash
git revert <commit>
```

## 6. SSH anahtarları ve push yetkisi
- Git push sırasında access hatası alırsanız, SSH anahtarınızın GitHub/GitLab hesabınıza eklendiğinden veya HTTPS ile token kullandığınızdan emin olun.

## 7. Proje özel ipuçları
- Migration/derleme sonrası büyük değişiklikler commit edilirken açıklayıcı mesaj kullanın: `feat:` `fix:` `chore:` gibi
- `docs/` içindeki değişiklikleri de commit etmeyi unutmayın (dokümantasyon test için önemlidir)

---

Eğer isterseniz bu dosyayı repoya commit edip pushlayayım, veya `git` config ayarlarınızı uygulayıp son commit'in author bilgisini düzelteyim. Hangi işlemi yapmamı istersiniz?