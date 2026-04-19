# مرصاد — نظام الإنذار المبكر للإعلام الرقمي
## Jordan Digital Media Early Warning System

---

## 🚀 التشغيل الفوري (دقيقة واحدة)

افتح `index.html` في المتصفح — يعمل فوراً ببيانات نموذجية.

---

## 🔑 تفعيل الأخبار الحقيقية

افتح `index.html` بأي محرر نصوص (Notepad / VS Code)
ابحث عن هذا السطر:

```javascript
const CONFIG = {
  CLAUDE_API_KEY: '', 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  // ← ضع مفتاحك هنا
```

واستبدل `''` بمفتاحك من [console.anthropic.com](https://console.anthropic.com)

```javascript
CLAUDE_API_KEY: 'sk-ant-api03-...',
```

احفظ الملف وأعد فتحه في المتصفح — الأخبار الحقيقية ستظهر فوراً.

---

## 📡 ما يحدث عند إضافة المفتاح

```
عند فتح الصفحة أو الضغط على ⟳ تحديث:

١. جلب أحدث أخبار بترا + JRTV + قناة المملكة
٢. جلب RSS من 7 مصادر أردنية مستقلة
٣. Claude يحلل كل خبر ويصنفه (سياسة/اقتصاد/أمن...)
٤. Claude يحدد المشاعر (إيجابي/سلبي/محايد)
٥. Claude يحسب مؤشرات PESTEL الحقيقية
٦. Claude يقيّم مستوى المخاطر الأمنية
٧. تحديث تلقائي كل 6 ساعات
```

---

## 🌐 النشر على GitHub Pages (مجاني)

1. اذهب إلى github.com وأنشئ مستودعاً جديداً
2. ارفع `index.html`
3. Settings → Pages → Branch: main → Save
4. الرابط: `https://اسم-المستخدم.github.io/اسم-المستودع`

---

## 🗄️ Backend اختياري (Render.com)

لتحديث الأخبار كل 6 ساعات تلقائياً حتى بدون فتح المتصفح:

1. ارفع مجلد `MorsadBackend` على GitHub
2. أنشئ Web Service على Render.com (Docker)
3. أضف Environment Variables:
   - `ConnectionStrings__MorsadDb` = رابط PostgreSQL
   - `Claude__ApiKey` = مفتاحك
4. بعد النشر، ضع رابط API في:
   ```javascript
   DB_API_URL: 'https://morsad-api.onrender.com',
   ```

---

## 📋 صفحات النظام

| الصفحة | الوصف |
|--------|-------|
| لوحة التحكم | نظرة شاملة + إحصاءات + مخططات |
| الأخبار الحية | جميع الأخبار مع فلاتر |
| تحليل PESTEL | 6 مؤشرات استراتيجية |
| المخاطر الأمنية | 5 مؤشرات أمنية |
| فاحص الشائعات | تحقق من أي نص بالذكاء الاصطناعي |
| المصادر | 10 مصادر أردنية معتمدة |
| تصدير PDF | تقرير جاهز للطباعة |

---

## 💰 التكلفة

| المستوى | التكلفة |
|---------|---------|
| بدون Claude API | مجاني ١٠٠% |
| مع Claude API (haiku) | ~1-3 دولار/شهر |
| Backend على Render | مجاني (Free tier) |
| GitHub Pages | مجاني ١٠٠% |

---

## 🔧 المتطلبات

- متصفح حديث (Chrome / Firefox / Edge)
- لا يحتاج تثبيت أي شيء
- ملف واحد فقط: `index.html`

---

**المركز الوطني للأمن وإدارة الأزمات — الأردن**
