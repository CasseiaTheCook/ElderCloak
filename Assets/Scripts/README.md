# Hollow Knight Benzeri Karakter Kontrolcüsü - Entegrasyon Rehberi

Bu rehber, Hollow Knight benzeri karakter kontrolcüsü sistemini Unity projenize nasıl entegre edeceğinizi açıklar.

## Gereksinimler

- Unity 6000.2.4f1 veya daha yeni
- Unity Input System paketi (1.14.2+)
- 2D Physics sistemi

## Kurulum Adımları

### 1. Input System Kurulumu

Projenizde zaten `InputSystem_Actions.inputactions` dosyası mevcut ve aşağıdaki aksiyonları içeriyor:
- Move (WASD/Arrow Keys, Gamepad Stick)
- Jump (Space, Gamepad A Button)
- Attack (Mouse Left Click, Gamepad X Button)
- Dash (Right Shift, Gamepad RB)

### 2. Oyuncu Objesi Oluşturma

1. Yeni bir GameObject oluşturun ve `Player` olarak adlandırın
2. Aşağıdaki komponentleri ekleyin:
   - `Rigidbody2D`
   - `Collider2D` (BoxCollider2D veya CapsuleCollider2D önerilir)
   - `SpriteRenderer` (karakter sprite'ı için)
   - `PlayerInput` (Input System'den)
   - `PlayerManager`
   - `HealthSystem`
   - `PlayerController2D`
   - `MeleeAttack`

### 3. Komponent Ayarları

#### PlayerInput Komponent Ayarları:
- Actions: `InputSystem_Actions` dosyasını atayın
- Behavior: "Send Messages" veya "Invoke Unity Events" seçin

#### Rigidbody2D Ayarları:
- Gravity Scale: 3-4 arası
- Freeze Rotation Z: True (karakterin dönmesini engeller)
- Linear Drag: 0
- Angular Drag: 0.05

#### PlayerController2D Ayarları:
- Move Speed: 8
- Jump Force: 16
- Dash Force: 20
- Max Jumps: 2 (double jump için)
- Ground Layer Mask: Ground layer'ını seçin

#### HealthSystem Ayarları:
- Max Health: 100
- Invulnerability Time: 0.5

#### MeleeAttack Ayarları:
- Attack Damage: 25
- Attack Range: 1.5
- Attack Cooldown: 0.5
- Enemy Layer Mask: Enemy layer'ını seçin

### 4. Layer Kurulumu

Unity'de aşağıdaki layer'ları oluşturun:
- `Player` (layer 8)
- `Enemy` (layer 9) 
- `Ground` (layer 10)

Player objesini `Player` layer'ına atayın.

### 5. Düşman Oluşturma

1. Yeni GameObject oluşturun ve `Enemy` olarak adlandırın
2. Aşağıdaki komponentleri ekleyin:
   - `Rigidbody2D`
   - `Collider2D` (IsTrigger: true olabilir)
   - `SpriteRenderer`
   - `EnemyDamager`
   - `BasicEnemy` (düşmanın hasar alabilmesi için)

#### EnemyDamager Ayarları:
- Contact Damage: 20
- Player Layer Mask: Player layer'ını seçin

Enemy objesini `Enemy` layer'ına atayın.

### 6. Ground/Platform Oluşturma

1. Ground objelerinizi `Ground` layer'ına atayın
2. Collider2D komponentlerinin olduğundan emin olun

## Kullanım

### Kontroller:
- **Hareket**: WASD veya Ok Tuşları
- **Zıplama**: Space (Double Jump desteklenir)
- **Dash**: Right Shift (yönlü hızlı hareket)
- **Saldırı**: Sol Mouse Click veya Enter

### Özelleştirme

#### Hareket Değerlerini Ayarlama:
```csharp
PlayerController2D controller = GetComponent<PlayerController2D>();
// Hareket hızını değiştir
controller.moveSpeed = 10f;
// Zıplama kuvvetini değiştir
controller.jumpForce = 18f;
// Dash kuvvetini değiştir
controller.dashForce = 25f;
```

#### Sağlık Sistemini Kullanma:
```csharp
HealthSystem health = GetComponent<HealthSystem>();
// Hasar ver
health.TakeDamage(25);
// İyileştir
health.Heal(50);
// Tam iyileştir
health.FullHeal();
```

#### Event'leri Dinleme:
```csharp
HealthSystem health = GetComponent<HealthSystem>();
health.OnDamageTaken.AddListener((damage, source) => {
    Debug.Log($"Oyuncu {damage} hasar aldı!");
});
health.OnDeath.AddListener(() => {
    Debug.Log("Oyuncu öldü!");
});
```

## Sistem Özellikleri

### Modüler Tasarım
- Her komponent ayrı ayrı çalışabilir
- Interface'ler sayesinde genişletilebilir
- Event-driven sistem

### Hollow Knight Benzeri Özellikler
- ✅ Responsive hareket sistemi
- ✅ Double Jump
- ✅ Directional Dash
- ✅ Melee punch saldırı
- ✅ Düşman collision hasarı
- ✅ Sağlık/Can sistemi
- ✅ Ölüm ve respawn

### Performans
- FixedUpdate kullanarak fizik tabanlı
- Efficient ground check
- Minimal garbage collection

## Sorun Giderme

### Karakter hareket etmiyor:
1. PlayerInput komponentinin Actions field'ının dolu olduğunu kontrol edin
2. Rigidbody2D'nin kinematic olmadığını kontrol edin
3. Input System'in aktif olduğunu kontrol edin

### Zıplama çalışmıyor:
1. Ground Check ayarlarını kontrol edin
2. Ground Layer Mask'ının doğru ayarlandığını kontrol edin
3. GroundCheckPoint pozisyonunu kontrol edin

### Dash çalışmıyor:
1. Input System'de Dash aksiyonunun tanımlı olduğunu kontrol edin
2. Dash tuş atamalarını kontrol edin (Right Shift)
3. Dash cooldown süresini kontrol edin

## Genişletme Önerileri

- Animasyon sistemi ekleme
- Ses efektleri entegrasyonu
- Parçacık efektleri
- Farklı saldırı türleri
- Yetenek sistemi
- Save/Load sistemi

Bu sistem tamamen modüler olarak tasarlanmıştır ve kolayca genişletilebilir!