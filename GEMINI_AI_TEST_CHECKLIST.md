# ? GEMINI AI CHATBOT - TEST CHECKLIST

## ?? TEST SCENARIOS

### ? 1. Basic Functionality Tests

#### Test 1.1: Simple Query
**Input:** "T? v?n ?i?n tho?i d??i 10 tri?u"  
**Expected:**
- ? Response trong 3-5s
- ? ?? xu?t 2-3 s?n ph?m
- ? Giá c? chính xác
- ? Gi?i thích ng?n g?n

#### Test 1.2: Product Recommendation
**Input:** "iPhone m?i nh?t"  
**Expected:**
- ? List iPhone models
- ? So sánh tính n?ng
- ? G?i ý phù h?p v?i budget

#### Test 1.3: Empty Context
**Input:** "S?n ph?m bán ch?y" (nh?ng DB r?ng)  
**Expected:**
- ? Response: "Hi?n t?i ch?a có s?n ph?m..."
- ? Không crash

---

### ? 2. Conversation History Tests

#### Test 2.1: Multi-turn Conversation
```
User: "?i?n tho?i camera t?t?"
Bot: "G?i ý Samsung S23..."
User: "Còn pin thì sao?"
Bot: "Samsung S23 có pin 5000mAh..." (Context aware)
```

**Expected:**
- ? Bot nh? context tr??c
- ? Không l?p l?i thông tin

#### Test 2.2: Clear History
**Action:** Click "Xóa l?ch s?"  
**Expected:**
- ? Cache cleared
- ? New conversation ID generated
- ? Bot không nh? câu h?i c?

---

### ? 3. Error Handling Tests

#### Test 3.1: Invalid API Key
**Setup:** Sai API key trong appsettings.json  
**Expected:**
- ? Error: "L?i xác th?c API Key"
- ? User-friendly message
- ? No crash

#### Test 3.2: Rate Limit
**Setup:** G?i > 15 requests trong 1 phút  
**Expected:**
- ? Auto-retry (max 3 times)
- ? Exponential backoff (2s ? 4s ? 8s)
- ? Final message: "H? th?ng ?ang quá t?i..."

#### Test 3.3: Network Error
**Setup:** Disconnect internet  
**Expected:**
- ? Error: "L?i k?t n?i m?ng"
- ? No crash
- ? Can retry when reconnected

---

### ? 4. Edge Cases

#### Test 4.1: Very Long Input
**Input:** String 1000 characters  
**Expected:**
- ? Truncated ho?c handled gracefully
- ? No timeout

#### Test 4.2: Special Characters
**Input:** "?i?n tho?i <script>alert('xss')</script>"  
**Expected:**
- ? Sanitized input
- ? Normal response
- ? No XSS

#### Test 4.3: Empty Input
**Input:** "" (empty string)  
**Expected:**
- ? Validation error: "Vui lòng nh?p câu h?i"
- ? No API call

---

### ? 5. Performance Tests

#### Test 5.1: Response Time
**Metric:** Time from send ? receive  
**Expected:**
- ? < 3s (gemini-1.5-flash)
- ? < 5s (gemini-1.5-pro)

#### Test 5.2: Concurrent Requests
**Setup:** 5 users chat cùng lúc  
**Expected:**
- ? All responses successful
- ? No race conditions
- ? Rate limit handled

#### Test 5.3: Cache Performance
**Setup:** Same conversation ID, multiple requests  
**Expected:**
- ? History loaded from cache
- ? < 100ms cache access

---

### ? 6. UI/UX Tests

#### Test 6.1: Loading State
**Action:** Send message  
**Expected:**
- ? Typing indicator shows
- ? Send button disabled
- ? Smooth animation

#### Test 6.2: Product Cards
**Action:** Get product recommendation  
**Expected:**
- ? Cards displayed with image
- ? Price formatted (10,000,000?)
- ? Click ? Navigate to product detail

#### Test 6.3: Mobile Responsive
**Device:** iPhone, Android  
**Expected:**
- ? Chat window fits screen
- ? Buttons accessible
- ? No horizontal scroll

---

## ?? TEST RESULTS TEMPLATE

| Test ID | Scenario | Status | Notes |
|---------|----------|--------|-------|
| 1.1 | Simple Query | ? | Response time: 2.5s |
| 1.2 | Product Rec | ? | 3 products suggested |
| 2.1 | Multi-turn | ? | Context maintained |
| 3.1 | Invalid Key | ? | Error message shown |
| 3.2 | Rate Limit | ? | Auto-retry worked |
| 5.1 | Performance | ? | Avg: 2.8s |

---

## ?? DEBUGGING CHECKLIST

### If chatbot không ho?t ??ng:

1. **Check API Key**
   ```bash
   # Test key v?i curl
   curl "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=YOUR_KEY"
   ```

2. **Check Model Name**
   ```json
   // appsettings.json
   "Model": "gemini-1.5-flash" // Must be exact
   ```

3. **Check Logs**
   ```
   ERROR ? "NotFound" ? Model sai
   ERROR ? "429" ? Rate limit
   ERROR ? "401" ? API key sai
   ```

4. **Test Service Directly**
   ```csharp
   var service = new GeminiAIService(configuration);
   var result = await service.GenerateResponse("Test");
   Console.WriteLine(result); // Should not crash
   ```

---

## ?? ACCEPTANCE CRITERIA

### Minimum Viable Product (MVP)

- ? User có th? g?i message
- ? Bot tr? l?i trong < 5s
- ? ?? xu?t s?n ph?m chính xác
- ? Error handling không crash app
- ? Mobile responsive

### Nice to Have

- ? Typing indicator animation
- ? Product cards v?i ?nh ??p
- ? Suggested questions
- ? Export chat history
- ? Streaming response

---

## ?? TEST SCRIPT (Manual)

### Step-by-Step Test

1. **Setup**
   - Build & Run project
   - Navigate to `/HomeCustomer`
   - Click chatbot icon (bottom right)

2. **Test Basic Flow**
   ```
   1. Type: "Xin chào"
   2. Verify: Bot responds in Vietnamese
   3. Type: "T? v?n ?i?n tho?i"
   4. Verify: Bot suggests products
   5. Click product card
   6. Verify: Navigate to detail page
   ```

3. **Test Error Scenarios**
   ```
   1. Send 20 messages rapidly
   2. Verify: Rate limit handled gracefully
   3. Enter empty message
   4. Verify: Validation error
   ```

4. **Test Persistence**
   ```
   1. Send message
   2. Refresh page
   3. Reopen chat
   4. Verify: History preserved (30min)
   ```

---

## ?? CRITICAL ISSUES TO WATCH

### ?? High Priority
- API key leaked in logs
- Crash on invalid input
- XSS vulnerability
- Rate limit không ho?t ??ng

### ?? Medium Priority
- Slow response (> 10s)
- Cache không clear
- Mobile UI broken
- Product cards không hi?n th?

### ?? Low Priority
- Typo in messages
- Animation không smooth
- Suggested questions không relevant

---

**Test By:** QA Team  
**Date:** 2024-12-08  
**Version:** 1.0.0
