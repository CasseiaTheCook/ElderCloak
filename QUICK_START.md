# Hızlı Başlangıç Rehberi - Hollow Knight Tarzı Karakter Kontrolcüsü

Bu rehber size character controller'ı hızlıca test etmeniz için gerekli adımları gösterir.

## Adım 1: Oyuncu Objesi Oluşturma

1. Unity Editor'ü açın
2. Hierarchy'de sağ tık → Create Empty
3. Objeyi "Player" olarak adlandırın
4. Player objesini seçin
5. Inspector'da **Add Component** butonuna tıklayın
6. `PlayerSetupValidator` scripti ekleyin

## Adım 2: Otomatik Kurulum

1. `PlayerSetupValidator` bileşeninde **Auto Setup On Start** seçeneğinin aktif olduğundan emin olun
2. Play butonuna basın
3. Console'da kurulum mesajlarını kontrol edin

**VEYA**

Inspector'da `PlayerSetupValidator` bileşeninin sağ üst köşesindeki menüden:
- **Validate Player Setup**: Kurulumu kontrol et ve eksik bileşenleri ekle
- **Setup Example Scene**: Test ortamı oluştur (zemin + düşman)
- **Create Test Enemy**: Sadece test düşmanı oluştur

## Adım 3: Input Actions Atama

1. Player objesini seçin
2. Inspector'da `Player Input Handler` bileşenini bulun
3. **Input Actions** alanına `Assets/InputSystem_Actions.inputactions` dosyasını sürükleyin

## Adım 4: Tag ve Layer Ayarları

### Tags Oluşturma:
1. Edit → Project Settings → Tags and Layers
2. **Tags** bölümünde:
   - `Player` tag'i oluşturun
   - `Enemy` tag'i oluşturun

### Layers Oluşturma:
1. Aynı pencerede **Layers** bölümünde:
   - Layer 6: `Enemy`
   - Layer 8: `Ground`

### Player Tag Atama:
1. Player objesini seçin
2. Inspector'ın üst kısmında **Tag** dropdown'dan `Player` seçin

## Adım 5: Test Et!

Scene'i çalıştırın ve aşağıdaki kontrolleri test edin:

### Klavye Kontrolleri:
- **WASD**: Hareket
- **Space**: Zıplama (çift zıplama için iki kez bas)
- **Left Ctrl**: Dash
- **Mouse Sol Tık** veya **Enter**: Saldırı

### Gamepad Kontrolleri:
- **Sol Analog**: Hareket  
- **A Butonu**: Zıplama
- **B Butonu**: Dash
- **X Butonu**: Saldırı

## Hızlı Test Sahnesu

Eğer `PlayerSetupValidator` ile **Setup Example Scene** seçeneğini kullandıysanız:

1. ✅ Yeşil platform (zemin) - üzerinde yürüyebilirsiniz
2. ✅ Kırmızı küp (düşman) - temas ettiğinizde hasar alırsınız
3. ✅ Düşmana saldırı yapabilirsiniz

## Sorun Giderme

**Oyuncu hareket etmiyor:**
- Input Actions dosyasının atandığından emin olun
- Console'da hata mesajları kontrol edin

**Zıplayamıyorum:**
- Ground layer'ının doğru ayarlandığından emin olun
- Zemin objelerinin Ground layer'ında olduğundan emin olun

**Düşmandan hasar alamıyorum:**
- Enemy tag'lerinin doğru atandığından emin olun
- Player tag'inin atandığından emin olun

## İleri Seviye Özelleştirme

Sistemi özelleştirmek için:

1. **PlayerController** bileşenindeki değerleri ayarlayın:
   - `Move Speed`: Hareket hızı
   - `Acceleration Time`: Hızlanma süresi

2. **DoubleJumpSystem** ayarları:
   - `Jump Force`: Zıplama gücü
   - `Max Jumps`: Maksimum zıplama sayısı

3. **DashSystem** ayarları:
   - `Dash Force`: Dash gücü
   - `Dash Duration`: Dash süresi

4. **HealthSystem** ayarları:
   - `Max Health`: Maksimum can
   - `Invulnerability Time`: Dokunulmazlık süresi

Detaylı bilgi için `Assets/Scripts/README.md` dosyasına bakın.

## Başarılı Kurulum Kontrol Listesi

- [ ] Player objesi oluşturuldu
- [ ] PlayerSetupValidator çalıştırıldı
- [ ] Input Actions atandı  
- [ ] Player tag atandı
- [ ] Ground ve Enemy layerları oluşturuldu
- [ ] Test sahnesinde hareket edebiliyor
- [ ] Zıplama çalışıyor
- [ ] Dash çalışıyor
- [ ] Saldırı çalışıyor
- [ ] Düşmanlardan hasar alıyor

Tüm kontroller tamamlandıysa sistem hazır! 🎉