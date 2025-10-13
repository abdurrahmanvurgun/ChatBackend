# Client Integration Guide

Bu doküman web (React + TypeScript) ve mobil (React Native) istemcileri için kullanılabilir, kopyalanabilir örnekler içerir. Örnekler SignalR ile gerçek zamanlı bağlantı, JWT ile kimlik doğrulama ve temel HTTP isteklerini kapsar.

## Özet
- Backend base URL: `http://localhost:5000` (development)
- SignalR hub yolu: `/chathub` (Program.cs'de tanımlandıysa kendi hub yolunuzu kullanın)
- Auth: JWT Bearer (login'den alınan token Authorization header or query string for SignalR)

---

## 1) React + TypeScript (SignalR + REST)

Aşağıdaki örnek React component'leri minimal, doğrudan kopyalanıp proje içine eklenebilir.

### 1.1 Install

```bash
npm install @microsoft/signalr axios
```

### 1.2 Auth helper

```ts
// src/lib/auth.ts
export function getAuthHeaders(token: string | null) {
  return token ? { Authorization: `Bearer ${token}` } : {};
}
```

### 1.3 SignalR bağlantısı ve Chat hook

```ts
// src/hooks/useChat.ts
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";

export function useChat(token: string | null) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [messages, setMessages] = useState<Array<{ from: string; content: string }>>([]);

  useEffect(() => {
    if (!token) {
      connection?.stop();
      setConnection(null);
      return;
    }

    const url = `${process.env.REACT_APP_API_URL || 'http://localhost:5000'}/chathub`;
    const conn = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => token })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    conn.start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR connection error', err));

    conn.on('ReceivePrivateMessage', (sender, content) => {
      setMessages(m => [...m, { from: sender, content }]);
    });

    setConnection(conn);

    return () => {
      conn.stop();
    };
  }, [token]);

  const sendPrivate = async (receiverUserId: string, message: string) => {
    if (!connection) throw new Error('Not connected');
    await connection.invoke('SendPrivateMessageByUserId', receiverUserId, message);
  };

  return { connection, messages, sendPrivate };
}
```

### 1.4 Chat Component (örnek)

```tsx
// src/components/Chat.tsx
import React, { useState } from 'react';
import { useChat } from '../hooks/useChat';
import axios from 'axios';
import { getAuthHeaders } from '../lib/auth';

export default function Chat({ token, authUserId }: { token: string | null; authUserId: string | null }) {
  const { messages, sendPrivate } = useChat(token);
  const [text, setText] = useState('');
  const [target, setTarget] = useState('');

  const send = async () => {
    if (!authUserId) return;
    // Optionally send via REST as well
    await axios.post((process.env.REACT_APP_API_URL || 'http://localhost:5000') + '/api/message/send',
      { sender: authUserId, receiver: target, content: text },
      { headers: getAuthHeaders(token) }
    );

    // Realtime via SignalR
    await sendPrivate(target, text);
    setText('');
  };

  return (
    <div>
      <h3>Chat</h3>
      <input value={target} onChange={e => setTarget(e.target.value)} placeholder="Receiver userId" />
      <ul>
        {messages.map((m, idx) => <li key={idx}><b>{m.from}</b>: {m.content}</li>)}
      </ul>

      <input value={text} onChange={e => setText(e.target.value)} />
      <button onClick={send}>Send</button>
    </div>
  );
}
```

---

## 2) React Native (Expo / Bare) örneği
React Native'de resmi SignalR client doğrudan desteklenmez; `@microsoft/signalr` çalışır fakat bazı native polyfill gerekebilir. Aşağıdaki örnek Expo ile çalışır (fetch polyfills gerekebilir).

### 2.1 Install

```bash
npm install @microsoft/signalr axios eventsource
```

### 2.2 Basit bağlantı örneği

```ts
import { HubConnectionBuilder } from '@microsoft/signalr';

export async function connectSignalR(token: string) {
  const url = `${process.env.API_URL || 'http://localhost:5000'}/chathub`;
  const connection = new HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .build();

  connection.on('ReceivePrivateMessage', (sender, content) => {
    console.log('Message', sender, content);
  });

  await connection.start();
  return connection;
}
```

> Not: Expo managed workflow'da bazı polyfill'ler (WebSocket/AbortController) gerekiyorsa `react-native-get-random-values` ve `abortcontroller-polyfill` yüklemeniz gerekebilir.

---

## 3) Swagger / OpenAPI - Basit koleksiyon
`docs/openapi.yaml` dosyası içinde temel endpoint'leri koydum; bu dosyayı Postman veya Swagger UI ile import edebilirsiniz.

---

## 4) SignalR olayları listesi
- ReceiveMessage (broadcast)
- ReceivePrivateMessage (kişiye özel)
- ReceiveGroupMessage
- GroupInviteReceived (kullanıcıya davet geldi)
- GroupMemberResponded (sahibe bildirir)
- GroupInviteCancelled

---

## 5) Güvenlik ve token
- HTTP istekleri için `Authorization: Bearer <token>` header kullanın.
- SignalR için `accessTokenFactory` (client tarafında) kullanın. Ayrıca WebSocket fallback için query string `?access_token=` kullanılabilir ama güvenlik zafiyeti oluşturabileceği için header tercih edilir.

---

## 6) Örnek Postman koleksiyonu (kısa)
- POST /api/user/register
- POST /api/user/login -> returns JWT in response
- GET /api/message/receiver/{receiverId}
- POST /api/message/send (body: sender, receiver, content)
- POST /api/groups (create)
- POST /api/groups/invite (body: groupId, targetUserId)
- POST /api/groups/respond/{groupId}?accept=true

---