# Unity 2D Hollow Knight-Style Character Controller

Bu proje Unity 2D için Hollow Knight tarzında modüler bir karakter kontrolcüsü ve sağlık sistemi içerir. Tüm sistemler yeni Unity Input System ile tam uyumludur.

## Özellikler

- **Double Jump (Çift Zıplama)**: Havada ikinci zıplama yapabilme
- **Dash (Hızlı Hareket)**: Yönlü kısa mesafe hızlı hareket
- **Melee Attack (Yakın Dövüş)**: Düşmanlara saldırı
- **Health System (Sağlık Sistemi)**: Hasar alma, ölüm ve yeniden doğma
- **Enemy Interaction (Düşman Etkileşimi)**: Düşmanlardan hasar alma

## Kurulum ve Entegrasyon

### 1. Oyuncu Objesi Oluşturma

1. Yeni bir GameObject oluşturun ve "Player" olarak adlandırın
2. Aşağıdaki bileşenleri ekleyin:
   - `Rigidbody2D`
   - `Collider2D` (CircleCollider2D veya CapsuleCollider2D önerilir)
   - `PlayerController` script
   - `PlayerInputHandler` script  
   - `HealthSystem` script

### 2. Input Actions Kurulumu

Input Actions dosyası (`Assets/InputSystem_Actions.inputactions`) zaten yapılandırılmıştır ve şu eylemleri içerir:

- **Move**: WASD / Arrow Keys / Gamepad Left Stick
- **Jump**: Space / Gamepad South Button
- **Dash**: Left Ctrl / Gamepad East Button
- **Attack**: Mouse Left / Enter / Gamepad West Button

`PlayerInputHandler` bileşeninde `Input Actions` alanına bu dosyayı atayın.

### 3. Layer Ayarları

1. Edit > Project Settings > Tags and Layers'a gidin
2. Şu layer'ları oluşturun:
   - `Ground` (Layer 8)
   - `Enemy` (Layer 6)

### 4. Fizik Ayarları

1. Edit > Project Settings > Physics 2D'ye gidin
2. Ground ve Player layer'ları arasında çarpışma olduğundan emin olun
3. Enemy ve Player layer'ları arasında çarpışma (trigger olarak) ayarlayın

## Bileşen Ayarları

### PlayerController

```csharp
[Header("Movement Settings")]
moveSpeed = 8f;          // Yürüme hızı
accelerationTime = 0.1f;  // Hızlanma süresi
decelerationTime = 0.1f;  // Yavaşlama süresi

[Header("Transform References")]
groundCheck;  // Zemin kontrolü için (otomatik oluşturulur)
attackPoint;  // Saldırı noktası (otomatik oluşturulur)
```

### DoubleJumpSystem

```csharp
jumpForce = 12f;        // İlk zıplama gücü
doubleJumpForce = 10f;  // İkinci zıplama gücü
maxJumps = 2;           // Maksimum zıplama sayısı
coyoteTime = 0.1f;      // Zemin bırakıldıktan sonra zıplama süresi
jumpBufferTime = 0.1f;  // Zıplama komutu tamponu
```

### DashSystem

```csharp
dashForce = 20f;       // Dash gücü
dashDuration = 0.2f;   // Dash süresi
dashCooldown = 1f;     // Dash bekleme süresi
maxDashes = 1;         // Maksimum dash sayısı
```

### MeleeAttackSystem

```csharp
attackDamage = 25;        // Saldırı hasarı
attackRange = 1.5f;       // Saldırı menzili
attackCooldown = 0.5f;    // Saldırı bekleme süresi
knockbackForce = 5f;      // Geri tepme gücü
```

### HealthSystem

```csharp
maxHealth = 100;             // Maksimum can
invulnerabilityTime = 1.5f;  // Hasar sonrası dokunulmazlık süresi
```

## Düşman Oluşturma

1. Yeni GameObject oluşturup "Enemy" tag'i verin
2. `EnemyDamager` script ekleyin
3. Collider2D ekleyin ve `Is Trigger = true` yapın
4. Layer'ı "Enemy" olarak ayarlayın

```csharp
damageAmount = 15;      // Oyuncuya vereceği hasar
damageInterval = 1f;    // Hasar verme aralığı
enemyHealth = 50;       // Düşman canı
knockbackForce = 8f;    // Geri tepme gücü
```

## Kontroller

### Klavye + Fare
- **Hareket**: WASD veya Ok Tuşları
- **Zıplama**: Space
- **Dash**: Left Ctrl
- **Saldırı**: Mouse Sol Tık veya Enter

### Gamepad
- **Hareket**: Sol Analog Çubuk
- **Zıplama**: A (South) Butonu
- **Dash**: B (East) Butonu
- **Saldırı**: X (West) Butonu

## Sistem Özellikleri

### Double Jump
- Zeminde iken 1. zıplama, havada iken 2. zıplama
- Coyote Time: Zemin bırakıldıktan kısa süre sonra hala zıplayabilir
- Jump Buffer: Zıplama tuşuna erken basılsa bile zemine değince zıplar

### Dash
- Hareket yönünde dash, input yoksa baktığı yönde dash
- Dash sırasında yerçekimi devre dışı
- Zemine değince dash sayısı sıfırlanır

### Combat
- Saldırı menzili içindeki tüm düşmanlara hasar
- Düşmanlara knockback etkisi
- Saldırı cooldown sistemi

### Health
- Hasar alma ve iyileşme sistemi
- Hasar sonrası geçici dokunulmazlık
- Ölüm ve yeniden doğma

## Event System

Sistemler UnityEvent kullanarak iletişim kurar:

```csharp
// Health Events
OnHealthChanged   // Can değiştiğinde
OnDamageTaken    // Hasar alındığında
OnDeath          // Ölümde
OnHeal           // İyileşmede
```

## Debug Özellikler

- Scene view'da zemin kontrolü görsel (yeşil/kırmızı çizgi)
- Saldırı menzili görsel (sarı daire)
- Console'da sistem durumu logları
- Inspector'da test butonları (sağ tık menüsü)

## Genişletme İpuçları

1. **Animasyon**: Animator Controller ekleyip animasyonları tetikleyin
2. **Sesler**: AudioSource ekleyip ses efektleri oynatın
3. **Partiküller**: Particle System'lar ekleyip efektler oluşturun
4. **UI**: Health bar, dash cooldown göstergeleri ekleyin
5. **Save System**: Oyuncu durumunu kaydetme sistemi ekleyin

## Sorun Giderme

**Oyuncu hareket etmiyor:**
- Rigidbody2D freeze rotation Z'yi kontrol edin
- Input Actions dosyasının atandığından emin olun

**Zıplama çalışmıyor:**
- Ground Check pozisyonunu kontrol edin
- Ground layer ayarlarını kontrol edin

**Düşmanlardan hasar almıyor:**
- Enemy tag ve layer ayarlarını kontrol edin
- Collider trigger ayarlarını kontrol edin

Bu sistem tamamen modüler olarak tasarlandı ve kolayca genişletilebilir. Her sistem bağımsız çalışır ve ihtiyacınıza göre özelleştirilebilir.